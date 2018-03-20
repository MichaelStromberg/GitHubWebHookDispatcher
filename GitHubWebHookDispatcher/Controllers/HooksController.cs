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

            string repositoryUrl = GetRepositoryUrl(webhook.compare);

            if (repositoryUrl == null)
            {
                _logger.LogDebug("repositoryUrl is null. Skipping dispatch.");
                return;
            }

            string branch = GetBranch(webhook.@ref);

            _logger.LogInformation($"Repository URL: {repositoryUrl}, branch: {branch}");

            if (!_repositoryToScriptPath.TryGetValue(repositoryUrl, out string scriptPath))
            {
                _logger.LogError($"Unable to find repository ({repositoryUrl}) in the dictionary. Skipping dispatch.");
                return;
            }

            _logger.LogInformation($"Script Path: {scriptPath}");

            Task.Run(() => RunScript(scriptPath, branch));
        }

        private static string GetBranch(string refs)
        {
            const string refsHeads = "refs/heads/";
            if (string.IsNullOrEmpty(refs) || !refs.StartsWith(refsHeads)) return null;
            return refs.Substring(refsHeads.Length);
        }

        private static string GetRepositoryUrl(string compare)
        {
            if (string.IsNullOrEmpty(compare)) return null;

            int compareIndex = compare.IndexOf("/compare", StringComparison.Ordinal);
            return compareIndex == -1 ? null : compare.Substring(0, compareIndex);
        }

        private void RunScript(string batchPath, string branch)
        {
            _logger.LogInformation($"Running the script ({batchPath}) associated with the repository.");

            var process = new Process
            {
                StartInfo =
                {
                    FileName         = batchPath,
                    Arguments        = branch,
                    WorkingDirectory = Path.GetDirectoryName(batchPath)
                }
            };

            process.Start();
            process.WaitForExit();

            _logger.LogInformation($"Finished executing the script ({batchPath}).");
        }
    }
}
