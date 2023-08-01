using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API_EF.Config;
using API_EF.Models;
using API_EF.DTO;
using System.Security.Claims;
using API_EF.Config;
using API_EF.DTO;
using API_EF.Models;

namespace API_EF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly JwtConfig jwtConfig;

        private PRN231DBContext context;

        public AuthController(JwtConfig jwtConfig, PRN231DBContext context)
        {
            this.jwtConfig = jwtConfig;
            this.context = context;
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public IActionResult SignIn([FromQuery(Name = "email")] string email,
            [FromQuery(Name = "password")] string password)
        {
            var token = jwtConfig.Authenticate(email, password, context);

            if (token == null)
            {
                return Unauthorized();
            }

            return Ok(token);
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public IActionResult Register(RegisterDTO registerDTO)
        {

            Account user = registerDTO.account;

            Account user_ = context.Accounts.FirstOrDefault(x => x.Email.Equals(user.Email));

            if (user_ != null)
            {
                return BadRequest("Email exists");
            }

            string cusId = "";

            for (int i = 0; i < 5; i++)
            {
                int ran = new Random().Next(65, 91);

                char ranChar = Convert.ToChar(ran);

                cusId += ranChar;

            }

            registerDTO.customer.CustomerId = cusId;
            registerDTO.account.CustomerId = cusId;

            context.Customers.Add(registerDTO.customer);

            //string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);

            //user.Password = hashedPassword;

            context.Accounts.Add(user);

            context.SaveChanges();

            user.Customer = null;

            return Ok(user);

        }

        [AllowAnonymous]
        [HttpGet("[action]")]
        public IActionResult GetRole()
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var role = claimsIdentity.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(role);
        }

    }
}
