using System;

namespace Logging
{
    public class FileLoggerOptions : BatchingLoggerOptions
    {
        private int? _fileSizeLimit          = 10 * 1024 * 1024;
        private int? _retainedFileCountLimit = 3;
        private string _fileName             = "logs-";

        public int? FileSizeLimit
        {
            get => _fileSizeLimit;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(FileSizeLimit)} must be positive.");
                }
                _fileSizeLimit = value;
            }
        }

        public int? RetainedFileCountLimit
        {
            get => _retainedFileCountLimit;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(RetainedFileCountLimit)} must be positive.");
                }
                _retainedFileCountLimit = value;
            }
        }

        public string FileName
        {
            get => _fileName;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException(nameof(value));
                }
                _fileName = value;
            }
        }

        public string LogDirectory { get; set; } = "Logs";
    }
}