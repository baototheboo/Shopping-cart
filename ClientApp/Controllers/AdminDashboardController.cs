using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ClientApp.Controllers
{
    public class AdminDashboardController : Controller
    {
        public decimal weeklySale = 0;
        public Dictionary<int, int> monthlyOrder = new Dictionary<int, int>();
        public decimal totalOrder = 0;
        public int totalCustomer = 0;
        public int totalGuest = 0;

        public IActionResult Index()
        {

            if(!Filters.Filters.isAuthorized("2", HttpContext.Session))
            {
                return Redirect("/Auth/SignIn");
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                client.DefaultRequestHeaders.Add("Authorization", HttpContext.Session.GetString("token"));

                //HTTP GET
                var responseTask = client.GetAsync("/api/order/GetMonthlyNumber");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    monthlyOrder = JsonConvert.DeserializeObject<Dictionary<int, int>>(readTask.Result);
                }
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                client.DefaultRequestHeaders.Add("Authorization", HttpContext.Session.GetString("token"));

                //HTTP GET
                var responseTask = client.GetAsync("/api/order/GetWeeklySale");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    weeklySale = JsonConvert.DeserializeObject<decimal>(readTask.Result);
                }
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                client.DefaultRequestHeaders.Add("Authorization", HttpContext.Session.GetString("token"));

                //HTTP GET
                var responseTask = client.GetAsync("/api/order/GetTotalOrders");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    totalOrder = JsonConvert.DeserializeObject<decimal>(readTask.Result);
                }
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                client.DefaultRequestHeaders.Add("Authorization", HttpContext.Session.GetString("token"));

                //HTTP GET
                var responseTask = client.GetAsync("/api/customer/GetTotalCustomer");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    totalCustomer = JsonConvert.DeserializeObject<int>(readTask.Result);
                }
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                client.DefaultRequestHeaders.Add("Authorization", HttpContext.Session.GetString("token"));

                //HTTP GET
                var responseTask = client.GetAsync("/api/customer/GetTotalGuests");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    totalGuest = JsonConvert.DeserializeObject<int>(readTask.Result);
                }
            }

            List<int[]> monthly = new List<int[]>();
            monthly.Add(monthlyOrder.Keys.ToArray());
            monthly.Add(monthlyOrder.Values.ToArray());

            ViewBag.monthlyOrder = JsonConvert.SerializeObject(monthly);
            ViewBag.weeklySale = weeklySale;
            ViewBag.totalOrder = totalOrder;
            ViewBag.totalCustomer = totalCustomer;
            ViewBag.totalGuest = totalGuest;



            //monthlyOrder.Values.ToArray();

            return View();



        }
    }
}
