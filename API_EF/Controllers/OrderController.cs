using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_EF.Config;
using API_EF.DTO;
using API_EF.Models;
using System.Globalization;
using System.Security.Claims;

namespace API_EF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : Controller
    {
        public PRN231DBContext dBContext;

        private readonly JwtConfig jwtConfig;

        public OrderController(PRN231DBContext dBContext, JwtConfig jwtConfig)
        {
            this.dBContext = dBContext;
            this.jwtConfig = jwtConfig;
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> GetAllOrder()
        {
            var E = await dBContext.Orders.Include(x => x.OrderDetails).Include(x => x.Employee)
                .Include(x => x.Customer).ToListAsync();

            foreach (Order o in E)
            {
                foreach (OrderDetail od in o.OrderDetails)
                {
                    od.Order = null;
                }

                if (o.Employee != null)
                {
                    o.Employee.Orders = null;
                }

                if (o.Customer != null)
                {
                    o.Customer.Orders = null;
                }
            }

            var A = E.OrderBy(x => x.OrderDate);
            var result = A.Reverse();

            return Ok(result);
        }

        [HttpGet("[action]/{id}")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var E = await dBContext.Orders.FirstOrDefaultAsync(x => x.OrderId == id);

            return Ok(E);
        }

        [HttpGet("[action]/{id}")]
        //[Authorize(Roles = "1,2")]
        [AllowAnonymous]
        public async Task<IActionResult> GetOrderDetail(int id)
        {
            var E = await dBContext.Orders.Include(x => x.OrderDetails).FirstOrDefaultAsync(x => x.OrderId == id);

            if (E != null)
            {
                foreach (var od in E.OrderDetails)
                {
                    od.Order = null;
                }
            }

            return Ok(E);
        }

        [HttpGet("[action]/{id}")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> GetAllWithEmployeeId(int id)
        {
            var E = await dBContext.Orders.Include(x => x.OrderDetails).Include(x => x.Employee)
                .Include(x => x.Customer).ToListAsync();

            var result = E.FindAll(x => x.EmployeeId == id);

            foreach (Order o in result)
            {
                foreach (OrderDetail od in o.OrderDetails)
                {
                    od.Order = null;
                }

                if (o.Employee != null)
                {
                    o.Employee.Orders = null;
                }

                if (o.Customer != null)
                {
                    o.Customer.Orders = null;
                }
            }

            var A = result.OrderBy(x => x.OrderDate);
            var result2 = A.Reverse();

            return Ok(result2);
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> GetCancel()
        {
            var E = await dBContext.Orders.Include(x => x.OrderDetails).ToListAsync();

            var result = E.FindAll(x => x.RequiredDate == null);

            foreach (Order o in result)
            {
                foreach (OrderDetail od in o.OrderDetails)
                {
                    od.Order = null;
                }
            }

            return Ok(result);
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> GetAllWithAccount()
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var email = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;

            Account account;

            if (email != null)
            {
                account = dBContext.Accounts.FirstOrDefault(x => x.Email.Equals(email));

                var customer = dBContext.Customers.FirstOrDefault(x => x.CustomerId.Equals(account.CustomerId));

                var E = await dBContext.Orders.Include(x => x.OrderDetails).ToListAsync();

                List<Order> orders = E.FindAll(x => x.CustomerId.Equals(customer.CustomerId)).OrderBy(x => x.OrderDate).ToList();
                orders.Reverse();

                orders = orders.FindAll(x => x.RequiredDate != null);

                foreach (Order o in orders)
                {
                    foreach (OrderDetail od in o.OrderDetails)
                    {
                        od.Order = null;
                        od.Product = dBContext.Products.ToList().Find(x => x.ProductId == od.ProductId);
                    }
                    o.Customer = null;
                }


                return Ok(orders);
            }

            return BadRequest();
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> GetAllCancelWithAccount()
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var email = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;

            Account account;

            if (email != null)
            {
                account = dBContext.Accounts.FirstOrDefault(x => x.Email.Equals(email));

                var customer = dBContext.Customers.FirstOrDefault(x => x.CustomerId.Equals(account.CustomerId));

                var E = await dBContext.Orders.Include(x => x.OrderDetails).ToListAsync();

                List<Order> orders = E.FindAll(x => x.CustomerId.Equals(customer.CustomerId)).OrderBy(x => x.OrderDate).ToList();
                orders.Reverse();

                orders = orders.FindAll(x => x.RequiredDate == null);

                foreach (Order o in orders)
                {
                    foreach (OrderDetail od in o.OrderDetails)
                    {
                        od.Order = null;
                        od.Product = dBContext.Products.ToList().Find(x => x.ProductId == od.ProductId);
                    }
                    o.Customer = null;
                }


                return Ok(orders);
            }

            return BadRequest();
        }

        [HttpPost("[action]")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> AddWithAccount(List<OrderDetail> orderDetails, DateTime requiredDate)
        {

            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var email = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;

            Account account;

            if (email != null)
            {
                account = dBContext.Accounts.FirstOrDefault(x => x.Email.Equals(email));

                var customer = dBContext.Customers.FirstOrDefault(x => x.CustomerId.Equals(account.CustomerId));


                Order order = new Order();

                order.CustomerId = customer.CustomerId;
                order.OrderDate = DateTime.Now;
                order.ShipName = customer.ContactName;
                order.ShipAddress = customer.Address;
                order.RequiredDate = requiredDate;

                order.EmployeeId = 10;

                float count = 0;

                foreach (OrderDetail od in orderDetails)
                {
                    count += od.Quantity * (float)od.UnitPrice * (1 - od.Discount);
                    od.Product = null;
                }
                order.Freight = (decimal)count;

                dBContext.Orders.Add(order);
                dBContext.SaveChanges();

                foreach (OrderDetail od in orderDetails)
                {
                    od.OrderId = order.OrderId;
                    //od.Order = null;
                }

                dBContext.OrderDetails.AddRange(orderDetails);
                dBContext.SaveChanges();

                Order order1 = dBContext.Orders.Include(x => x.OrderDetails).FirstOrDefault(x => x.OrderId == order.OrderId);
                order1.Customer = null;


                foreach (OrderDetail od1 in order1.OrderDetails)
                {
                    od1.Order = null;

                }
                try
                {
                    return Ok(order1);
                }
                catch (Exception ex)
                {
                    int a = 0;
                }

            }

            return NotFound();
        }


        [HttpPost("[action]")]
        //[Authorize(Roles = "1,2")]
        [AllowAnonymous]
        public async Task<IActionResult> AddByGuest(BuyDTO dto)
        {

            Random ran = new Random();
            string cusId = "";
            for (int i = 0; i < 5; i++)
            {
                int a = ran.Next(65, 91);
                cusId += (char)a;
            }
            dto.customer.CustomerId = cusId;
            await dBContext.Customers.AddAsync(dto.customer);

            dBContext.SaveChanges();

            Order order = new Order();

            order.CustomerId = dto.customer.CustomerId;
            order.OrderDate = DateTime.Now;
            order.ShipName = dto.customer.ContactName;
            order.ShipAddress = dto.customer.Address;
            order.EmployeeId = 10;
            order.RequiredDate = dto.requiredDate;

            float count = 0;

            foreach (OrderDetail od in dto.orderDetails)
            {
                count += od.Quantity * (float)od.UnitPrice;
            }
            order.Freight = (decimal)count;

            dBContext.Orders.Add(order);
            dBContext.SaveChanges();

            foreach (OrderDetail od in dto.orderDetails)
            {
                od.OrderId = order.OrderId;
                od.Product = null;
            }
            try
            {

                dBContext.OrderDetails.AddRange(dto.orderDetails);
                dBContext.SaveChanges();
            }
            catch (Exception ex)
            {
                int a = 0;
            }

            foreach (OrderDetail orderDetail in order.OrderDetails)
            {
                orderDetail.Order = null;
            }

            order.Customer = null;

            Order order1 = dBContext.Orders.Include(x => x.OrderDetails).FirstOrDefault(x => x.OrderId == order.OrderId);

            List<OrderDetail> lsd = new List<OrderDetail>();

            foreach (OrderDetail od1 in order1.OrderDetails)
            {
                od1.Order = null;
                OrderDetail de = dBContext.OrderDetails.Include(x => x.Product).FirstOrDefault(x => x.OrderId == od1.OrderId && x.ProductId == od1.ProductId);
                de.Order = null;
                lsd.Add(de);
            }

            order1.OrderDetails = lsd;

            return Ok(order1);
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> GetMonthlyNumber()
        {
            var E = await dBContext.Orders.ToListAsync();

            List<Order> orders = E.ToList();

            Dictionary<int, int> monthlyOrders = new Dictionary<int, int>();

            for (int i = 1; i < 13; i++)
            {
                int count = orders.FindAll(x => x.OrderDate.Value.Month == i && x.OrderDate.Value.Year == DateTime.Now.Year).Count();

                monthlyOrders.Add(i, count);
            }

            return Ok(monthlyOrders);
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> GetWeeklySale()
        {
            var cultureInfo = Thread.CurrentThread.CurrentCulture;

            DayOfWeek firstDay = cultureInfo.DateTimeFormat.FirstDayOfWeek;
            CalendarWeekRule weekRule = cultureInfo.DateTimeFormat.CalendarWeekRule;
            Calendar cal = cultureInfo.Calendar;

            int week = cal.GetWeekOfYear(DateTime.Now, weekRule, firstDay);

            var E = await dBContext.Orders.ToListAsync();

            List<Order> orders = E.FindAll(x => x.RequiredDate == null && x.OrderDate.Value.Year == DateTime.Now.Year
            && cal.GetWeekOfYear(x.OrderDate.Value, weekRule, firstDay) == week);

            decimal count = 0;

            foreach (Order o in orders)
            {
                count += (decimal)o.Freight;
            }

            return Ok(count);
        }


        [HttpGet("[action]")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> GetTotalOrders()
        {
            var E = await dBContext.Orders.ToListAsync();

            List<Order> orders = E.FindAll(x => x.RequiredDate != null);

            decimal count = 0;

            foreach (Order o in orders)
            {
                count += (decimal)o.Freight;
            }

            return Ok(count);

        }

        [HttpGet("[action]")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> GetAllWithDate([FromQuery(Name = "from")] DateTime fromDate,
            [FromQuery(Name = "to")] DateTime toDate, [FromQuery(Name = "empId")] int empId)
        {

            //DateTime from = DateTime.ParseExact(fromDate, "yyyy-MM-dd", null);

            DateTime def = DateTime.ParseExact("0001/01/01", "yyyy/MM/dd", null);

            var E = await dBContext.Orders.Include(x => x.Employee)
                .Include(x => x.Customer).ToListAsync();



            var result = E.FindAll(x => (CompareOnlyDate(x.OrderDate.Value.Date, fromDate.Date) || CompareOnlyDate(def, fromDate.Date))
            && (CompareOnlyDate(toDate.Date, x.OrderDate.Value.Date) || CompareOnlyDate(def, toDate.Date)));

            if (empId > 0)
            {
                result = result.FindAll(x => x.EmployeeId == empId);
            }

            foreach (Order o in result)
            {
                foreach (OrderDetail od in o.OrderDetails)
                {
                    od.Order = null;
                }

                if (o.Employee != null)
                {
                    o.Employee.Orders = null;
                }

                if (o.Customer != null)
                {
                    o.Customer.Orders = null;
                }
            }

            //foreach(Order o in E)
            //{
            //    Da
            //}

            var A = result.OrderBy(x => x.OrderDate);
            var result2 = A.Reverse();

            return Ok(result2);


        }

        [HttpPut("[action]/{orderId}")]
        [Authorize(Roles = "1,2")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {

            Order order = await dBContext.Orders.FirstOrDefaultAsync(x => x.OrderId == orderId);

            order.RequiredDate = null;

            await dBContext.SaveChangesAsync();

            return Ok();


        }

        [HttpGet("[action]")]
        [Authorize(Roles = "1,2")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDetails([FromQuery(Name = "id")] int id)
        {

            List<OrderDetail> od = await dBContext.OrderDetails.Include(x => x.Product).ToListAsync();

            var result = od.FindAll(x => x.OrderId == id);

            foreach (OrderDetail o in result)
            {

            }

            return Ok(result);

        }

        private bool CompareOnlyDate(DateTime a, DateTime b)
        {
            if (a.Year >= b.Year && a.Month >= b.Month && a.Day >= b.Day)
            {
                return true;
            }
            return false;
        }
    }
}
