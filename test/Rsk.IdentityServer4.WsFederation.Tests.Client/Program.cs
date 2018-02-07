using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace Rsk.IdentityServer4.WsFederation.Tests.Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls("http://localhost:5001")
                .Build();

            host.Run();
        }
    }
}
