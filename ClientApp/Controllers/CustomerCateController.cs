using Microsoft.AspNetCore.Mvc;
using ClientApp.Models;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using ClientApp.DTO;

namespace ClientApp.Controllers
{
    public class CustomerCateController : Controller
    {

        public List<Product> listProduct;

        public List<Category> listCate;

        public Category category;

        [Route("CustomerCate/{cateId}")]
        public IActionResult Index(int cateId)
        {

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                //HTTP GET
                var responseTask = client.GetAsync("/api/product/GetWithCategory/" + cateId);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    listProduct = JsonConvert.DeserializeObject<List<Product>>(readTask.Result);
                }
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                //HTTP GET
                var responseTask = client.GetAsync("/api/category/GetCateById/" + cateId);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    category = JsonConvert.DeserializeObject<Category>(readTask.Result);
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

            ViewBag.listProduct = listProduct;
            ViewBag.listCate = listCate;
            ViewBag.category = category;
            return View();
        }

        
    }
}
