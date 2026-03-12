using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AngularDotNetChat.ApiService.Common;
using AngularDotNetChat.ApiService.Data;
using AngularDotNetChat.ApiService.DTOs;
using AngularDotNetChat.ApiService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AngularDotNetChat.ApiService.Services;

public class AuthService(AppDbContext db, IConfiguration configuration) : IAuthService
{
    public async Task<ServiceResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.UserEmail == request.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            return ServiceResponse<LoginResponseDto>.Fail("Invalid email or password.");

        var token = GenerateJwt(user);
        return ServiceResponse<LoginResponseDto>.Ok(token);
    }

    public async Task<ServiceResponse<LoginResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        if (await db.Users.AnyAsync(u => u.UserEmail == request.Email))
            return ServiceResponse<LoginResponseDto>.Fail("Email already registered.");

        var user = new User
        {
            UserEmail = request.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var token = GenerateJwt(user);
        return ServiceResponse<LoginResponseDto>.Ok(token, "Registration successful.");
    }

    private LoginResponseDto GenerateJwt(User user)
    {
        var jwtKey = configuration["Jwt:Key"]!;
        var jwtIssuer = configuration["Jwt:Issuer"]!;
        var expiresAt = DateTime.UtcNow.AddHours(8);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.UserEmail),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtIssuer,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return new LoginResponseDto(
            new JwtSecurityTokenHandler().WriteToken(token),
            user.UserEmail,
            user.UserId,
            expiresAt);
    }
}
