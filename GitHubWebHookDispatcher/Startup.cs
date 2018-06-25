using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GitHubWebHook
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // ReSharper disable once UnusedMember.Global
        public void ConfigureServices(IServiceCollection services)
        {
            var repositoryToScriptPath = GetScriptRepositoryDictionary();

            services.Configure<ScriptRepositoryOptions>(
                options => options.RepositoryToScriptPath = repositoryToScriptPath);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
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
            app.UseMvc();
        }
    }
}
