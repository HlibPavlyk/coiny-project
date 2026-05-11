import { HubConnection, HubConnectionBuilder, LogLevel, HubConnectionState } from '@microsoft/signalr';

export interface BidPlacedPayload {
  lotId: string;
  currentPriceUahKopiykas: number;
  bidCount: number;
  leaderDisplayName: string;
}

export interface AuctionExtendedPayload {
  lotId: string;
  newEndsAt: string;
}

export interface AuctionClosedPayload {
  lotId: string;
  finalPriceUahKopiykas: number | null;
  winnerDisplayName: string | null;
}

export interface AuctionHubHandlers {
  onBidPlaced?: (e: BidPlacedPayload) => void;
  onAuctionExtended?: (e: AuctionExtendedPayload) => void;
  onAuctionClosed?: (e: AuctionClosedPayload) => void;
  onConnectionLost?: () => void;
}

/**
 * Singleton SignalR client for the auction hub.
 *
 * Lifecycle per /docs/03-frontend-structure.md:
 *  - One HubConnection per app, lazily started on first subscription.
 *  - Never torn down by the client — survives route changes.
 *  - Three event names ("BidPlaced", "AuctionExtended", "AuctionClosed") are registered once at
 *    construction; payloads dispatch to per-lot handlers via the subscribers Map.
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

  conn.on('BidPlaced', (payload: BidPlacedPayload) => {
    subscribers.get(payload.lotId)?.onBidPlaced?.(payload);
  });
  conn.on('AuctionExtended', (payload: AuctionExtendedPayload) => {
    subscribers.get(payload.lotId)?.onAuctionExtended?.(payload);
  });
  conn.on('AuctionClosed', (payload: AuctionClosedPayload) => {
    subscribers.get(payload.lotId)?.onAuctionClosed?.(payload);
  });

  conn.onreconnected(async () => {
    connectionLostNotified = false;
    // Re-join every lot the app is still subscribed to. Per-lot failures log but don't throw.
    await Promise.allSettled(
      Array.from(joinedLots).map((lotId) =>
        conn.invoke('JoinLotGroup', lotId).catch((err) => {
          console.warn(`[auctionHub] re-join failed for ${lotId}`, err);
        }),
      ),
    );
  });

  conn.onclose((err) => {
    // Reconnect attempts have been exhausted — surface to every active subscriber so they can
    // switch to a polling fallback. Avoid duplicate notifications across consecutive closes.
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

/**
 * Subscribe to live updates for a lot. Lazily starts the connection on the first call.
 * Replaces any prior handlers for the same lotId — caller must unsubscribe when the
 * subscription target changes.
 */
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

/**
 * Unsubscribe from a lot. Best-effort — server group leave failures are logged but don't throw.
 * The connection itself stays alive for the rest of the app.
 */
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
