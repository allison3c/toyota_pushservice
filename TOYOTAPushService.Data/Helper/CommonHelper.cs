using JWT;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace TOYOTAPushService.Data.Helper
{
    public class CommonHelper
    {
        ILog log = LogManager.GetLogger("Logger");

        #region Push
        private static HttpClient _httpClient4Push;
        private static HttpClient CreateHttpClient4Push()
        {
            if (_httpClient4Push == null)
            {
                _httpClient4Push = new HttpClient();
                _httpClient4Push.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient4Push.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", ConfigurationManager.AppSettings["JPUSH_BASIC"]);
            }
            return _httpClient4Push;
        }
        private static HttpClient _httpClient4IOSPush;
        private static HttpClient CreateHttpClient4IOSPush()
        {
            if (_httpClient4IOSPush == null)
            {
                _httpClient4IOSPush = new HttpClient();
                _httpClient4IOSPush.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient4IOSPush.DefaultRequestHeaders.Add("ServiceBusNotification-Format", "apple");
                //_httpClient4IOSPush.DefaultRequestHeaders.Add("ServiceBusNotification-Tags", targetIOSUser);
            }
            return _httpClient4IOSPush;
        }
        public async void AndroidPush(string title, string content, string targetUser)
        {
            log.Info("AndroidPush Start");
            try
            {
                var httpClient = CreateHttpClient4Push();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://api.jpush.cn/v3/push");
                var msg = "{\"platform\": \"android\",\"audience\": {\"alias\":[" + targetUser + "]},"
                    + "\"message\": {\"msg_content\": \"" + content + "\",\"content_type\": \"text\",\"title\": \"" + title + "\",\"extras\": {\"key\": \"value\"}},"
                    + "\"options\": {\"time_to_live\": 60,\"apns_production\": false}}";
                request.Content = new StringContent(msg, Encoding.UTF8, "application/json");
                log.Info("Android-->" + msg);
                await httpClient.SendAsync(request);
                log.Info("AndroidPush End");
            }
            catch (Exception ex)
            {
                log.Info("AndroidPush:" + ex.Message);
            }
        }
        public async void IosPush(string title, string content, string targetUser)
        {
            log.Info("IosPush Start");
            try
            {
                //HttpClient httpClient = CreateHttpClient4Push();
                //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://api.jpush.cn/v3/push");
                //var msg = "{\"platform\": \"ios\",\"audience\": {\"alias\":[" + targetUser + "]},"
                //    + "\"notification\": {\"ios\": {\"alert\": \"" + title + "§" + content + "\",\"sound\": \"default\",\"badge\": \"+1\",\"extras\": {\"newsid\": 321}}},"
                //    + "\"options\": {\"time_to_live\": 60,\"apns_production\": true}}";
                //request.Content = new StringContent(msg, Encoding.UTF8, "application/json");
                //log.Info("IOS-->" + msg);
                //await httpClient.SendAsync(request);
                var targetIOSUser = targetUser.Replace(",", "||").Replace("\"", "");
                var shareAccessSig = createToken(ConfigurationManager.AppSettings["JPUSH_IOS_RESURI"], "DefaultFullSharedAccessSignature", ConfigurationManager.AppSettings["JPUSH_IOS_KEY"]);
                HttpClient httpClient = CreateHttpClient4IOSPush();
                httpClient.DefaultRequestHeaders.Remove("ServiceBusNotification-Tags");
                httpClient.DefaultRequestHeaders.Add("ServiceBusNotification-Tags", targetIOSUser);
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("SharedAccessSignature", shareAccessSig);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, ConfigurationManager.AppSettings["JPUSH_IOS_REQURI"]);
                //var msg = "{\"aps\":{\"alert\":\"" + title + "§" + content + "\"}}";
                var msg = "{\"aps\":{\"alert\":\"" + content + "\",\"sound\":\"default\",\"badge\":1}}";
                request.Content = new StringContent(msg, Encoding.UTF8, "application/json");
                log.Info("IOS-->" + msg);
                HttpResponseMessage message = await httpClient.SendAsync(request);
                log.Info("IosPush End");
            }
            catch (Exception ex)
            {
                log.Info("IosPush:" + ex.Message);
            }
        }
        #endregion

        #region PushInfo
        public async Task<T> HttpGet<T>(string baseUrl, string uri) where T : new()
        {
            var client = CreateHttpClient4Info();

            string endpoint = new Uri(new Uri(baseUrl), uri).ToString();
            log.Info("HttpGet endpoint:" + endpoint);
            var response = await client.GetAsync(endpoint).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(content);
            }
            return new T();
        }

        private static HttpClient _httpClient4Info;

        private static HttpClient CreateHttpClient4Info()
        {
            if (_httpClient4Info == null)
            {
                _httpClient4Info = new HttpClient();
                _httpClient4Info.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", GenerateToken());
            }
            return _httpClient4Info;
        }
        private static string GenerateToken()
        {
            var now = DateTime.UtcNow;
            var payload = new Dictionary<string, object>(){
                { "jti", Guid.NewGuid().ToString() },
                { "iat", ToUnixEpochDate(now).ToString()}};
            var secretKey = "toyotasupersecret_secretkey!123";
            string token = JsonWebToken.Encode(payload, secretKey, JwtHashAlgorithm.HS256);
            return token;
        }
        private static long ToUnixEpochDate(DateTime date)
        {
            return (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
        }
        #endregion

        private static string getBasic()
        {
            string encode = "";
            byte[] bytes = Encoding.GetEncoding("UTF-8").GetBytes("7ea762fe7e192e824514c1c2:c1a73fdbc31d452f7754fc55"); //将一组字符编码为一个字节序列. 
            try
            {
                encode = Convert.ToBase64String(bytes); //将8位无符号整数数组的子集转换为其等效的,以64为基的数字编码的字符串形式. 
            }
            catch
            {
            }
            return encode;
        }

        public static string createToken(string resourceUri, string keyName, string key)
        {
            TimeSpan sinceEpoch = DateTime.UtcNow - new DateTime(1970, 1, 1);
            var week = 60 * 60 * 24 * 7;
            var expiry = Convert.ToString((int)sinceEpoch.TotalSeconds + week);
            string stringToSign = HttpUtility.UrlEncode(resourceUri) + "\n" + expiry;
            HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
            var sasToken = String.Format(CultureInfo.InvariantCulture, "sr={0}&sig={1}&se={2}&skn={3}", HttpUtility.UrlEncode(resourceUri), HttpUtility.UrlEncode(signature), expiry, keyName);
            return sasToken;
        }
    }
}
