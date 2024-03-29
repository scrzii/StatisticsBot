using Quartz;
using StatisticsBot.Services;
using System.Runtime.Serialization;

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
            await _service.UpdateAll();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Job exception: {ex}");
        }
    }
}
