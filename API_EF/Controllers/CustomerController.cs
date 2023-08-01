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
    public class CustomerController : Controller
    {
        public PRN231DBContext dBContext;

        private readonly JwtConfig jwtConfig;

        public CustomerController(PRN231DBContext dBContext, JwtConfig jwtConfig)
        {
            this.dBContext = dBContext;
            this.jwtConfig = jwtConfig;
        }

        [HttpGet("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCustomerName()
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var email = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;

            if (email != null)
            {
                Account account = dBContext.Accounts.FirstOrDefault(x => x.Email.Equals(email));

                var customer = dBContext.Customers.FirstOrDefault(x => x.CustomerId.Equals(account.CustomerId));
                customer.Accounts = null;
                return Ok(customer);
            }
            return NotFound();
        }

        [HttpPut("[action]")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> UpdateAccount(RegisterDTO registerDTO)
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var email = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;

            if (email != null)
            {
                Account account = dBContext.Accounts.FirstOrDefault(x => x.Email.Equals(email));

                var customer = dBContext.Customers.FirstOrDefault(x => x.CustomerId.Equals(account.CustomerId));

                account.Email = registerDTO.account.Email;
                account.Password = registerDTO.account.Password;

                customer.CompanyName = registerDTO.customer.CompanyName;
                customer.ContactName = registerDTO.customer.ContactName;
                customer.ContactTitle = registerDTO.customer.ContactTitle;
                customer.Address = registerDTO.customer.Address;

                dBContext.Accounts.Update(account);
                dBContext.Customers.Update(customer);

                dBContext.SaveChanges();

                registerDTO.account = account;
                registerDTO.customer = customer;

                customer.Accounts = null;
                account.Customer = null;
                return Ok(registerDTO);
            }

            return NotFound();
        }

        [HttpDelete("[action]/{accountId}")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> DeleteAccount(int accountId)
        {

            Account account = dBContext.Accounts.FirstOrDefault(x => x.AccountId.Equals(accountId));

            var customer = dBContext.Customers.FirstOrDefault(x => x.CustomerId.Equals(account.CustomerId));

            var orders = dBContext.Orders.Include(x => x.OrderDetails).ToList().FindAll(x => x.CustomerId.Equals(customer.CustomerId));

            List<OrderDetail> orderDetails = new List<OrderDetail>();

            foreach (Order o in orders)
            {
                orderDetails.AddRange(o.OrderDetails);
            }

            dBContext.OrderDetails.RemoveRange(orderDetails);
            dBContext.Orders.RemoveRange(orders);
            dBContext.Customers.Remove(customer);
            dBContext.Accounts.Remove(account);

            dBContext.SaveChanges();

            return Ok();
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> GetTotalCustomer()
        {
            var E = await dBContext.Customers.ToListAsync();

            return Ok(E.Count);

        }

        [HttpGet("[action]")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> GetTotalGuests()
        {
            var E = await dBContext.Customers.Include(x => x.Accounts).ToListAsync();

            int count = 0;

            foreach(Customer c in E)
            {
                if(c.Accounts.Count > 0)
                {
                    count++;
                }
            }

            return Ok(E.Count - count);

        }

        


    }
}
