using log4net;
using Quartz;
using TOYOTAPushService.Data.Service;
using System;

namespace TOYOTAPushService.Data.Jobs
{
    [DisallowConcurrentExecution]
    public class PushCronJob : IJob
    {
        ILog log = LogManager.GetLogger("Logger");
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                PushService _pushService = new PushService();
                 _pushService.PushSend();
            }
            catch (System.Exception ex)
            {
                log.Info(ex.Message);
            }
        }
    }
}
