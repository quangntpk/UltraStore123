using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UltraStrore.Models.ViewModels;
using UltraStrore.Repository;

namespace UltraStrore.Utils
{
    public class JwtTokenGenerator : IJwtTokenServices
    {
        private readonly IConfiguration _configuration;
        public JwtTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(NguoiDungView user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.TaiKhoan),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.MaNguoiDung),
                new Claim(ClaimTypes.Role, user.VaiTro.ToString()) // VaiTro: 0=Admin, 1=Nhân viên, 2=User
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiryInMinutes = int.Parse(_configuration["Jwt:ExpiryInMinutes"]);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(expiryInMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
