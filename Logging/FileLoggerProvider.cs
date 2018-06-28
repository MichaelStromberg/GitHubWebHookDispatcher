using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logging
{
    [ProviderAlias("File")]
    public class FileLoggerProvider : BatchingLoggerProvider
    {
        private readonly string _path;
        private readonly string _fileName;
        private readonly int? _maxFileSize;
        private readonly int? _maxRetainedFiles;

        public FileLoggerProvider(IOptions<FileLoggerOptions> options) : base(options)
        {
            FileLoggerOptions loggerOptions = options.Value;
            _path                           = loggerOptions.LogDirectory;
            _fileName                       = loggerOptions.FileName;
            _maxFileSize                    = loggerOptions.FileSizeLimit;
            _maxRetainedFiles               = loggerOptions.RetainedFileCountLimit;
        }

        protected override async Task WriteMessagesAsync(IEnumerable<LogMessage> messages, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(_path);

            // Group messages by log date
            foreach (var group in messages.GroupBy(GetGrouping))
            {
                string fullName = GetFullName(group.Key);
                var fileInfo    = new FileInfo(fullName);

                // If we've exceeded the max file size, don't write any logs
                if (_maxFileSize > 0 && fileInfo.Exists && fileInfo.Length > _maxFileSize) return;

                // Write the log messages to the file
                using (StreamWriter streamWriter = File.AppendText(fullName))
                {
                    foreach (LogMessage item in group)
                    {
                        await streamWriter.WriteAsync(item.Message);
                    }
                }
            }

            RollFiles();
        }

        private string GetFullName((int Year, int Month, int Day) group) => Path.Combine(_path, $"{_fileName}{@group.Year:0000}{@group.Month:00}{@group.Day:00}.txt");

        private static (int Year, int Month, int Day) GetGrouping(LogMessage message) => (message.Timestamp.Year, message.Timestamp.Month, message.Timestamp.Day);

        protected void RollFiles()
        {
            if (_maxRetainedFiles == null || _maxRetainedFiles.Value == 0) return;

            var files = new DirectoryInfo(_path)
                .GetFiles(_fileName + "*")
                .OrderByDescending(f => f.Name)
                .Skip(_maxRetainedFiles.Value);

            foreach (FileInfo item in files) item.Delete();
        }
    }
}
