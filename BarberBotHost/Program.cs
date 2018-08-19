using Microsoft.AspNetCore.Hosting;

namespace BarberBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new WebHostBuilder()
               .UseUrls("https://+:5001")
               .UseKestrel()
               .UseStartup<Startup>()
               .Build()
               .Run();
        }
    }
}
