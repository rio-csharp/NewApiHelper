using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NewApiHelper.Data;
using NewApiHelper.Services;
using System.Net.Http.Headers;

namespace NewApiHelper.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddChannelHttpClient(this IServiceCollection services, string baseApiUrl, string token, string userId)
    {
        services.AddHttpClient<IChannelService, ChannelService>(client =>
        {
            client.BaseAddress = new Uri(baseApiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Add("New-Api-User", userId);
        });
        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));
        services.AddScoped<IUpStreamChannelService, UpStreamChannelService>();
        return services;
    }
}