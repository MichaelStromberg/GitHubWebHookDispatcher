using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace GitHubWebHookDispatcher
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        private const string Title      = "GitHub Webhook Dispatcher";
        private const string ApiVersion = "v1";

        public Startup(IConfiguration configuration) => Configuration = configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        // ReSharper disable once UnusedMember.Global
        public void ConfigureServices(IServiceCollection services)
        {
            var repositoryToScriptPath = GetScriptRepositoryDictionary();

            services.Configure<ScriptRepositoryOptions>(
                options => options.RepositoryToScriptPath = repositoryToScriptPath);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new Info { Title = Title, Version = ApiVersion });

                    // Set the comments path for the Swagger JSON and UI.
                    string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    c.IncludeXmlComments(xmlPath);
                });
        }

        private Dictionary<string, string> GetScriptRepositoryDictionary()
        {
            var repositoryToScriptPath = new Dictionary<string, string>();
            var scriptRepositoryPairs = Configuration.GetSection("ScriptRepositoryPairs").Get<List<ScriptRepositoryPair>>();

            foreach (ScriptRepositoryPair pair in scriptRepositoryPairs)
            {
                repositoryToScriptPath[pair.RepositoryUrl] = pair.FullScriptPath;
            }

            return repositoryToScriptPath;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // ReSharper disable once UnusedMember.Global
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseStaticFiles();

            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.  
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{Title} {ApiVersion}");
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "default",
                    "{controller=Hooks}/{action=Index}/{id?}");
            });
        }
    }
}
