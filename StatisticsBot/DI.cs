using Microsoft.Extensions.DependencyInjection;
using StatisticsBot.Jobs;
using StatisticsBot.MessageHandlers;
using StatisticsBot.Services;
using StatisticsBot.Services.Data;
using Telegram.Bot;

namespace StatisticsBot;
public static class DI
{
    public static IServiceProvider Provider { get; private set; }

    public static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddDbContext<DataContext>(ServiceLifetime.Transient);
        services.AddBot();
        services.AddJobs();

        services.AddTransient<CommandHandler>();
        services.AddTransient<UpdateService>();
        services.AddTransient<RenderService>();

        return services.BuildServiceProvider();
    }

    private static void AddBot(this IServiceCollection services)
    {
        services.AddTransient<ITelegramBotClient>(x => new TelegramBotClient(Config.Instance.TelegramToken));
    }

    private static void AddJobs(this IServiceCollection services)
    {
        services.AddTransient<UpdateJob>();
        services.AddTransient<NotificationJob>();
    }
}
