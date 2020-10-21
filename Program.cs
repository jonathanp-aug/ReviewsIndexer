using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using ReviewsIndexer.Data;

namespace ReviewsIndexer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ProductIndexationTask task = new ProductIndexationTask(LocalProductIndex.Instance);
            task.Start();

            CreateHostBuilder(args).Build().Run();

            task.Stop();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
