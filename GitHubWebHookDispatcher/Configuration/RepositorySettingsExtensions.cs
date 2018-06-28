using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GitHubWebHookDispatcher.Configuration
{
    public static class RepositorySettingsExtensions
    {
        public static void AddScripts(this IServiceCollection services, IConfiguration configuration)
        {
            var repositoryToScriptPath = new Dictionary<string, string>();
            var repositorySettings     = configuration.GetSection("RepositorySettings").Get<RepositorySettings>();

            if (repositorySettings == null)
            {
                throw new FileNotFoundException("ERROR: Unable to find the repository settings. Is the appsettings.json file in your output directory?");
            }

            foreach (ScriptRepositoryPair pair in repositorySettings.Scripts)
            {
                repositoryToScriptPath[pair.RepositoryUrl] = pair.FullScriptPath;
            }

            services.Configure<ScriptRepositoryOptions>(options => options.RepositoryToScriptPath = repositoryToScriptPath);
        }
    }
}
