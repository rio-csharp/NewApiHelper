using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NewApiHelper.Data;
using NewApiHelper.Extensions;
using NewApiHelper.Services;
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

        // 确保数据库和表存在
        using (var scope = serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.EnsureCreated();
        }

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
            .SetBasePath(Directory.GetCurrentDirectory())
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

        string? apiBaseUrl = configuration["Api:BaseUrl"];
        string? apiToken = configuration["Api:Token"];
        string? apiUserId = configuration["Api:UserId"];
        string? dbConn = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(apiBaseUrl) || string.IsNullOrWhiteSpace(apiToken) || string.IsNullOrWhiteSpace(apiUserId) || string.IsNullOrWhiteSpace(dbConn))
        {
            MessageBox.Show("配置文件缺少必要的Api或数据库连接信息，请检查appsettings.json。", "配置错误", MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(1);
        }
        services.AddChannelHttpClient(apiBaseUrl!, apiToken!, apiUserId!);
        services.AddDatabase(dbConn!);
        services.AddTransient<ChannelManagementViewModel>();
        services.AddTransient<UpStreamChannelManagementViewModel>();
        services.AddTransient<IMessageService, MessageService>();
        // 优化View注入，自动注入ViewModel
        services.AddTransient<ChannelManagementView>(sp => new ChannelManagementView(sp.GetRequiredService<ChannelManagementViewModel>()));
        services.AddTransient<UpstreamManagementView>(sp => new UpstreamManagementView(sp.GetRequiredService<UpStreamChannelManagementViewModel>()));
        services.AddTransient<DataDisplayView>();
        services.AddTransient<SyncLogView>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MainWindow>();
    }
}