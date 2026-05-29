import { HubConnection, HubConnectionBuilder, LogLevel, HubConnectionState } from '@microsoft/signalr';

export interface LotChangedPayload {
  lotId: string;
}

export interface AuctionHubHandlers {
  onLotChanged?: (e: LotChangedPayload) => void;
  onConnectionLost?: () => void;
}

/**
 * Singleton SignalR client for the auction hub.
 *
 * Thin-push model: server emits a single <c>LotChanged</c> event with only the lot id; the
 * client invalidates its cached lot + bid history queries and re-fetches authoritative state
 * from REST. No payload state is duplicated on the client.
 *
 * Lifecycle:
 *  - One HubConnection per app, lazily started on first subscription.
 *  - Never torn down by the client — survives route changes.
 *  - One event listener registered once at construction; dispatches to per-lot handlers via
 *    the subscribers Map.
 */

const HUB_URL = '/auctionHub';

let connection: HubConnection | null = null;
const subscribers = new Map<string, AuctionHubHandlers>();
const joinedLots = new Set<string>();
let connectionLostNotified = false;

function buildConnection(): HubConnection {
  const conn = new HubConnectionBuilder()
    .withUrl(HUB_URL, { withCredentials: true })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build();

  conn.on('LotChanged', (payload: LotChangedPayload) => {
    subscribers.get(payload.lotId)?.onLotChanged?.(payload);
  });

  conn.onreconnected(async () => {
    connectionLostNotified = false;
    await Promise.allSettled(
      Array.from(joinedLots).map((lotId) =>
        conn.invoke('JoinLotGroup', lotId).catch((err) => {
          console.warn(`[auctionHub] re-join failed for ${lotId}`, err);
        }),
      ),
    );
  });

  conn.onclose((err) => {
    if (connectionLostNotified) return;
    connectionLostNotified = true;
    if (err) console.warn('[auctionHub] connection closed', err);
    for (const handlers of subscribers.values()) {
      handlers.onConnectionLost?.();
    }
  });

  return conn;
}

function getConnection(): HubConnection {
  if (connection === null) {
    connection = buildConnection();
  }
  return connection;
}

async function ensureStarted(): Promise<HubConnection> {
  const conn = getConnection();
  if (conn.state === HubConnectionState.Disconnected) {
    await conn.start();
  }
  return conn;
}

export async function subscribeToLot(lotId: string, handlers: AuctionHubHandlers): Promise<void> {
  subscribers.set(lotId, handlers);

  try {
    const conn = await ensureStarted();
    if (!joinedLots.has(lotId)) {
      await conn.invoke('JoinLotGroup', lotId);
      joinedLots.add(lotId);
    }
  } catch (err) {
    console.warn(`[auctionHub] subscribeToLot(${lotId}) failed`, err);
    handlers.onConnectionLost?.();
  }
}

export async function unsubscribeFromLot(lotId: string): Promise<void> {
  subscribers.delete(lotId);
  joinedLots.delete(lotId);

  const conn = connection;
  if (conn === null || conn.state !== HubConnectionState.Connected) {
    return;
  }
  try {
    await conn.invoke('LeaveLotGroup', lotId);
  } catch (err) {
    console.warn(`[auctionHub] unsubscribeFromLot(${lotId}) failed`, err);
  }
}
