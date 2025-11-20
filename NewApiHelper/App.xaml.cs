using Microsoft.Extensions.Configuration;
using Serilog;
using System.IO;
using System.Windows;

namespace NewApiHelper;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ILogger _logger;
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        // 1. 读取appsettings.json配置
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // 当前exe目录
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        // 2. 初始化Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
        _logger = Log.Logger;
        _logger.Information("应用启动");
        // TODO: 这里继续你的启动流程

        // 可以考虑捕获全局异常，并写入日志
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            var ex = (Exception)args.ExceptionObject;
            _logger.Fatal(ex, "捕获未处理异常");
            Log.CloseAndFlush();
        };
    }
    protected override void OnExit(ExitEventArgs e)
    {
        _logger.Information("应用退出");
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
