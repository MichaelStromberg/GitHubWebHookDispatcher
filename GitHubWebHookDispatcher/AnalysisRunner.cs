using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace GitHubWebHookDispatcher
{
    public class AnalysisRunner
    {
        private readonly BlockingCollection<AnalysisConfiguration> _queue = new BlockingCollection<AnalysisConfiguration>();
        private readonly ILogger _logger;

        public AnalysisRunner(ILogger logger)
        {
            _logger = logger;
            var thread = new Thread(StartConsumer) { IsBackground = true };
            thread.Start();
        }

        public void Queue(AnalysisConfiguration config) => _queue.Add(config);

        private void StartConsumer()
        {
            _logger.LogInformation("Analysis runner is ready.");

            while (true)
            {
                AnalysisConfiguration config = _queue.Take();
                _logger.LogInformation($"Running the script ({config.ScriptPath}) associated with the repository.");

                if (!File.Exists(config.ScriptPath))
                {
                    _logger.LogError($"ERROR: The script path ({config.ScriptPath}) does not exist.");
                    continue;
                }

                var process = new Process
                {
                    StartInfo =
                    {
                        FileName         = config.ScriptPath,
                        Arguments        = $"{config.Branch} {config.ShortBranch} {config.Key}",
                        WorkingDirectory = Path.GetDirectoryName(config.Branch)
                    }
                };

                process.Start();
                process.WaitForExit();

                _logger.LogInformation($"Finished executing the script ({config.Branch}).");
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}
