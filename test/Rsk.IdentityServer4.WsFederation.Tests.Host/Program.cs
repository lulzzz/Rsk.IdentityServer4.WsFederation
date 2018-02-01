using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace Rsk.IdentityServer4.WsFederation.Tests.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls("http://localhost:5000")
                .Build();

            host.Run();
        }
    }
}
