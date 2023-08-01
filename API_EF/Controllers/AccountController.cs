using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_EF.Config;
using API_EF.DTO;
using API_EF.Models;
using System.Security.Claims;

namespace API_EF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly JwtConfig jwtConfig;

        private PRN231DBContext context;

        public AccountController(JwtConfig jwtConfig, PRN231DBContext context)
        {
            this.jwtConfig = jwtConfig;
            this.context = context;
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> GetPersonalInfo()
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var email = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;

            Account account;

            if (email != null)
            {
                account = await context.Accounts.FirstOrDefaultAsync(x => x.Email.Equals(email));

                var customer = await context.Customers.FirstOrDefaultAsync(x => x.CustomerId.Equals(account.CustomerId));

                if (customer != null)
                {
                    account.Customer = null;
                    customer.Accounts = null;

                    RegisterDTO dto = new RegisterDTO();
                    dto.account = account;
                    dto.customer = customer;

                    return Ok(dto);
                }
            }

            return BadRequest();
        }

        [HttpPut("[action]")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> EditPersonalInfo(RegisterDTO dto)
        {
            string token = null;
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var email = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;

            Account account;

            if (email != null)
            {
                account = await context.Accounts.FirstOrDefaultAsync(x => x.Email.Equals(email));

                var customer = await context.Customers.FirstOrDefaultAsync(x => x.CustomerId.Equals(account.CustomerId));

                if (customer != null)
                {
                    if (!account.Email.Equals(dto.account.Email))
                    {

                        Account user_ = context.Accounts.FirstOrDefault(x => x.Email.Equals(dto.account.Email));

                        if (user_ != null)
                        {
                            return BadRequest("Email exists");
                        }

                        account.Email = dto.account.Email;
                        context.Accounts.Update(account);

                        await context.SaveChangesAsync();

                        token = jwtConfig.Authenticate(dto.account.Email, account.Password, context);
                    }

                    customer.CompanyName = dto.customer.CompanyName;
                    customer.ContactTitle = dto.customer.ContactTitle;
                    customer.ContactName = dto.customer.ContactName;
                    customer.Address = dto.customer.Address;

                    await context.SaveChangesAsync();

                    return Ok(token);
                }
            }

            return BadRequest();
        }

    }
}
