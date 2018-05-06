using log4net;
using Newtonsoft.Json;
using TOYOTAPushService.Data.Helper;
using TOYOTAPushService.Data.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace TOYOTAPushService.Data.Service
{
    public class PushService
    {
        CommonHelper _commonHelper = new CommonHelper();
        ILog log = LogManager.GetLogger("Logger");
        public async void PushSend()
        {
            log.Info("PushSend Start");
            try
            {
                List<PushInfo> pushInfos = await GetPushInfo();
                if (pushInfos != null)
                {
                    foreach (var item in pushInfos)
                    {
                        _commonHelper.AndroidPush(item.Title, item.Content, item.TargetUser);
                        _commonHelper.IosPush(item.Title, item.Content, item.TargetUser);
                    }
                }
                log.Info("PushSend End");
            }
            catch (Exception ex)
            {
                log.Info("PushSend:" + ex.Message);
            }
        }

        public async Task<List<PushInfo>> GetPushInfo()
        {
            log.Info("GetPushInfo Start");
            List<PushInfo> pushs = new List<PushInfo>();
            try
            {
                var result = await _commonHelper.HttpGet<APIResult>(ConfigurationManager.AppSettings["API_BASEURL"], "/toyota/api/v1/Users/GetPushInfo");
                pushs = JsonConvert.DeserializeObject<List<PushInfo>>(result.Body);
                log.Info("GetPushInfo End");
            }
            catch (Exception ex)
            {
                log.Info("GetPushInfo:" + ex.Message);
            }
            return pushs;
        }
    }
}
