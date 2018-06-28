using System.Diagnostics;
using System.IO;
using System;
using System.Reflection;
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
            string pathToExe         = Assembly.GetExecutingAssembly().Location;
            string pathToContentRoot = Path.GetDirectoryName(pathToExe);
			
			Console.WriteLine($"Content root: {pathToContentRoot}"); 

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
