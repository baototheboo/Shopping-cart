using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_EF.Config;
using API_EF.Models;
using API_EF.DTO;
using OfficeOpenXml;
using System.Text;

namespace API_EF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : Controller
    {
        public PRN231DBContext dBContext;

        private readonly JwtConfig jwtConfig;

        public ProductController(PRN231DBContext dBContext, JwtConfig jwtConfig)
        {
            this.dBContext = dBContext;
            this.jwtConfig = jwtConfig;
        }

        [HttpGet("[action]")]
        public async Task<IEnumerable<Product>> Get()
        {
            var E = await dBContext.Products.ToListAsync();
            List<Product> result = new List<Product>();

            result.Reverse();
            return E;
        }

        [HttpGet("[action]/{categoryId}")]
        public async Task<IEnumerable<Product>> GetWithCategory(int categoryId)
        {
            var E = await dBContext.Products.ToListAsync();
            List<Product> result = E.FindAll(x => x.CategoryId == categoryId);

            return result;
        }

        [HttpGet("[action]")]
        public async Task<IEnumerable<Category>> GetCategory()
        {
            var E = await dBContext.Categories.ToListAsync();
            List<Product> result = new List<Product>();

            return E;
        }

        [HttpGet("[action]/{id}")]
        public async Task<Product> GetWithId(int id)
        {
            var E = dBContext.Products.ToList().FirstOrDefault(x => x.ProductId == id);

            return E;
        }


        [HttpPost("[action]")]
        public async Task<IEnumerable<Product>> FilterAndSearch(SearchFilterDTO dto)
        {
            var E = await dBContext.Products.ToListAsync();

            if (!string.IsNullOrEmpty(dto.Name))
            {
                E = E.FindAll(x => x.ProductName.Contains(dto.Name, StringComparison.CurrentCultureIgnoreCase));
            }

            if (dto.CategoryId > 0)
            {
                E = E.FindAll(x => x.CategoryId == dto.CategoryId);
            }

            E.Reverse();

            return E;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddProduct(Product product)
        {
            await dBContext.Products.AddAsync(product);

            await dBContext.SaveChangesAsync();

            return Ok();

        }

        [HttpPut("[action]")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> Update(Product product)
        {
            var E = await dBContext.Products.FirstOrDefaultAsync(x => x.ProductId == product.ProductId);

            if (E != null)
            {

                if (ModelState.IsValid)
                {
                    dBContext.Entry<Product>(E).State = EntityState.Detached;
                    dBContext.Products.Update(product);

                    await dBContext.SaveChangesAsync();

                    return Ok(product);
                }
                else
                {
                    return BadRequest();
                }

            }

            return NotFound();
        }

        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var E = await dBContext.Products.FirstOrDefaultAsync(x => x.ProductId == id);
            var E2 = await dBContext.Products.Where(x => x.CategoryId == E.CategoryId).ToListAsync();
            if (E!= null)
            {
                    var order = await dBContext.OrderDetails.Include(p=>p.Product).Where(x => x.Product.CategoryId == E.CategoryId).ToListAsync();
                    if (order != null)
                    {
                            dBContext.OrderDetails.RemoveRange(order);
                            dBContext.Products.RemoveRange(E2);
                    }
            }

            //var order = await dBContext.OrderDetails.FirstOrDefaultAsync(x => x.ProductId == id);

            //if (order != null)
            //{
            //    return BadRequest("Unable to delete employee, because there're orders of the employee");
            //}
            //dBContext.Products.Remove(E);

            await dBContext.SaveChangesAsync();

            return Ok("Record deleted successfully");
        }




        [HttpGet("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> GetHot()
        {
            var E = await dBContext.Products.ToListAsync();

            var listOrderDetail = await dBContext.OrderDetails.ToListAsync();


            float[][] count = new float[E.Count][];

            for (int i = 0; i < E.Count; i++)
            {
                count[i] = new float[2];
                count[i][0] = 0;
                count[i][1] = 0;
            }

            foreach (OrderDetail od in listOrderDetail)
            {
                int productId = od.ProductId;
                int index = -1;

                for (int i = 0; i < E.Count; i++)
                {
                    if (productId == E[i].ProductId)
                    {
                        index = i;
                        break;
                    }
                }

                if (index != -1)
                {
                    count[index][0] += od.Discount;
                    count[index][1] += 1;
                }

            }

            int[] max = new int[4];

            Array.Fill<int>(max, -1);

            float[] countMax = new float[E.Count];

            for (int i = 0; i < E.Count; i++)
            {
                if (count[i][1] != 0)
                {
                    countMax[i] = count[i][0] / count[i][1];
                }
                else
                {
                    countMax[i] = 0;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                float maxValue = -1;
                int maxIndex = -1;
                for (int j = 0; j < E.Count; j++)
                {
                    if (!max.Contains(j))
                    {
                        if (maxValue < countMax[j])
                        {
                            maxValue = countMax[j];
                            maxIndex = j;
                        }
                    }
                }

                max[i] = maxIndex;
            }

            List<Product> result = new List<Product>();
            for (int i = 0; i < max.Length; i++)
            {
                result.Add(E[max[i]]);
            }

            return Ok(result);

        }

        [HttpGet("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBestSale()
        {
            var E = await dBContext.Products.ToListAsync();

            Dictionary<int, int> count = new Dictionary<int, int>();

            for (int i = 0; i < E.Count; i++)
            {
                count.Add(E[i].ProductId, 0);
            }

            var orders = await dBContext.Orders.ToListAsync();

            var orderDetails = await dBContext.OrderDetails.ToListAsync();

            foreach (Order o in orders)
            {
                List<OrderDetail> detail = orderDetails.FindAll(x => x.OrderId == o.OrderId);

                List<int> productId = new List<int>();

                foreach (OrderDetail de in detail)
                {
                    if (!productId.Contains(de.ProductId))
                    {
                        productId.Add(de.ProductId);
                    }
                }

                foreach (int id in productId)
                {
                    count[id]++;
                }
            }

            List<Product> result = new List<Product>();
            List<int> productIds = new List<int>();

            for (int i = 0; i < 4; i++)
            {
                float maxValue = -1;
                int maxIndex = -1;
                for (int j = 0; j < E.Count; j++)
                {
                    if (!productIds.Contains(E[j].ProductId))
                    {
                        if (maxValue < count[E[j].ProductId])
                        {
                            maxValue = count[E[j].ProductId];
                            maxIndex = E[j].ProductId;
                        }
                    }
                }

                productIds.Add(maxIndex);
            }

            for (int i = 0; i < 4; i++)
            {
                result.Add(E.Find(x => x.ProductId == productIds[i]));
            }

            return Ok(result);

        }
        [HttpGet("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> GetNewItem()
        {
            var E = await dBContext.Products.ToListAsync();

            List<Product> result = new List<Product>();
            List<int> productIds = new List<int>();

            for (int i = 0; i < 4; i++)
            {
                float maxValue = -1;
                int maxIndex = -1;
                for (int j = 0; j < E.Count; j++)
                {
                    if (!productIds.Contains(E[j].ProductId))
                    {
                        if (maxValue < E[j].ProductId)
                        {
                            maxValue = E[j].ProductId;
                            maxIndex = E[j].ProductId;
                        }
                    }
                }

                productIds.Add(maxIndex);
            }

            for (int i = 0; i < 4; i++)
            {
                result.Add(E.Find(x => x.ProductId == productIds[i]));
            }

            return Ok(result);
        }
    }
}
