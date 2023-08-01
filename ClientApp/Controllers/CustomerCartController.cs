using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ClientApp.DTO;
using ClientApp.Models;

namespace ClientApp.Controllers
{
    public class CustomerCartController : Controller
    {
        public List<OrderDetail> details;
        public IActionResult Index()
        {
            SetUp();
            string orderDetails = HttpContext.Session.GetString("orderDetails");

            if (string.IsNullOrEmpty(orderDetails))
            {
                details = new List<OrderDetail>();
            }
            else
            {
                details = JsonConvert.DeserializeObject<List<OrderDetail>>(orderDetails);
            }

            ViewBag.orderDetails = details;
            return View();
        }

        [Route("/CustomerCartController/Add/{sign}/{productId}")]
        public IActionResult Add(string sign, int productId)
        {
            SetUp();
            string orderDetails = HttpContext.Session.GetString("orderDetails");

            if (string.IsNullOrEmpty(orderDetails))
            {
                details = new List<OrderDetail>();
            }
            else
            {
                details = JsonConvert.DeserializeObject<List<OrderDetail>>(orderDetails);
            }

            foreach (var od in details)
            {
                if (od.ProductId == productId)
                {
                    if ("a".Equals(sign))
                    {
                        od.Quantity++;
                    }
                    else if ("c".Equals(sign))
                    {
                        details.Remove(od);
                    }
                    else
                    {
                        if (od.Quantity <= 1)
                        {
                            details.Remove(od);
                        }
                        else
                        {
                            od.Quantity--;
                        }
                    }
                    break;
                }
            }

            HttpContext.Session.SetString("orderDetails", JsonConvert.SerializeObject(details));

            ViewBag.orderDetails = details;

            return View("Index");
        }

        [Route("/CustomerCart/BuyNow/{productId}")]
        public IActionResult BuyNow(int productId)
        {

            SetUp();
            string orderDetails = HttpContext.Session.GetString("orderDetails");

            if (string.IsNullOrEmpty(orderDetails))
            {
                details = new List<OrderDetail>();
            }
            else
            {
                details = JsonConvert.DeserializeObject<List<OrderDetail>>(orderDetails);
            }

            bool check = false;
            foreach (var od in details)
            {
                if (od.ProductId == productId)
                {
                    od.Quantity++;
                    check = true;
                    break;
                }
            }

            if (!check)
            {
                Product product = null;
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                    //HTTP GET
                    var responseTask = client.GetAsync("/api/product/getwithid/" + productId);
                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync();
                        readTask.Wait();

                        product = JsonConvert.DeserializeObject<Product>(readTask.Result);
                    }
                }

                OrderDetail od = new OrderDetail();
                od.UnitPrice = (decimal)product.UnitPrice;
                od.Discount = 0;
                od.Quantity = 1;
                od.ProductId = productId;

                product.OrderDetails = null;

                od.Product = product;

                details.Add(od);
            }

            HttpContext.Session.SetString("orderDetails", JsonConvert.SerializeObject(details));

            ViewBag.orderDetails = details;

            return View("Index");
        }

        [Route("/CustomerCart/Order")]
        public IActionResult BuyNow(string CompanyName, string ContactName, string ContactTitle, string Address, DateTime RequiredDate)
        {

            SetUp();
            string orderDetails = HttpContext.Session.GetString("orderDetails");

            if (string.IsNullOrEmpty(orderDetails))
            {
                details = new List<OrderDetail>();
            }
            else
            {
                details = JsonConvert.DeserializeObject<List<OrderDetail>>(orderDetails);
            }

            if (string.IsNullOrEmpty(HttpContext.Session.GetString("email")))
            {

                BuyDTO dto = new BuyDTO();
                dto.customer = new Customer()
                {
                    CompanyName = CompanyName,
                    ContactName = ContactName,
                    ContactTitle = ContactTitle,
                    Address = Address
                };

                dto.orderDetails = details;
                dto.requiredDate = RequiredDate;

                Order o = new Order();


                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    //HTTP GET
                    var responseTask = client.PostAsJsonAsync("/api/order/add-no-account", dto);
                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync();
                        readTask.Wait();

                        o = JsonConvert.DeserializeObject<Order>(readTask.Result);

                        HttpContext.Session.SetString("orderDetails", "");
                        details = new List<OrderDetail>();
                    }
                    else
                    {
                        ViewBag.error = "Error while order, please try again later";
                    }
                    ViewBag.orderDetails = details;
                    return View("Index");
                }
            }
            else
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Filters.Utils.BaseUrl()) ;
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("Authorization", HttpContext.Session.GetString("token"));
                    //HTTP GET
                    var responseTask = client.PostAsJsonAsync("/api/order/add-with-account?requiredDate=" + RequiredDate, details);
                    responseTask.Wait();
                    Order o = new Order();
                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync();
                        readTask.Wait();

                        o = JsonConvert.DeserializeObject<Order>(readTask.Result);

                        HttpContext.Session.SetString("orderDetails", "");
                        details = new List<OrderDetail>();
                    }
                    else
                    {
                        ViewBag.error = "Error while order, please try again later";
                    }
                    ViewBag.orderDetails = details;
                    //return Redirect("/SendMailController/SendMail/" + o.OrderId);
                    return View("Index");
                }
            }





            return View();
        }

        private void SetUp()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("token")))
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                    //client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("Authorization", HttpContext.Session.GetString("token"));
                    //HTTP GET
                    var responseTask = client.GetAsync("/api/account/GetPersonalInfo");
                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync();
                        readTask.Wait();

                        RegisterDTO r = JsonConvert.DeserializeObject<RegisterDTO>(readTask.Result);

                        ViewBag.CompanyName = r.customer.CompanyName;
                        ViewBag.ContactName = r.customer.ContactName;
                        ViewBag.ContactTitle = r.customer.ContactTitle;
                        ViewBag.Address = r.customer.Address;

                    }

                }
            }
        }
    }
}
