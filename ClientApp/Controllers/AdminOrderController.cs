using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ClientApp.Filters;
using ClientApp.Models;
using OfficeOpenXml;
using System.Data;
using ClosedXML.Excel;

namespace ClientApp.Controllers
{
    public class AdminOrderController : Controller
    {

        public List<Order> listOrder = new List<Order>();

        [HttpGet]
        public IActionResult Index()
        {
            if (! Filters.Filters.isAuthorized("2", HttpContext.Session))
            {
                return Redirect("/Auth/SignIn");
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                client.DefaultRequestHeaders.Add("Authorization", HttpContext.Session.GetString("token"));

                //HTTP GET
                var responseTask = client.GetAsync("/api/order/GetAllOrder");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    listOrder = JsonConvert.DeserializeObject<List<Order>>(readTask.Result);
                }
            }


            ViewBag.listOrder = listOrder;

            return View();
        }

        [Route("/AdminOrder/Cancel/{id}")]
        public IActionResult Cancel(int id)
        {
            if (!Filters.Filters.isAuthorized("2", HttpContext.Session))
            {
                return Redirect("/Auth/SignIn");
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                client.DefaultRequestHeaders.Add("Authorization", HttpContext.Session.GetString("token"));

                //HTTP GET
                var responseTask = client.PutAsync("/api/order/CancelOrder/" + id, null);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();
                }
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                client.DefaultRequestHeaders.Add("Authorization", HttpContext.Session.GetString("token"));

                //HTTP GET
                var responseTask = client.GetAsync("/api/order/GetAllOrder");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    listOrder = JsonConvert.DeserializeObject<List<Order>>(readTask.Result);
                }
            }


            ViewBag.listOrder = listOrder;

            return View("Index");
        }

        [Route("/AdminOrder/Filter")]
        public IActionResult Filter(DateTime from, DateTime to)
        {
            if (!Filters.Filters.isAuthorized("2", HttpContext.Session))
            {
                return Redirect("/Auth/SignIn");
            }

            DateTime def = DateTime.ParseExact("0001/01/01", "yyyy/MM/dd", null);

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                client.DefaultRequestHeaders.Add("Authorization", HttpContext.Session.GetString("token"));

                //HTTP GET
                var responseTask = client.GetAsync("/api/order/GetAllWithDate?from=" + from+"&to="+to);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    listOrder = JsonConvert.DeserializeObject<List<Order>>(readTask.Result);
                }
            }


            ViewBag.listOrder = listOrder;
            
            if(!(from.Year == def.Year && from.Month == def.Month && from.Day == def.Day))
            {
                ViewBag.from = from.ToString("yyyy-MM-dd");
            }

            if (!(to.Year == def.Year && to.Month == def.Month && to.Day == def.Day))
            {
                ViewBag.to = to.ToString("yyyy-MM-dd");
            }

            return View("Index");
        }
    }
}
