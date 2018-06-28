using System.Diagnostics;
using System.IO;
using Logging;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;

namespace GitHubWebHookDispatcher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string pathToExe         = Process.GetCurrentProcess().MainModule.FileName;
            string pathToContentRoot = Path.GetDirectoryName(pathToExe);

            IWebHost host = WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(pathToContentRoot)
                .ConfigureLogging(builder => builder.AddFile())
                .UseStartup<Startup>()
                .UseUrls("http://0.0.0.0:7000/")
                .Build();

            host.RunAsService();
        }
    }
}
