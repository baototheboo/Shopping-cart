using Microsoft.AspNetCore.Mvc;
using ClientApp.Models;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using ClientApp.DTO;
using System.Net.Http.Headers;
using System.Text;
using System.Data;
using ClosedXML.Excel;

namespace ClientApp.Controllers
{
    public class AdminProductController : Controller
    {

        public List<Product> listProduct;

        public List<Category> listCate;

        [HttpGet]
        public IActionResult Index()
        {
            if (!Filters.Filters.isAuthorized("2", HttpContext.Session))
            {
                return Redirect("/Auth/SignIn");
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                //HTTP GET
                var responseTask = client.GetAsync("/api/product/Get");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    listProduct = JsonConvert.DeserializeObject<List<Product>>(readTask.Result);
                }
            }
            listProduct.Reverse();
            SetUpIndex();

            ViewBag.listProduct = listProduct;
            //ViewBag.listCategory = listCate;

            return View();
        }


        [Route("/AdminProduct/Edit")]
        public IActionResult Edit(Product product, int id)
        {

            if (!Filters.Filters.isAuthorized("2", HttpContext.Session))
            {
                return Redirect("/Auth/SignIn");
            }

            if (product.ProductId <= 0)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                    
                    //HTTP GET
                    var responseTask = client.GetAsync("/api/product/Get");
                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync();
                        readTask.Wait();

                        listProduct = JsonConvert.DeserializeObject<List<Product>>(readTask.Result);
                    }
                }
                listProduct.Reverse();
                SetUpIndex();

                ViewBag.listProduct = listProduct;
                ViewBag.editProductId = id;
                //ViewBag.listCategory = listCate;

                return View("Index");
            }
            else
            {

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                    client.DefaultRequestHeaders.Add("Authorization", HttpContext.Session.GetString("token"));
                    //HTTP GET
                    var responseTask = client.PutAsJsonAsync("/api/product/Update", product);
                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync();
                        readTask.Wait();

                        //listProduct = JsonConvert.DeserializeObject<List<Product>>(readTask.Result);
                    }
                }

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                    //HTTP GET
                    var responseTask = client.GetAsync("/api/product/Get");
                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync();
                        readTask.Wait();

                        listProduct = JsonConvert.DeserializeObject<List<Product>>(readTask.Result);
                    }
                }
                listProduct.Reverse();
                SetUpIndex();

                ViewBag.listProduct = listProduct;
                //ViewBag.listCategory = listCate;


                return View("Index");
            }
        }

        [Route("/AdminProduct/Delete/{id}")]
        public IActionResult Delete(int id)
        {

            if (!Filters.Filters.isAuthorized("2", HttpContext.Session))
            {
                return Redirect("/Auth/SignIn");
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                //HTTP GET
                var responseTask = client.DeleteAsync("/api/product/Delete/" + id);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    //listProduct = JsonConvert.DeserializeObject<List<Product>>(readTask.Result);
                }
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                //HTTP GET
                var responseTask = client.GetAsync("/api/product/Get");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    listProduct = JsonConvert.DeserializeObject<List<Product>>(readTask.Result);
                }
            }
            listProduct.Reverse();
            SetUpIndex();

            ViewBag.listProduct = listProduct;
            //ViewBag.listCategory = listCate;

            return View("Index");
        }

        [Route("/AdminProduct/Add")]
        public IActionResult Add(Product product) {

            if (!Filters.Filters.isAuthorized("2", HttpContext.Session))
            {
                return Redirect("/Auth/SignIn");
            }

            if (product.ProductName == null)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                    //HTTP GET
                    var responseTask = client.GetAsync("/api/product/Get");
                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync();
                        readTask.Wait();

                        listProduct = JsonConvert.DeserializeObject<List<Product>>(readTask.Result);
                    }
                }

                SetUpIndex();

                listProduct.Add(new Product());
                listProduct.Reverse();
                

                ViewBag.listProduct = listProduct;
                //ViewBag.listCategory = listCate;

                return View("Index");
            }

            else
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                    //HTTP GET
                    var responseTask = client.PostAsJsonAsync("/api/product/AddProduct", product);
                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync();
                        readTask.Wait();

                        //listProduct = JsonConvert.DeserializeObject<List<Product>>(readTask.Result);
                    }
                }

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                    //HTTP GET
                    var responseTask = client.GetAsync("/api/product/Get");
                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync();
                        readTask.Wait();

                        listProduct = JsonConvert.DeserializeObject<List<Product>>(readTask.Result);
                    }
                }

                SetUpIndex();
                listProduct.Reverse();


                return View("Index");
            }

        }

        [Route("/AdminProduct/Search_Filter")]
        //[ValidateAntiForgeryToken]
        public IActionResult Search_Filter( string txtSearch, int categoryId)
        {

            if (!Filters.Filters.isAuthorized("2", HttpContext.Session))
            {
                return Redirect("/Auth/SignIn");
            }

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

                    listProduct = JsonConvert.DeserializeObject<List<Product>>(readTask.Result);
                }
            }

            SetUpIndex();
            listProduct.Reverse();

            ViewBag.listProduct = listProduct;
            ViewBag.txtSearch = txtSearch;
            ViewBag.categoryId = categoryId;
            return View("Index");
        }

        private void SetUpIndex()
        {
            

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
            ViewBag.listCategory = listCate;
        }
    }
}
