using Microsoft.AspNetCore.Mvc;
using ClientApp.Models;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using ClientApp.DTO;

namespace ClientApp.Controllers
{
    public class CustomerProductController : Controller
    {

        public List<Product> listHot;
        public List<Product> listSale;
        public List<Product> listNew;

        public List<Category> listCate;
        public IActionResult Index()
        {

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                //HTTP GET
                var responseTask = client.GetAsync("/api/product/GetHot");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    listHot = JsonConvert.DeserializeObject<List<Product>>(readTask.Result);
                }
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                //HTTP GET
                var responseTask = client.GetAsync("/api/product/GetBestSale");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    listSale = JsonConvert.DeserializeObject<List<Product>>(readTask.Result);
                }
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                //HTTP GET
                var responseTask = client.GetAsync("/api/product/GetNewItem");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    listNew = JsonConvert.DeserializeObject<List<Product>>(readTask.Result);
                }
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                //HTTP GET
                var responseTask = client.GetAsync("/api/product/GetCategory");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    listCate = JsonConvert.DeserializeObject<List<Category>>(readTask.Result);
                }
            }

            ViewBag.listHot = listHot;
            ViewBag.listNew = listNew;
            ViewBag.listSale = listSale;
            ViewBag.listCate = listCate;
            return View();
        }

        
        [Route("/CustomerProduct/Add/{productId}")]
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
            foreach(OrderDetail od in details)
            {
                if(od.ProductId == productId)
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

                details.Add(od);
            }

            orderDetails = JsonConvert.SerializeObject(details);

            HttpContext.Session.SetString("orderDetails", orderDetails);

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                //HTTP GET
                var responseTask = client.GetAsync("/api/product/GetHot");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    listHot = JsonConvert.DeserializeObject<List<Product>>(readTask.Result);
                }
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                //HTTP GET
                var responseTask = client.GetAsync("/api/product/GetBestSale");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    listSale = JsonConvert.DeserializeObject<List<Product>>(readTask.Result);
                }
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                //HTTP GET
                var responseTask = client.GetAsync("/api/product/GetNewItem");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    listNew = JsonConvert.DeserializeObject<List<Product>>(readTask.Result);
                }
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                //HTTP GET
                var responseTask = client.GetAsync("/api/product/GetCategory");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    listCate = JsonConvert.DeserializeObject<List<Category>>(readTask.Result);
                }
            }

            ViewBag.listHot = listHot;
            ViewBag.listNew = listNew;
            ViewBag.listSale = listSale;
            ViewBag.listCate = listCate;
            return View("Index");

        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        public IActionResult Search_Filter([FromForm(Name ="txtSearch")] string txtSearch, [FromForm(Name = "categoryId")] int categoryId)
        {
            using (var client = new HttpClient())
            {

                SearchFilterDTO dto = new SearchFilterDTO();
                dto.Name = txtSearch;
                dto.CategoryId = categoryId;
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                
                //HTTP GET
                var responseTask = client.PostAsJsonAsync("/api/product/FilterAndSearch", dto);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    listHot = JsonConvert.DeserializeObject<List<Product>>(readTask.Result);
                }
            }

            
            ViewBag.listProduct = listHot;
            ViewBag.name = txtSearch;
            return View("Index");
        }
    }
}
