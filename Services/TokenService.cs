using BusinessObjects.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Services.ViewModels;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class TokenService
    {

        private readonly JwtSettings _jwtSettings;

        public TokenService(IOptions<JwtSettings> jwtOptions)
        {
            _jwtSettings = jwtOptions.Value;
        }

        public string GenerateToken(User user)
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("UserID", user.UserId.ToString()),
            new Claim("Role", user.Role.ToString() )
            //new Claim(ClaimTypes.Role, GetRoleName(user.Role) )
            //new Claim("Role", GetRoleName(user.Role) )
            
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GetRoleName(int role)
        {
            return role switch
            {
                1 => "Admin",
                2 => "Guest",
                3 => "Student",
                4 => "Teacher",
                _ => "Unknown" // fallback nếu role không hợp lệ
            };

        }
    }
}
