using System.Collections.Generic;
using GitHubWebHookDispatcher.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GitHubWebHookDispatcher
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var repositoryToScriptPath = GetScriptRepositoryDictionary();

            services.Configure<ScriptRepositoryOptions>(
                options => options.RepositoryToScriptPath = repositoryToScriptPath);

            // Add framework services.
            services.AddMvc();
        }

        private Dictionary<string, string> GetScriptRepositoryDictionary()
        {
            var repositoryToScriptPath = new Dictionary<string, string>();
            var scriptRepositoryPairs  = Configuration.GetSection("ScriptRepositoryPairs").Get<List<ScriptRepositoryPair>>();

            foreach (var pair in scriptRepositoryPairs)
            {
                repositoryToScriptPath[pair.RepositoryUrl] = pair.FullScriptPath;
            }

            return repositoryToScriptPath;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();
        }
    }
}
