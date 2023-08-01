
using Microsoft.AspNetCore.Mvc;

namespace ClientApp.Filters
{
    public class Filters : Controller
    {

        public static bool isAuthorized(string role, ISession session)
        {
            if (session.GetString("role") != null && session.GetString("role").Equals(role))
            {
                return true;
            }

            return false;
        }

    }
}
