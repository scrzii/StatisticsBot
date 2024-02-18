using Quartz;
using StatisticsBot.Services;

namespace StatisticsBot.Jobs;

public class UpdateJob : IJob
{
    private readonly UpdateService _service;

    public UpdateJob(UpdateService service)
    {
        _service = service;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await _service.UpdateUsers();
            await _service.UpdateChart();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Job exception: {ex.Message}");
        }
    }
}
