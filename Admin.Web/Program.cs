using Serilog;

namespace Admin.Web
{
    public class Program
    {
        public async static Task<int> Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Async(c => c.File("Logs/logs.txt"))
                .WriteTo.Async(c => c.Console())
                .CreateBootstrapLogger();

            try
            {
                Log.Information("Starting web host.");
                var builder = WebApplication.CreateBuilder(args);
                builder.Host
                    .AddAppSettingsSecretsJson()
                    .UseAutofac()
                    .UseSerilog((context, services, loggerConfiguration) =>
                    {
                        loggerConfiguration
                            .ReadFrom.Configuration(context.Configuration)
                            .ReadFrom.Services(services);
                    });
                await builder.AddApplicationAsync<AdminWebModule>();
                var app = builder.Build();
                await app.InitializeApplicationAsync();
                await app.RunAsync();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly!");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}


//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddControllersWithViews();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseRouting();

//app.UseAuthorization();

//app.MapStaticAssets();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}")
//    .WithStaticAssets();


//app.Run();
