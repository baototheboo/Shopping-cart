
using API_EF.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.AccessControl;
using System.Security.Claims;
using System.Text;

namespace API_EF.Config
{
    public class JwtConfig
    {
        private readonly string key;

        private PRN231DBContext context;

        public JwtConfig(string key)
        {
            this.key = key;
        }

        public string Authenticate(string email, string password, PRN231DBContext context)
        {

            if(password == null || email == null)
            {
                return null;
            }

            Account user = null;

            var a = context.Accounts;

            foreach (Account u in context.Accounts.ToList()){
                if(u.Email.Equals(email) && u.Password.Equals(password))
                {
                    user = u;
                    break;
                }
            }

            if (user != null)
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                var tokenKey = Encoding.ASCII.GetBytes(key);

                SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new System.Security.Claims.ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, email),
                        new Claim(ClaimTypes.Role, user.Role + "")
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),

                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(tokenKey),
                        SecurityAlgorithms.HmacSha256Signature)

                };


                var token = tokenHandler.CreateToken(tokenDescriptor);

                return tokenHandler.WriteToken(token);
            }

            return null;
        }
    }
}
