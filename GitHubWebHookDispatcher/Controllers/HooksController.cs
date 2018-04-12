using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
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

        private static readonly Dictionary<string, object> KeyToLockObjects = new Dictionary<string, object>();
        private readonly MD5 _md5Hash = MD5.Create();
        private readonly object _lock = new object();

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
                lock(_lock) _logger.LogDebug("Webhook is null. Skipping dispatch.");
                return;
            }

            string repositoryUrl = GetRepositoryUrl(webhook.compare);

            if (repositoryUrl == null)
            {
                lock (_lock) _logger.LogDebug("repositoryUrl is null. Skipping dispatch.");
                return;
            }

            string branch      = GetBranch(webhook.@ref);
            string key         = GetMd5Checksum($"{repositoryUrl}/{branch}");
            string shortBranch = GetShortBranch(branch);

            lock (_lock) _logger?.LogInformation($"Repository URL: {repositoryUrl}, branch: {branch} ({shortBranch}), key: {key}");

            if (!_repositoryToScriptPath.TryGetValue(repositoryUrl, out string scriptPath))
            {
                lock (_lock) _logger?.LogError($"Unable to find repository ({repositoryUrl}) in the dictionary. Skipping dispatch.");
                return;
            }

            lock (_lock) _logger?.LogInformation($"Script Path: {scriptPath}");

            Task.Run(() => RunScript(scriptPath, branch, shortBranch, key));
        }

        private static string GetBranch(string refs)
        {
            const string refsHeads = "refs/heads/";
            if (string.IsNullOrEmpty(refs) || !refs.StartsWith(refsHeads)) return null;
            return refs.Substring(refsHeads.Length);
        }

        private static string GetShortBranch(string branch)
        {
            if (string.IsNullOrEmpty(branch)) return null;
            int slashIndex = branch.LastIndexOf("/", StringComparison.Ordinal);
            return slashIndex == -1 ? branch : branch.Substring(slashIndex + 1);
        }

        private static string GetRepositoryUrl(string compare)
        {
            if (string.IsNullOrEmpty(compare)) return null;

            int compareIndex = compare.IndexOf("/compare", StringComparison.Ordinal);
            return compareIndex == -1 ? null : compare.Substring(0, compareIndex);
        }

        private string GetMd5Checksum(string s)
        {
            var sb = new StringBuilder();
            var data = _md5Hash.ComputeHash(Encoding.ASCII.GetBytes(s));
            foreach (byte b in data) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        /// <summary>
        /// Runs the analysis script
        /// </summary>
        /// <param name="batchPath">Used to execute the right script</param>
        /// <param name="branch">Used for git checkout</param>
        /// <param name="shortBranch">Used for the title of the project</param>
        /// <param name="key">Used to store the project in SonarQube</param>
        private void RunScript(string batchPath, string branch, string shortBranch, string key)
        {
            if (!KeyToLockObjects.TryGetValue(batchPath, out object lockObject))
            {
                lockObject = new object();
                KeyToLockObjects[batchPath] = lockObject;
            }

            // handle different repos in parallel, but wait if the same repo is being analyzed
            lock (lockObject)
            {
                _logger.LogInformation($"Running the script ({batchPath}) associated with the repository.");

                var process = new Process
                {
                    StartInfo =
                    {
                        FileName         = batchPath,
                        Arguments        = $"{branch} {shortBranch} {key}",
                        WorkingDirectory = Path.GetDirectoryName(batchPath)
                    }
                };

                process.Start();
                process.WaitForExit();

                _logger.LogInformation($"Finished executing the script ({batchPath}).");
            }
        }
    }
}
