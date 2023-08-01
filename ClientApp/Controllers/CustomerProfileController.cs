using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ClientApp.DTO;
using ClientApp.Models;

namespace ClientApp.Controllers
{
    public class CustomerProfileController : Controller
    {
        public IActionResult Index()
        {
            if (!Filters.Filters.isAuthorized("1", HttpContext.Session))
            {
                return Redirect("/Auth/SignIn");
            }

            SetUp();

            return View();
        }

        [Route("/CustomerProfile/Order")]
        public IActionResult Order()
        {

            if (!Filters.Filters.isAuthorized("1", HttpContext.Session))
            {
                return Redirect("/Auth/SignIn");
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Authorization", HttpContext.Session.GetString("token"));
                //HTTP GET
                var responseTask = client.GetAsync("/api/order/GetAllWithAccount");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    //HttpContext.Session.SetString("token", "Bearer " + readTask.Result);

                    ViewBag.orders = JsonConvert.DeserializeObject<List<Order>>(readTask.Result);

                }
            }

            return View();
        }

        [Route("/CustomerProfile/Cancel")]
        public IActionResult Cancel()
        {

            if (!Filters.Filters.isAuthorized("1", HttpContext.Session))
            {
                return Redirect("/Auth/SignIn");
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Authorization", HttpContext.Session.GetString("token"));
                //HTTP GET
                var responseTask = client.GetAsync("/api/order/GetAllCancelWithAccount");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    //HttpContext.Session.SetString("token", "Bearer " + readTask.Result);

                    ViewBag.orders = JsonConvert.DeserializeObject<List<Order>>(readTask.Result);

                }
            }

            return View();
        }

        [Route("/CustomerProfile/Cancel/{orderId}")]
        public IActionResult Cancel(int orderId)
        {

            if (!Filters.Filters.isAuthorized("1", HttpContext.Session))
            {
                return Redirect("/Auth/SignIn");
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Authorization", HttpContext.Session.GetString("token"));
                //HTTP GET
                var responseTask = client.PutAsync("/api/order/CancelOrder/" + orderId, null);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    //HttpContext.Session.SetString("token", "Bearer " + readTask.Result);

                    //ViewBag.orders = JsonConvert.DeserializeObject<List<Order>>(readTask.Result);

                }
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Authorization", HttpContext.Session.GetString("token"));
                //HTTP GET
                var responseTask = client.GetAsync("/api/order/GetAllWithAccount");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    //HttpContext.Session.SetString("token", "Bearer " + readTask.Result);

                    ViewBag.orders = JsonConvert.DeserializeObject<List<Order>>(readTask.Result);

                }
            }

            return View("Order");
        }

        [Route("/CustomerProfile/Edit")]
        [HttpPost]
        public IActionResult Edit(string CompanyName, string ContactName, string ContactTitle, string Address, string Email)
        {

            if (!Filters.Filters.isAuthorized("1", HttpContext.Session))
            {
                return Redirect("/Auth/SignIn");
            }

            RegisterDTO dto = new RegisterDTO();
            dto.account = new Models.Account()
            {
                Email = Email
            };

            dto.customer = new Models.Customer()
            {
                CompanyName = CompanyName,
                ContactName = ContactName,
                ContactTitle = ContactTitle,
                Address = Address
            };

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Authorization", HttpContext.Session.GetString("token"));
                //HTTP GET
                var responseTask = client.PutAsJsonAsync("/api/account/EditPersonalInfo", dto);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    if (!string.IsNullOrEmpty(readTask.Result)){

                        string token = "Bearer " + readTask.Result.Trim('\"');

                        HttpContext.Session.SetString("token", token);

                        HttpContext.Session.SetString("email", Email);
                    }

                    ViewBag.message = "Edit successful";

                }
                else
                {
                    ViewBag.error = "Error while edit, please try again later";
                }

                SetUp();

                return View("Index");
            }
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
