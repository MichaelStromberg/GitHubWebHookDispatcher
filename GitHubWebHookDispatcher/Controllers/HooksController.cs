using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GitHubWebHookDispatcher.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// Receives webhooks from a GitHub repo
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
    public class HooksController : ControllerBase
    {
        private readonly Dictionary<string, string> _repositoryToScriptPath;
        private readonly ILogger<HooksController> _logger;
        private readonly MD5 _md5Hash = MD5.Create();
        private readonly AnalysisRunner _runner;

        public HooksController(IOptions<ScriptRepositoryOptions> options, ILogger<HooksController> logger)
        {
            _repositoryToScriptPath = options.Value.RepositoryToScriptPath;
            _logger                 = logger;
            _runner                 = new AnalysisRunner(logger);
        }

        /// <summary>
        /// Submits a webhook
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///        "ref": "refs/heads/develop",
        ///        "compare": "https://git.illumina.com/Bioinformatics/Nirvana/compare/82040380f689...6176f4192726"
        ///     }
        ///
        /// </remarks>
        /// <param name="webhook"></param>
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

            string branch      = GetBranch(webhook.@ref);
            string key         = GetMd5Checksum($"{repositoryUrl}/{branch}");
            string shortBranch = GetShortBranch(branch);

            _logger?.LogInformation($"Repository URL: {repositoryUrl}, branch: {branch} ({shortBranch}), key: {key}");

            if (!_repositoryToScriptPath.TryGetValue(repositoryUrl, out string scriptPath))
            {
                _logger?.LogError($"Unable to find repository ({repositoryUrl}) in the dictionary. Skipping dispatch.");
                return;
            }

            _logger?.LogInformation($"Script Path: {scriptPath}");

            var config = new AnalysisConfiguration(scriptPath, branch, shortBranch, key);
            _runner.Queue(config);
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
            var sb   = new StringBuilder();
            var data = _md5Hash.ComputeHash(Encoding.ASCII.GetBytes(s));
            foreach (byte b in data) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
