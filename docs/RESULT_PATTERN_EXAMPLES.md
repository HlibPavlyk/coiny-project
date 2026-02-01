# Result Pattern - Приклади використання

## Базове використання

### 1. Простий приклад - повернення помилки

```csharp
public async Task<Result<Album>> GetAlbumByIdAsync(Guid id)
{
    var album = await _unitOfWork.Albums.GetByIdAsync(id);

    if (album is null)
        return Result.Failure<Album>(Error.NotFound($"Album with ID '{id}' was not found"));

    return Result.Success(album);
}
```

### 2. Валідація перед операцією

```csharp
public async Task<Result<Album>> ActivateAlbumAsync(Guid id, Guid userId)
{
    var album = await _unitOfWork.Albums.GetByIdAsync(id);

    if (album is null)
        return Result.Failure<Album>(Error.NotFound($"Album '{id}' not found"));

    if (album.UserId != userId)
        return Result.Failure<Album>(Error.Forbidden("You are not the owner of this album"));

    if (album.Status == AlbumStatus.Active)
        return Result.Failure<Album>(Error.Conflict("Album is already active"));

    album.Status = AlbumStatus.Active;
    await _unitOfWork.SaveChangesAsync();

    return Result.Success(album);
}
```

### 3. Використання з Extension Methods

```csharp
public async Task<Result<AlbumGetDto>> GetAlbumDtoAsync(Guid id)
{
    var album = await _unitOfWork.Albums.GetByIdAsync(id);

    return album
        .ToResult($"Album '{id}' not found")  // Конвертує null в Result.Failure
        .Map(_mapper.Map<AlbumGetDto>);       // Маппінг якщо успіх
}
```

### 4. Chaining з Ensure

```csharp
public async Task<Result<Album>> ApproveAlbumAsync(Guid albumId, Guid moderatorId)
{
    var album = await _unitOfWork.Albums.GetByIdAsync(albumId);
    var moderator = await _unitOfWork.Users.GetByIdAsync(moderatorId);

    return album
        .ToResult($"Album '{albumId}' not found")
        .Ensure(a => a.Status == AlbumStatus.NotApproved,
                "Album must be in NotApproved status to be approved")
        .Ensure(a => moderator is not null,
                "Moderator not found");
}
```

### 5. AI Moderation - handling external service failures

```csharp
public async Task<Result<AiModerationResult>> AnalyzeAlbumElementAsync(Guid elementId)
{
    try
    {
        var element = await _unitOfWork.AlbumElements.GetByIdAsync(elementId);

        if (element is null)
            return Result.Failure<AiModerationResult>(
                Error.NotFound($"Album element '{elementId}' not found"));

        var aiResult = await _azureAiService.AnalyzeImageAsync(element.ImageUrl);

        if (aiResult.ConfidenceScore < 0)
            return Result.Failure<AiModerationResult>(
                Error.Failure("AI service returned invalid confidence score"));

        return Result.Success(aiResult);
    }
    catch (Exception ex)
    {
        return Result.Failure<AiModerationResult>(
            Error.Failure($"AI moderation failed: {ex.Message}"));
    }
}
```

### 6. Trust Score validation

```csharp
public async Task<Result> CreateListingAsync(CreateListingDto dto, Guid userId)
{
    var user = await _unitOfWork.Users.GetByIdAsync(userId);

    if (user is null)
        return Result.Failure(Error.NotFound("User not found"));

    // Перевірка trust score
    if (user.TrustScore < 10)
        return Result.Failure(
            Error.Forbidden($"Your trust score ({user.TrustScore}) is too low. Minimum required: 10"));

    var element = await _unitOfWork.AlbumElements.GetByIdAsync(dto.AlbumElementId);

    if (element is null)
        return Result.Failure(Error.NotFound("Album element not found"));

    if (element.UserId != userId)
        return Result.Failure(Error.Forbidden("You don't own this element"));

    if (element.ListingId is not null)
        return Result.Failure(Error.Conflict("Element is already listed"));

    // Create listing...
    return Result.Success();
}
```

### 7. Controller usage - Match pattern

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetAlbum(Guid id)
{
    var result = await _albumService.GetAlbumByIdAsync(id);

    return result.Match<IActionResult>(
        onSuccess: album => Ok(album),
        onFailure: error => error.Type switch
        {
            ErrorType.NotFound => NotFound(new { error.Message }),
            ErrorType.Forbidden => Forbid(),
            ErrorType.Validation => BadRequest(new { error.Message }),
            ErrorType.Conflict => Conflict(new { error.Message }),
            _ => StatusCode(500, new { error.Message })
        });
}
```

### 8. Controller usage - простіший варіант

```csharp
[HttpPost("activate/{id}")]
public async Task<IActionResult> ActivateAlbum(Guid id)
{
    var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    var result = await _albumService.ActivateAlbumAsync(id, userId);

    if (result.IsFailure)
        return HandleError(result.Error);

    return Ok(result.Value);
}

private IActionResult HandleError(Error error)
{
    return error.Type switch
    {
        ErrorType.NotFound => NotFound(new { error.Message }),
        ErrorType.Forbidden => StatusCode(403, new { error.Message }),
        ErrorType.Validation => BadRequest(new { error.Message }),
        ErrorType.Conflict => Conflict(new { error.Message }),
        ErrorType.Unauthorized => Unauthorized(new { error.Message }),
        _ => StatusCode(500, new { error.Message })
    };
}
```

## Автоматичний трекінг джерела помилки

Кожна помилка автоматично записує де вона виникла (файл, рядок, метод):

```csharp
public async Task<Result<Album>> GetAlbumAsync(Guid id)
{
    var album = await _repo.GetByIdAsync(id);

    if (album is null)
        return Result.Failure<Album>(Error.NotFound("Album not found"));
        // Автоматично записується: AlbumService.cs:42 in GetAlbumAsync

    return Result.Success(album);
}
```

### Логування помилок:

```csharp
if (result.IsFailure)
{
    _logger.LogError("Operation failed: {Error}", result.Error.GetDetailedMessage());
    // Output: "Operation failed: Album not found (at AlbumService.cs:42 in GetAlbumAsync)"
}
```

### Детальна інформація:

```csharp
var error = result.Error;
Console.WriteLine($"Message: {error.Message}");
Console.WriteLine($"Type: {error.Type}");
Console.WriteLine($"Source: {error.Source}");      // Full file path
Console.WriteLine($"Line: {error.Line}");          // Line number
Console.WriteLine($"Method: {error.MemberName}");  // Method name
Console.WriteLine($"Details: {error.GetDetailedMessage()}");
```

## Типи помилок та їх використання

### NotFound - 404
Використовуйте коли ресурс не знайдено:
```csharp
Error.NotFound("Album not found")
Error.NotFound($"User with ID '{userId}' does not exist")
// Автоматично додається: AlbumService.cs:156 in GetAlbumByIdAsync
```

### Validation - 400
Використовуйте для бізнес-правил та валідації:
```csharp
Error.Validation("Album name cannot be empty")
Error.Validation("Bid amount must be greater than current bid")
Error.Validation("Trust score is too low for this operation")
```

### Forbidden - 403
Використовуйте коли користувач не має прав:
```csharp
Error.Forbidden("You are not the owner of this album")
Error.Forbidden("Only moderators can approve albums")
Error.Forbidden("Insufficient trust score")
```

### Conflict - 409
Використовуйте для конфліктів стану:
```csharp
Error.Conflict("Album is already active")
Error.Conflict("Element is already listed in marketplace")
Error.Conflict("Auction has already ended")
```

### Unauthorized - 401
Використовуйте для проблем аутентифікації:
```csharp
Error.Unauthorized("Invalid credentials")
Error.Unauthorized("Token has expired")
```

### Failure - 500
Використовуйте для технічних помилок:
```csharp
Error.Failure("Database connection failed")
Error.Failure($"AI service error: {exception.Message}")
Error.Failure("Unable to process image")
```

## Переваги цього підходу

1. ✅ **Явна обробка помилок** - немає hidden exceptions
2. ✅ **Гнучкість** - можна передати будь-яке повідомлення
3. ✅ **Типобезпека** - компілятор змушує обробляти помилки
4. ✅ **Легко тестувати** - просто перевіряйте IsSuccess/IsFailure
5. ✅ **Clean API** - контролери чистіші без try-catch
6. ✅ **Functional approach** - можна chain операції

## Приклад комплексного workflow

```csharp
public async Task<Result<TransactionDto>> PurchaseItemAsync(Guid listingId, Guid buyerId)
{
    // Validate listing exists and is active
    var listing = await _unitOfWork.Listings.GetByIdAsync(listingId);
    if (listing is null)
        return Result.Failure<TransactionDto>(Error.NotFound("Listing not found"));

    if (listing.Status != ListingStatus.Active)
        return Result.Failure<TransactionDto>(Error.Validation("Listing is not active"));

    // Validate buyer
    var buyer = await _unitOfWork.Users.GetByIdAsync(buyerId);
    if (buyer is null)
        return Result.Failure<TransactionDto>(Error.NotFound("Buyer not found"));

    if (listing.SellerId == buyerId)
        return Result.Failure<TransactionDto>(Error.Validation("Cannot buy your own listing"));

    // Check trust score
    if (buyer.TrustScore < listing.RequiredTrustLevel)
        return Result.Failure<TransactionDto>(
            Error.Forbidden($"Trust score {buyer.TrustScore} is below required {listing.RequiredTrustLevel}"));

    // Create transaction
    var transaction = new Transaction
    {
        ListingId = listingId,
        BuyerId = buyerId,
        SellerId = listing.SellerId,
        Amount = listing.FixedPrice,
        Type = TransactionType.FixedPriceSale,
        Status = TransactionStatus.Pending
    };

    await _unitOfWork.Transactions.AddAsync(transaction);
    listing.Status = ListingStatus.Sold;

    await _unitOfWork.SaveChangesAsync();

    var dto = _mapper.Map<TransactionDto>(transaction);
    return Result.Success(dto);
}
```