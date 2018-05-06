using Common.Logging;
using Quartz;
using TOYOTAPushService.Data.Service;

namespace TOYOTAPushService.Data.Jobs
{
    [DisallowConcurrentExecution]
    public class SimpleJob : IJob
    {
        ILog log = LogManager.GetLogger("Logger");
        public void Execute(IJobExecutionContext context)
        {
            log.Info("SimpleJob");
        }

    }


}
