using AuthenticationApi.Application.DTOs;
using AuthenticationApi.Application.Interfaces;
using AuthenticationApi.Domain.EntityModel;
using AuthenticationApi.Infrastructure.Database;
using eCommerce.SharedLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthenticationApi.Infrastructure.Repositories
{
    public class UserRepository(AuthenticationDbContext context, IConfiguration config) : IUser
    {
        private async Task<AppUserModel?> GetUserByEmail(string email)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) { return null; }
            return user!;
        }
        public async Task<GetUserDTO> GetUser(int userId)
        {
            var user = await context.Users.FindAsync(userId);
            if (user is not null)
            {
                return new GetUserDTO(
                    user.Id,
                    user.Name!,
                    user.TelephoneNumber!,
                    user.Address!,
                    user.Email!,
                    user.Role!);

            }
            return null!;
        }

        public async Task<Response> Login(LoginDTO loginDTO)
        {
            var getUser = await GetUserByEmail(loginDTO.Email);
            if (getUser is null) 
            {
                return new Response(false, "Invalid credentials");
            }
            bool verifyPassword = BCrypt.Net.BCrypt.Verify(loginDTO.Password, getUser.Password);
            if (!verifyPassword)
            { return new Response(false, "Invalid credentials."); }

            string token = GenerateToken(getUser);
            return new Response(true, token);
        }

        private string GenerateToken(AppUserModel user)
        {
            var key = Encoding.UTF8.GetBytes(config.GetSection("Authentication:Key").Value!);
            var securityKey = new SymmetricSecurityKey(key);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new (ClaimTypes.Name, user.Name!),
                new (ClaimTypes.Email, user.Email!)
            };
            if (!string.IsNullOrEmpty(user.Role) || !Equals("string", user.Role))
                claims.Add(new(ClaimTypes.Role, user.Role!));

            var token = new JwtSecurityToken(
                issuer: config["Authentication:Issuer"],
                audience: config["Authetication:Audience"],
                claims: claims,
                expires: null,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<Response> Register(AppUserDTO appUserDTO)
        {
            var getUser = await GetUserByEmail(appUserDTO.Email);
            if (getUser is not null) { return new Response(false, "account with this email is alredy registerd, please use diferent email."); }

            var result = context.Users.Add(new AppUserModel()
            {
                Name = appUserDTO.Name,
                Email = appUserDTO.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(appUserDTO.Password),
                Address = appUserDTO.Address,
                TelephoneNumber = appUserDTO.TelephoneNumber,
                Role = appUserDTO.Role
            });
            await context.SaveChangesAsync();
            if (result.Entity.Id > 0)
            {
                return new Response(true, "User registered successfully");
            }
            return new Response(false, "Invalid data provided");
        }
    }
}
