using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ClientApp.Models;

namespace ClientApp.Controllers
{
    public class AdminOrderDetail : Controller
    {

        public List<OrderDetail> listOrderDetail = new List<OrderDetail>();

        public Order order = new Order();

        [Route("/AdminOrderDetail/Index/{id}")]
        public IActionResult Index(int id)
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
                var responseTask = client.GetAsync("/api/order/GetDetails?id=" + id);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    listOrderDetail = JsonConvert.DeserializeObject<List<OrderDetail>>(readTask.Result);
                }
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Filters.Utils.BaseUrl());
                client.DefaultRequestHeaders.Add("Authorization", HttpContext.Session.GetString("token"));

                //HTTP GET
                var responseTask = client.GetAsync("/api/order/GetOrder/" + id);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    order = JsonConvert.DeserializeObject<Order>(readTask.Result);
                }
            }

            ViewBag.listOrderDetail = listOrderDetail;

            ViewBag.order = order;

            return View("Index");
        }
    }
}
