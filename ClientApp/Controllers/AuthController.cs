using Microsoft.AspNetCore.Mvc;
using ClientApp.Models;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using ClientApp.DTO;

namespace ClientApp.Controllers
{
    public class AuthController : Controller
    {

        public List<Product> listProduct;

        public List<Category> listCate;

        public Category category;

        [Route("Auth/signin")]
        [HttpGet]
        public IActionResult SignIn()
        {
            return View();
        }

        [Route("Auth/signout")]
        public IActionResult SignOut()
        {
            HttpContext.Session.Clear();

            return Redirect("/CustomerProduct/Index");
        }

        [Route("Auth/signin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SignIn(string email, string password)
        {
            //if (!string.IsNullOrEmpty(HttpContext.Session.GetString("email")))
            //{
            //    if ("1".Equals(HttpContext.Session.GetString("role")))
            //    {
            //        return View("/CustomerProduct/Index");
            //    }
            //    if ("2".Equals(HttpContext.Session.GetString("role")))
            //    {
            //        return RedirectToAction("", "AdminProduct");
            //    }
            //}
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                //HTTP GET
                var responseTask = client.PostAsync("/api/auth/signin?email=" + email + "&password=" + password, null);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    //Session["hmm"] = JsonConvert.DeserializeObject<List<Product>>(readTask.Result);
                    String token = readTask.Result;
                    if (!string.IsNullOrEmpty(token))
                    {
                        HttpContext.Session.SetString("email", email);
                        HttpContext.Session.SetString("token", "Bearer " + token);

                        using (var client2 = new HttpClient())
                        {
                            client2.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                            client2.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                            //HTTP GET
                            var responseTask2 = client2.GetAsync("/api/auth/GetRole");
                            responseTask2.Wait();

                            var result2 = responseTask2.Result;
                            if (result2.IsSuccessStatusCode)
                            {
                                var readTask2 = result2.Content.ReadAsStringAsync();
                                readTask2.Wait();

                                //Session["hmm"] = JsonConvert.DeserializeObject<List<Product>>(readTask.Result);
                                String role = readTask2.Result;
                                if (!string.IsNullOrEmpty(token))
                                {
                                    HttpContext.Session.SetString("role", role);

                                    if ("1".Equals(role))
                                    {
                                        return Redirect("/CustomerProduct/Index");
                                    }else if ("2".Equals(role))
                                    {
                                        return RedirectToAction("", "AdminProduct");
                                    }
                                }


                            }
                        }


                        //return RedirectToAction("Index", "CustomerProduct");
                    }


                }
            }

            return View();
        }


    }
}
