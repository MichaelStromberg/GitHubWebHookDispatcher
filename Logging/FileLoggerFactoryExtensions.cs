using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Logging
{
    /// <summary>
    /// Extensions for adding the <see cref="ILoggingBuilder" /> to the <see cref="FileLoggerProvider" />
    /// </summary>
    public static class FileLoggerFactoryExtensions
    {
        public static ILoggingBuilder AddFile(this ILoggingBuilder builder)
        {
            builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();
            return builder;
        }

        public static ILoggingBuilder AddFile(this ILoggingBuilder builder, string filename)
        {
            builder.AddFile(options => options.FileName = "log-");
            return builder;
        }

        public static ILoggingBuilder AddFile(this ILoggingBuilder builder, Action<FileLoggerOptions> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            builder.AddFile();
            builder.Services.Configure(configure);

            return builder;
        }
    }
}