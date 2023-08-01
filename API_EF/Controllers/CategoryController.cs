using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_EF.Config;
using API_EF.Models;

namespace API_EF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : Controller
    {
        private readonly JwtConfig jwtConfig;

        private PRN231DBContext context;

        public CategoryController(JwtConfig jwtConfig, PRN231DBContext context)
        {
            this.jwtConfig = jwtConfig;
            this.context = context;
        }

        [AllowAnonymous]
        [HttpGet("[action]/{cateId}")]
        public async Task<IActionResult> GetCateById(int cateId)
        {
            var E = await context.Categories.FirstOrDefaultAsync(x => x.CategoryId == cateId);

            if (E != null)
            {
                Category c = E;
                c.Picture = null;
                return Ok(c); 
            }
            else
            {
                return NotFound();
            }


        }
    }
}
