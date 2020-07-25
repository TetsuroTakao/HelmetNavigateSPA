using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using SampleApp;
using SampleApp.Models;

namespace React.Sample.Webpack.CoreMvc.Controllers
{
	public class HomeController : Controller
	{
		private const int COMMENTS_PER_PAGE = 3;

		private readonly IDictionary<string, AuthorModel> _authors;
		private readonly IList<CommentModel> _comments;
		string clientId { get; set; }
		string redirectUri { get; set; }
		string endpoint { get; set; }
		string clientSecret { get; set; }
		string profileScope { get; set; }
		string profileResource { get; set; }
		string tokenSST { get; set; }

		public HomeController()
		{
			redirectUri = Environment.GetEnvironmentVariable("RedirectLocal");
			switch (Environment.GetEnvironmentVariable("TargetBrand"))
			{
				case "microsoft":
					endpoint = Environment.GetEnvironmentVariable("MSEndpoint");
					clientId = Environment.GetEnvironmentVariable("MSClientId");
					clientSecret = Environment.GetEnvironmentVariable("MSClientSecret");
					profileScope = Environment.GetEnvironmentVariable("MSProfileScope");
					profileResource = Environment.GetEnvironmentVariable("MSProfileResource");
					tokenSST = Environment.GetEnvironmentVariable("MSTokenSTS");
					break;
				case "facebook":
					endpoint = Environment.GetEnvironmentVariable("FBEndpoint");
					clientId = Environment.GetEnvironmentVariable("FBClientId");
					clientSecret = Environment.GetEnvironmentVariable("FBClientSecret");
					profileScope = Environment.GetEnvironmentVariable("FBProfileScope");
					profileResource = Environment.GetEnvironmentVariable("FBProfileResource");
					tokenSST = Environment.GetEnvironmentVariable("FBTokenSTS");
					break;
				default:
					endpoint = Environment.GetEnvironmentVariable("MSEndpoint");
					clientId = Environment.GetEnvironmentVariable("MSClientId");
					clientSecret = Environment.GetEnvironmentVariable("MSClientSecret");
					profileScope = Environment.GetEnvironmentVariable("MSProfileScope");
					profileResource = Environment.GetEnvironmentVariable("MSProfileResource");
					tokenSST = Environment.GetEnvironmentVariable("MSTokenSTS");
					break;
			}

			_authors = new Dictionary<string, AuthorModel>
			{
				{"daniel", new AuthorModel { Name = "Daniel Lo Nigro", GithubUsername = "Daniel15" }},
				{"vjeux", new AuthorModel { Name = "Christopher Chedeau", GithubUsername = "vjeux" }},
				{"cpojer", new AuthorModel { Name = "Christoph Pojer", GithubUsername = "cpojer" }},
				{"jordwalke", new AuthorModel { Name = "Jordan Walke", GithubUsername = "jordwalke" }},
				{"zpao", new AuthorModel { Name = "Paul O'Shannessy", GithubUsername = "zpao" }},
			};
			_comments = new List<CommentModel>
			{
				new CommentModel { Author = _authors["daniel"], Text = "First!!!!111!" },
				new CommentModel { Author = _authors["zpao"], Text = "React is awesome!" },
				new CommentModel { Author = _authors["cpojer"], Text = "Awesome!" },
				new CommentModel { Author = _authors["vjeux"], Text = "Hello World" },
				new CommentModel { Author = _authors["daniel"], Text = "Foo" },
				new CommentModel { Author = _authors["daniel"], Text = "Bar" },
				new CommentModel { Author = _authors["daniel"], Text = "FooBarBaz" },
			};
		}

		public ActionResult Index()
		{
			#region Get Authorization code
			var query = Request.Query;
			var referer = Request.Headers["Referer"].ToString();
			var code = string.Empty;
			if (query.ContainsKey("code")) code = query["code"];//if (query.ContainsKey("session_state")) state = query["session_state"];
			#endregion
			ViewBag.IsSignin = !string.IsNullOrEmpty(code);
			if (ViewBag.IsSignin) 
			{
				using (var httpClient = new HttpClient())
				{
					#region Get Access token
					var properties = "client_id=" + clientId + "&client_secret=" + clientSecret;
					properties += "&redirect_uri=" + redirectUri;
					properties += "&scope=" + profileScope;
					properties += "&code=" + code + "&grant_type=authorization_code";
					HttpResponseMessage res = null;
					ViewBag.Brand = Environment.GetEnvironmentVariable("TargetBrand");
					if (ViewBag.Brand == "microsoft")
					{
						var content = new StringContent(properties, Encoding.UTF8, "application/x-www-form-urlencoded");
						res = httpClient.PostAsync(tokenSST, content).Result;
					}
					else if (Environment.GetEnvironmentVariable("TargetBrand") == "facebook") 
					{
						res = httpClient.GetAsync(tokenSST + "?" + properties).Result;
					}
					string resultJson = res.Content.ReadAsStringAsync().Result;
					var tempElement = new JsonElement();
					#endregion
					var token = string.Empty;
					if (res.IsSuccessStatusCode)
					{
						var doc = JsonDocument.Parse(resultJson).RootElement;
						if (doc.TryGetProperty("access_token", out tempElement)) token = tempElement.GetString();
						using (var httpClient2 = new HttpClient())
						{
							httpClient2.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
							var res2 = httpClient2.GetAsync(profileResource).Result;
							string resultJson2 = res2.Content.ReadAsStringAsync().Result;
							if (res2.IsSuccessStatusCode)
							{
								BrandPrimitive user = null;
								if (ViewBag.Brand == "microsoft")
								{
									user = JsonSerializer.Deserialize<MSGraphUser>(resultJson2);
									ViewBag.AccountName = (user as MSGraphUser).displayName;
									var appUser = new ApplicationUser<MSGraphUser>() { UserCore = (user as MSGraphUser), BrandName = IPBlandType.Microsoft };
									var loginState = new LoginModel<MSGraphUser>() { AuthrizeUrl = tokenSST, IsLogin = true, User = appUser };
									new DataAccessLayer().SetStateManagement<LoginModel<MSGraphUser>>(loginState);
								}
								else 
								{
									user = JsonSerializer.Deserialize<FBGraphUser>(resultJson2);
									ViewBag.AccountName = (user as FBGraphUser).name;
									var appUser = new ApplicationUser<FBGraphUser>() { UserCore = (user as FBGraphUser), BrandName = IPBlandType.Facebook };
									var loginState = new LoginModel<FBGraphUser>() { AuthrizeUrl = tokenSST, IsLogin = true, User = appUser };
									new DataAccessLayer().SetStateManagement<LoginModel<FBGraphUser>>(loginState);
								}
							}
							else 
							{
								ViewBag.Brand = string.Empty;
							}
						}
					}
				}
			}
			ViewBag.AppTitle = "Screen Reservation";
			return View();
			//return View(new IndexViewModel
			//{
			//	Comments = _comments.Take(COMMENTS_PER_PAGE).ToList().AsReadOnly(),
			//	CommentsPerPage = COMMENTS_PER_PAGE,
			//	Page = 1
			//});
		}

		public ActionResult Comments(int page)
		{
			var comments = _comments.Skip((page - 1) * COMMENTS_PER_PAGE).Take(COMMENTS_PER_PAGE);
			var hasMore = page * COMMENTS_PER_PAGE < _comments.Count;

			if (ControllerContext.HttpContext.Request.ContentType == "application/json")
			{
				return new JsonResult(new
				{
					comments = comments,
					hasMore = hasMore
				});
			}
			else
			{
				return View("~/Views/Home/Index.cshtml", new IndexViewModel
				{
					Comments = _comments.Take(COMMENTS_PER_PAGE * page).ToList().AsReadOnly(),
					CommentsPerPage = COMMENTS_PER_PAGE,
					Page = page
				});
			}
		}

		public class AuthorModel
		{
			public string Name { get; set; }
			public string GithubUsername { get; set; }
		}

		public class CommentModel
		{
			public AuthorModel Author { get; set; }
			public string Text { get; set; }
		}

		public class IndexViewModel
		{
			public IReadOnlyList<CommentModel> Comments { get; set; }
			public int CommentsPerPage { get; set; }
			public int Page { get; set; }
		}

	}
}
