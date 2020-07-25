using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SampleApp.Controllers
{
    public class AccountController : Controller
    {
        string clientId { get; set; }
        string redirectUri { get; set; }
        string endpoint { get; set; }
        string profileScope { get; set; }
        public AccountController()
        {
            endpoint = Environment.GetEnvironmentVariable("MSEndpoint");
            clientId = Environment.GetEnvironmentVariable("MSClientId");
            redirectUri = Environment.GetEnvironmentVariable("RedirectLocal");
            profileScope = Environment.GetEnvironmentVariable("MSProfileScope");
        }
        public IActionResult Index()
        {
            return View();
        }
        public void SignIn(string brand)
        {
            var param = "?client_id=" + clientId + "&redirect_uri=" + redirectUri + "&grant_type=implicit&response_type=code&scope=" + profileScope;
            Environment.SetEnvironmentVariable("TargetBrand", brand);
            switch (brand) 
            {
                case "microsoft":
                    Response.Redirect(endpoint + "authorize" + param);
                    break;
                case "facebook":
                    endpoint = Environment.GetEnvironmentVariable("FBEndpoint");
                    clientId = Environment.GetEnvironmentVariable("FBClientId");
                    profileScope = Environment.GetEnvironmentVariable("FBProfileScope");
                    param = "?client_id=" + clientId + "&redirect_uri=" + redirectUri + "&grant_type=implicit&response_type=code&scope=" + profileScope;
                    Response.Redirect(endpoint + "oauth" + param);
                    break;
            }
        }
    }
}
