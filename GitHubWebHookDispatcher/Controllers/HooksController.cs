using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using GitHubWebHookDispatcher.Models;
using GitHubWebHookDispatcher.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GitHubWebHookDispatcher.Controllers
{
    [Route("api/[controller]")]
    public class HooksController : Controller
    {
        private readonly Dictionary<string, string> _repositoryToScriptPath;
        private readonly ILogger<HooksController> _logger;

        public HooksController(IOptions<ScriptRepositoryOptions> options, ILogger<HooksController> logger)
        {
            _repositoryToScriptPath = options.Value.RepositoryToScriptPath;
            _logger                 = logger;
        }

        // POST api/hooks
        [HttpPost]
        public void Submit([FromBody]GitHubEvent webhook)
        {
            if (webhook == null)
            {
                _logger.LogDebug("Webhook is null. Skipping dispatch.");
                return;
            }

            var repositoryUrl = GetRepositoryUrl(webhook.compare);

            if (repositoryUrl == null)
            {
                _logger.LogDebug("repositoryUrl is null. Skipping dispatch.");
                return;
            }

            _logger.LogInformation($"Repository URL: {repositoryUrl}");

            string scriptPath;

            if (!_repositoryToScriptPath.TryGetValue(repositoryUrl, out scriptPath))
            {
                _logger.LogError($"Unable to find repository ({repositoryUrl}) in the dictionary. Skipping dispatch.");
                return;
            }

            Task.Run(() => RunScript(scriptPath));
        }

        private static string GetRepositoryUrl(string compare)
        {
            if (string.IsNullOrEmpty(compare)) return null;

            int compareIndex = compare.IndexOf("/compare", StringComparison.Ordinal);
            if (compareIndex == -1) return null;

            return compare.Substring(0, compareIndex);
        }

        private void RunScript(string batchPath)
        {
            _logger.LogInformation($"Running the script ({batchPath}) associated with the repository.");

            var process = new Process
            {
                StartInfo =
                {
                    FileName         = batchPath,
                    WorkingDirectory = Path.GetDirectoryName(batchPath)
                }
            };

            process.Start();
            process.WaitForExit();

            _logger.LogInformation($"Finished executing the script ({batchPath}).");
        }
    }
}
