using Common.Logging;
using TOYOTAPushService.Data;
using System.ServiceProcess;

namespace TOYOTAPushService
{
    public partial class TOYOTAPushService : ServiceBase
    {
        ILog log = LogManager.GetLogger("Logger");

        public TOYOTAPushService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            JobHelper.JobLoader();
            log.Info("TOYOTAPushService Started");
        }

        protected override void OnStop()
        {
            JobHelper.QuartzShutdown(false);
            log.Info("TOYOTAPushService Stoped");
        }
    }
}
