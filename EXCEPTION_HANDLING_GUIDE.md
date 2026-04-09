# Exception Handling Refactoring Guide

## Problem: Traditional Exception Throwing ❌
Previously, your code was throwing exceptions directly:
```csharp
if (customer == null)
{
    var failure = new FluentValidation.Results.ValidationFailure("Customer", "Customer not found");
    throw new FluentValidation.ValidationException(new[] { failure });
}
```

## Solution: Result Pattern with FluentResults ✅

### 1. **Use Result<T> Return Types**
Instead of throwing, return `Result<T>` with errors:
```csharp
public async Task<Result<CustomerDTO>> GetCustomerAsync(Ulid id, CancellationToken ct)
{
    var customer = await _repository.GetByIdAsync(id, ct);
    
    if (customer == null)
    {
        return Result.Fail(new NotFoundError("Customer", id.ToString()));
    }
    
    return Result.Ok(new CustomerDTO { /* ... */ });
}
```

### 2. **Custom Error Types (Already Created)**
We've created specialized error classes for different scenarios:

#### **NotFoundError** - For missing resources
```csharp
return Result.Fail(new NotFoundError("Customer", customerId));
```

#### **ValidationError** - For validation failures
```csharp
var errors = new Dictionary<string, string[]>
{
    { "Email", new[] { "Invalid email format" } },
    { "Phone", new[] { "Phone is required" } }
};
return Result.Fail(new ValidationError(errors));
```

#### **ExternalServiceError** - For external service failures
```csharp
return Result.Fail(ExternalServiceError.InventoryServiceError(statusCode, errorDetails));
```

#### **UnauthorizedError** - For authentication failures
```csharp
return Result.Fail(UnauthorizedError.Unauthorized("Invalid credentials"));
```

### 3. **Handling Results in Service Calls**
When calling methods that return `Result<T>`:

**Before (Throwing Exceptions):**
```csharp
var inventory = await _inventoryService.CreateInventoryAsync(...);
if (inventory != null)
{
    // process inventory
}
```

**After (Using Result Pattern):**
```csharp
var inventoryResult = await _inventoryService.CreateInventoryAsync(...);

if (inventoryResult.IsSuccess)
{
    var inventory = inventoryResult.Value;
    // process inventory
}
else
{
    // Errors are in inventoryResult.Errors
    return Result.Fail(inventoryResult.Errors);
}
```

### 4. **Automatic Error Handling via Middleware**
Your `GlobalExceptionHandler` catches any exceptions and converts them to JSON responses:

- **ValidationException** → 400 Bad Request with error details
- **UnauthorizedException** → 401 Unauthorized
- **NotFoundException** → 404 Not Found
- Any other exception → 500 Internal Server Error with logging

### 5. **Best Practices**

✅ **DO:**
- Return `Result<T>` from all service/handler methods
- Use specific error types (NotFoundError, ValidationError, etc.)
- Check `result.IsSuccess` before accessing `result.Value`
- Compose errors from downstream calls with `Result.Fail(errors)`

❌ **DON'T:**
- Throw generic `Exception` or `BadHttpRequestException`
- Use `throw new FluentValidation.ValidationException()`
- Ignore errors - always propagate them with `Result.Fail()`
- Mix Exception throwing with Result pattern

### 6. **Refactoring Checklist**

For each method or handler:
- [ ] Change return type to `Result<T>`
- [ ] Replace `throw new Exception()` with `return Result.Fail(error)`
- [ ] Check for null/invalid conditions and return errors
- [ ] Update all callers to handle `Result<T>`
- [ ] Test error scenarios

### 7. **Example Refactoring**

**Original Code:**
```csharp
public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken ct)
{
    var user = await _userRepository.GetByUsernameAsync(request.Username, ct);
    if (user == null)
    {
        throw new FluentValidation.ValidationException(
            new[] { new FluentValidation.Results.ValidationFailure("User", "User not found") });
    }
    user.DeleteUser();
    _userRepository.Update(user);
    return Unit.Value;
}
```

**Refactored Code:**
```csharp
public async Task<Result<Ulid>> Handle(DeleteUserCommand request, CancellationToken ct)
{
    var user = await _userRepository.GetByUsernameAsync(request.Username, ct);
    if (user == null)
    {
        return Result.Fail(new NotFoundError("User", request.Username));
    }
    user.DeleteUser();
    _userRepository.Update(user);
    return Result.Ok(user.Id);
}
```

### 8. **Files Already Updated**
✅ Created:
- `NotFoundError.cs` - for missing resources
- `ValidationError.cs` - for validation failures  
- `ExternalServiceError.cs` - for external service errors

✅ Refactored:
- `DeleteCustomerCommandHandler.cs`
- `DeleteUserCommandHandler.cs`
- `UpdateUserCommandHandler.cs`
- `InventoryInternalClient.cs`
- `IInventoryService.cs`
- `ProductService.cs`

## Benefits

1. **No Exception Overhead** - Exceptions are expensive; Results are lightweight
2. **Explicit Error Handling** - Errors are visible in method signatures
3. **Type-Safe** - Compile-time guarantees about what can go wrong
4. **Better Testing** - Easier to assert on error cases
5. **Cleaner Code** - No try-catch blocks everywhere
6. **Composable** - Errors can be combined and transformed easily
