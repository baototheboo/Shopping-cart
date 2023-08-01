using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_EF.Config;
using API_EF.Models;

namespace API_EF.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : Controller
    {
        private readonly JwtConfig jwtConfig;

        private PRN231DBContext context;

        public EmployeeController(JwtConfig jwtConfig, PRN231DBContext context)
        {
            this.jwtConfig = jwtConfig;
            this.context = context;
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> getAll()
        {
            var E = await context.Employees.ToListAsync();

            return Ok(E);
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> getWithEmail(string email)
        {
            var E = await context.Accounts.Include(x => x.Employee).FirstOrDefaultAsync(x => x.Email.Equals(email));

            if (E == null)
            {
                return NotFound();
            }
            var result = E.Employee;
            result.Accounts = null;

            return Ok(result);

        }

    }

}
