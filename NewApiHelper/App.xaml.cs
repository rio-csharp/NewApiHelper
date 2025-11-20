using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NewApiHelper.Extensions;
using NewApiHelper.ViewModels;
using NewApiHelper.Views;
using Serilog;
using System.IO;
using System.Windows;

namespace NewApiHelper;

public partial class App : Application
{
    private ILogger<App> _logger = null!;
    public IServiceProvider serviceProvider { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        serviceProvider = services.BuildServiceProvider();

        _logger = serviceProvider.GetRequiredService<ILogger<App>>();

        _logger.LogInformation("应用启动");

        var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();

        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            var ex = (Exception)args.ExceptionObject;
            _logger.LogError(ex, "捕获未处理异常");
            Log.CloseAndFlush();
        };
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _logger.LogInformation("应用退出");
        Log.CloseAndFlush();
        base.OnExit(e);
    }

    private void ConfigureServices(ServiceCollection services)
    {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // 当前exe目录
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .Build();
        services.AddSingleton<IConfiguration>(configuration);
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(Log.Logger, dispose: true);
        });

        services.AddChannelHttpClient(configuration["Api:BaseUrl"] ?? throw new ArgumentNullException("Api:BaseUrl"),
            configuration["Api:Token"] ?? throw new ArgumentNullException("Api:Token"),
            configuration["Api:UserId"] ?? throw new ArgumentNullException("Api:UserId"));
        services.AddTransient<ChannelManagementViewModel>();
        services.AddTransient<ChannelManagementView>();
        services.AddTransient<CollectionConfigView>();
        services.AddTransient<DataDisplayView>();
        services.AddTransient<SyncLogView>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MainWindow>();
    }
}