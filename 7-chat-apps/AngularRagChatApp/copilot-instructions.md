# Copilot Instructions - AngularRagChatApp

## Overview
This document outlines the coding standards and best practices for the AngularRagChatApp project. All code generation and modifications should follow these guidelines.

---

## 1. Architecture & Project Structure

### 1.1 Controller-Based API Endpoints
- **Always use Controllers** for all API endpoints
- Controllers should be located in the `Controllers` folder
- Use RESTful naming conventions for endpoints
- One controller per logical feature/domain

### 1.2 File Organization
Each entity/feature should have individual files:
- **Classes**: `Models/EntityName.cs`
- **Interfaces**: `Interfaces/IServiceName.cs`
- **Enums**: `Enums/EnumName.cs`
- **DTOs**: `Dtos/EntityNameDto.cs`
- **Services**: `Services/ServiceName.cs`
- **Controllers**: `Controllers/EntityNameController.cs`

---

## 2. Dependency Injection

### 2.1 Service Registration
- All services must be registered in the `ServiceCollection` (typically in `Program.cs`)
- Use interface-based registration: `services.AddScoped<IServiceName, ServiceName>()`

### 2.2 Attribute-Based Injection in Services
- **Inject dependencies directly to Service class without constructor**
- Use property-based injection with attributes
- Example:
```csharp
public class UserService
{
    [Inject]
    public IUserRepository UserRepository { get; set; }

    [Inject]
    public ApplicationDbContext DbContext { get; set; }
}
```

### 2.3 Constructor Injection in Controllers
- Controllers use standard constructor injection for services
- Keep constructor parameters clean and minimal

---

## 3. Controller Guidelines

### 3.1 Lean and Clean Action Methods
- Controller action methods should be simple and focused
- Minimal business logic in controllers
- Delegate all logic to Service classes
- Example structure:
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id)
{
    var result = await _userService.GetUserByIdAsync(id);
    return Ok(result);
}
```

### 3.2 Error Handling
- Controllers should handle HTTP status responses
- Services return status information in ServiceResponse<T>
- Controllers map ServiceResponse to appropriate HTTP responses

---

## 4. Service Class Guidelines

### 4.1 Business Logic Location
- **All business logic must be in Service classes**
- Services have direct access to DbContext
- Services can use DbContext directly without going through repositories (if not using repository pattern)
- Example:
```csharp
public class UserService : IUserService
{
    [Inject]
    public ApplicationDbContext DbContext { get; set; }

    public async Task<ServiceResponse<UserDto>> CreateUserAsync(CreateUserDto dto)
    {
        // Business logic here
        var user = new User { Name = dto.Name };
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();
        
        return new ServiceResponse<UserDto> 
        { 
            Success = true, 
            Data = MapToDto(user) 
        };
    }
}
```

### 4.2 XML Comments
- **XML comments are NOT required in Service class methods**
- Comments should focus on complex business logic when necessary

---

## 5. API Response Format

### 5.1 ServiceResponse<T> Wrapper
- **All API responses must be wrapped in ServiceResponse<T>**
- Where T is a class format (typically a DTO)
- The ServiceResponse class structure:
```csharp
public class ServiceResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
    public List<string> Errors { get; set; } = new();
}
```

### 5.2 Response Class Requirement
- If the required ServiceResponse class or response DTO is not present, **ask the user to create it** before proceeding
- Do not create response classes independently

---

## 6. Data Transfer Objects (DTOs)

### 6.1 When to Use DTOs
- Use DTOs for API requests and responses
- Use DTOs to expose limited data models
- Use DTOs for data transformation between layers
- Use DTOs to prevent circular references

### 6.2 DTO Naming Conventions
- Request DTOs: `Create{Entity}Dto`, `Update{Entity}Dto`
- Response DTOs: `{Entity}Dto`
- Each DTO in its own file in the `Dtos` folder

### 6.3 DTO Example
```csharp
// Dtos/UserDto.cs
public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

// Dtos/CreateUserDto.cs
public class CreateUserDto
{
    public string Name { get; set; }
    public string Email { get; set; }
}
```

---

## 7. XML Documentation Comments

### 7.1 Controller Methods
- **Always add XML comments to Controller methods**
- Include summary and parameter descriptions
- Example:
```csharp
/// <summary>
/// Retrieves a user by their unique identifier.
/// </summary>
/// <param name="id">The user ID to retrieve</param>
/// <returns>The user data if found, otherwise not found response</returns>
[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id)
{
    var result = await _userService.GetUserByIdAsync(id);
    return Ok(result);
}
```

### 7.2 Service Methods
- **Parameter information is NOT required in Service class**
- Only add summary comments for complex methods if needed
- Focus on complex business logic explanations

### 7.3 DTO and Model Classes
- Add XML comments to public properties
- Describe the purpose of each property

---

## 8. Best Practices & Implementation

### 8.1 Code Quality
- Keep it simple and readable
- Follow SOLID principles
- Use async/await for all I/O operations
- Use LINQ for data queries

### 8.2 Validation
- Validate input in Service classes
- Use data annotations or FluentValidation where appropriate
- Return proper error messages in ServiceResponse

### 8.3 Exception Handling
- Use try-catch in Services
- Return errors in ServiceResponse.Errors
- Log exceptions appropriately

### 8.4 Database Access
- Use DbContext directly in Services
- Use async methods (SaveChangesAsync, ToListAsync, FirstOrDefaultAsync)
- Keep queries optimized and performant

```csharp
public async Task<ServiceResponse<List<UserDto>>> GetAllUsersAsync()
{
    try
    {
        var users = await DbContext.Users
            .AsNoTracking()
            .ToListAsync();
        
        return new ServiceResponse<List<UserDto>> 
        { 
            Success = true, 
            Data = MapToDtoList(users) 
        };
    }
    catch (Exception ex)
    {
        return new ServiceResponse<List<UserDto>> 
        { 
            Success = false, 
            Message = "Error retrieving users",
            Errors = new List<string> { ex.Message }
        };
    }
}
```

---

## 9. Implementation Workflow

### 9.1 Before Every Implementation
- **ALWAYS show your plan before writing code**
- Describe the structure you will create
- List files and classes that will be created/modified
- Wait for approval before proceeding

### 9.2 Implementation Steps
1. Present the detailed plan
2. Get approval
3. Create/modify files according to the plan
4. Ensure all guidelines are followed
5. Verify the implementation

---

## 10. Common Checklist

- [ ] Using Controllers for all API endpoints
- [ ] Dependency injection properly configured
- [ ] Controller methods are lean and focused
- [ ] All business logic in Service classes
- [ ] DbContext used directly in Services
- [ ] All API responses wrapped in ServiceResponse<T>
- [ ] DTOs created where appropriate
- [ ] Separate files for Classes, Interfaces, Enums, DTOs
- [ ] XML comments added to Controller methods
- [ ] Best practices and simple implementations followed
- [ ] Plan presented before implementation

---

## Questions?
If anything is unclear or if you need to create missing common classes (like ServiceResponse<T>), please ask before proceeding with implementation.
