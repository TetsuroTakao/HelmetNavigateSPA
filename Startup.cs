using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Extensions.MsDependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using React.AspNet;
using System;
using System.IO;

namespace React.Sample.Webpack.CoreMvc
{
    public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
			#region insert settings value
			Environment.SetEnvironmentVariable("EncKey", Configuration["Encryptor:AesCryptoServiceProviderKey"]);
			Environment.SetEnvironmentVariable("EncIV", Configuration["Encryptor:AesCryptoServiceProviderIV"]);
			Environment.SetEnvironmentVariable("Redirect", Configuration["Redirect"]);
			Environment.SetEnvironmentVariable("RedirectLocal", Configuration["RedirectLocal"]);
			#region microsoft
			Environment.SetEnvironmentVariable("MSEndpoint", Configuration["MicrosoftIdentity:Endpoint"]);
			Environment.SetEnvironmentVariable("MSClientId", Configuration["MicrosoftIdentity:ClientId"]);
			Environment.SetEnvironmentVariable("MSClientSecret", Configuration["MicrosoftIdentity:ClientSecret"]);
			Environment.SetEnvironmentVariable("MSProfileScope", Configuration["MicrosoftIdentity:ProfileScope"]);
			Environment.SetEnvironmentVariable("MSProfileResource", Configuration["MicrosoftIdentity:ProfileResource"]);
			Environment.SetEnvironmentVariable("MSTokenSTS", Configuration["MicrosoftIdentity:TokenSTS"]);
			#endregion
			#region facebook
			Environment.SetEnvironmentVariable("FBEndpoint", Configuration["FacebookIdentity:Endpoint"]);
			Environment.SetEnvironmentVariable("FBClientId", Configuration["FacebookIdentity:ClientId"]);
			Environment.SetEnvironmentVariable("FBClientSecret", Configuration["FacebookIdentity:ClientSecret"]);
			Environment.SetEnvironmentVariable("FBProfileScope", Configuration["FacebookIdentity:ProfileScope"]);
			Environment.SetEnvironmentVariable("FBProfileResource", Configuration["FacebookIdentity:ProfileResource"]);
			Environment.SetEnvironmentVariable("FBTokenSTS", Configuration["FacebookIdentity:TokenSTS"]);
			#endregion
			#endregion
		}
		public IConfiguration Configuration { get; }
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc(options=>options.EnableEndpointRouting = false);

			services.AddJsEngineSwitcher(options => options.DefaultEngineName = ChakraCoreJsEngine.EngineName)
				.AddChakraCore();
			services.AddReact();
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			// Build the intermediate service provider then return it
			services.BuildServiceProvider();
		}
		public void Configure(IApplicationBuilder app, IHostEnvironment env)
		{
            #region // Initialise ReactJS.NET. Must be before static files.
            app.UseReact(config =>
			{
				config
					.SetReuseJavaScriptEngines(true)
					.SetLoadBabel(false)
					.SetLoadReact(false)
					.SetReactAppBuildPath("~/dist");
			});
			#endregion
			if (env.IsDevelopment())
			{
					app.UseDeveloperExceptionPage();
			}
			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
			app.UseStaticFiles(new StaticFileOptions
			{
				FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "contents")),
				RequestPath = "/contents"
			});
            //app.UseRouting();
            //app.UseEndpoints(endpoints =>
            //         {
            //             endpoints.MapControllerRoute("default", "{path?}", new { controller = "Home", action = "Index" });
            //             endpoints.MapControllerRoute("comments-root", "comments", new { controller = "Home", action = "Index" });
            //             endpoints.MapControllerRoute("comments", "comments/page-{page}", new { controller = "Home", action = "Comments" });
            //         });
        }
	}
}
