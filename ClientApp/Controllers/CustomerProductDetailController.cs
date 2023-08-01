using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ClientApp.Models;

namespace ClientApp.Controllers
{
    public class CustomerProductDetailController : Controller
    {
        Product product;

        [Route("CustomerProductDetail/{productId}")]
        public IActionResult Index(int productId)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                //HTTP GET
                var responseTask = client.GetAsync("/api/product/GetWithId/" + productId);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    product = JsonConvert.DeserializeObject<Product>(readTask.Result);
                }
            }

            ViewBag.product = product;

            return View();

        }

        [Route("/CustomerProductDetail/Add/{productId}")]
        public IActionResult Add(int productId)
        {
            Product product = null;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                //HTTP GET
                var responseTask = client.GetAsync("/api/product/GetWithId/" + productId);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    product = JsonConvert.DeserializeObject<Product>(readTask.Result);
                }
            }


            string orderDetails = HttpContext.Session.GetString("orderDetails");
            List<OrderDetail> details;

            if (string.IsNullOrEmpty(orderDetails))
            {
                details = new List<OrderDetail>();
            }
            else
            {
                details = JsonConvert.DeserializeObject<List<OrderDetail>>(orderDetails);
            }

            bool check = false;
            foreach (OrderDetail od in details)
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
                OrderDetail od = new OrderDetail();
                od.UnitPrice = (decimal)product.UnitPrice;
                od.Discount = 0;
                od.Quantity = 1;
                od.ProductId = productId;

                product.OrderDetails = null;

                od.Product = product;

                details.Add(od);
            }

            orderDetails = JsonConvert.SerializeObject(details);

            HttpContext.Session.SetString("orderDetails", orderDetails);

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                //HTTP GET
                var responseTask = client.GetAsync("/api/product/GetWithId/" + productId);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    product = JsonConvert.DeserializeObject<Product>(readTask.Result);
                }
            }

            ViewBag.product = product;

            return View("Index");
        }
    }
}
