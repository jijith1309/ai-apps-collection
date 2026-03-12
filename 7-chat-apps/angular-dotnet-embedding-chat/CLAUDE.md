# Claude Instructions — .NET 10 Project

## Architecture & Project Structure

- Always use **Controllers** for API endpoints. No minimal API endpoints.
- Use **individual files** for each class, interface, enum, and DTO (one type per file).
- Do **not** create separate Repository classes. Write all data access logic directly in the Service class.
- Organize files by feature/domain where appropriate.

## Dependency Injection

- Always use **primary constructor injection** (inject directly into the service class without a separate explicit constructor body).
- Register all services in `Program.cs` using the appropriate lifetime (`Scoped`, `Transient`, or `Singleton`).

```csharp
// Correct — primary constructor DI (no constructor body needed)
public class ProductService(AppDbContext db) : IProductService
{
    // use db directly
}
```

## Controller Guidelines

- Controller action methods must be **lean and clean** — no business logic inside controllers.
- Controllers should only: validate input, call the service, and return the response.
- Always wrap responses using `ServiceResponse<T>` (see below).

```csharp
[HttpGet("{id}")]
public async Task<ActionResult<ServiceResponse<ProductDto>>> GetProduct(int id)
{
    return Ok(await _productService.GetProductAsync(id));
}
```

## Service Class Guidelines

- All **business logic** lives in the Service class.
- `DbContext` can be used directly inside Service classes — no repository layer.
- Services must implement an interface (`IProductService`, etc.).
- XML doc comments are **not required** in service methods (keep them clean).

## API Response Wrapper

- **All API responses** must be wrapped in `ServiceResponse<T>` where `T` is a typed class/DTO.
- If `ServiceResponse<T>` does not exist in the project, **ask the user to create it** before proceeding.

Expected shape:

```csharp
public class ServiceResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}
```

## DTOs

- Use **DTOs** wherever data is passed between the API layer and service layer, or when the entity shape differs from what the client needs.
- Keep DTOs simple and flat. Avoid nesting unless necessary.

## XML Documentation Comments

- Add **XML doc comments** on all Controller action methods (include parameter descriptions and return type info).
- XML doc comments are **not required** in Service classes.

```csharp
/// <summary>
/// Retrieves a product by its ID.
/// </summary>
/// <param name="id">The unique identifier of the product.</param>
/// <returns>A <see cref="ServiceResponse{T}"/> containing the product data.</returns>
[HttpGet("{id}")]
public async Task<ActionResult<ServiceResponse<ProductDto>>> GetProduct(int id) { ... }
```

## General Best Practices

- Follow **SOLID principles** — keep classes focused and single-purpose.
- Use `async/await` for all I/O-bound operations.
- Use `CancellationToken` where appropriate.
- Prefer expression-bodied members and pattern matching where it improves readability.
- Avoid over-engineering — keep implementations simple and straightforward.
- Use `record` types for DTOs when immutability makes sense.

## Planning

- **Always show a plan before implementation.** Outline the files to be created/modified, the approach, and any assumptions before writing any code.
