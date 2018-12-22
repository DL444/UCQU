using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Model = DL444.UcquLibrary.Models;
using Newtonsoft.Json;

namespace UCqu
{
    static class WebClient
    {
        static string Host => "https://api.ucqu.dl444.net";
        static HttpClient client = new HttpClient();
        
        static WebClient()
        {
            client.BaseAddress = new Uri(Host);
        }

        public static async Task<Model.StaticDataModel> GetStaticDataAsync()
        {
            var response = await client.GetAsync("/api/StaticData");
            string modelJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Model.StaticDataModel>(modelJson);
        }
        public static async Task<string> LoginAsync(string userId, string passwordHash)
        {
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, "/api/Login");
            message.Content = new StringContent($"{{ \"UserId\": \"{userId}\", \"PasswordHash\": \"{passwordHash}\" }}");
            var response = await client.SendAsync(message);
            // 1: PasswordIncorrect, 2: ServerNetworkError, 3: ServersideFormatError, 4: SystemPreReg
            return await response.Content.ReadAsStringAsync();
        }
        public static async Task<Model.StudentInfo> GetStudentInfoAsync(string token)
        {
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, "/api/StudentInfo");
            message.Headers.Add("Cookie", $"token={token}");
            var response = await client.SendAsync(message);
            string responseString = await response.Content.ReadAsStringAsync();
            if(responseString == "1" || responseString == "2" || responseString == "3" || responseString == "4")
            {
                // 1: UserNotFound, 2: ServerNetworkError, 3: NoData, 4: SessionInvalid
                throw new RequestFailedException("Request failed.", null, int.Parse(responseString));
            }
            return JsonConvert.DeserializeObject<Model.StudentInfo>(responseString);
        }
        public static async Task<Model.Score> GetScoreAsync(string token, bool isMajor = true)
        {
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, $"/api/Score?isMajor={(isMajor ? "true" : "false")}");
            message.Headers.Add("Cookie", $"token={token}");
            var response = await client.SendAsync(message);
            string responseString = await response.Content.ReadAsStringAsync();
            if (responseString == "1" || responseString == "2" ||  responseString == "4")
            {
                // 1: UserNotFound, 2: ServerNetworkError, 3: NoData, 4: SessionInvalid
                throw new RequestFailedException("Request failed.", null, int.Parse(responseString));
            }
            else if(responseString == "3")
            {
                return new Model.Score("", "", 0, "", "", isMajor);
            }
            return JsonConvert.DeserializeObject<Model.Score>(responseString);
        }
        public static async Task<Model.Schedule> GetScheduleAsync(string token)
        {
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, "/api/Schedule");
            message.Headers.Add("Cookie", $"token={token}");
            var response = await client.SendAsync(message);
            string responseString = await response.Content.ReadAsStringAsync();
            if (responseString == "1" || responseString == "2" || responseString == "3" || responseString == "4")
            {
                // 1: UserNotFound, 2: ServerNetworkError, 3: NoData, 4: SessionInvalid
                throw new RequestFailedException("Request failed.", null, int.Parse(responseString));
            }
            return JsonConvert.DeserializeObject<Model.Schedule>(responseString);
        }
        public static async Task PostWnsChannelAsync(string token, string channel)
        {
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, "/api/WnsChannel");
            message.Headers.Add("Cookie", $"token={token}");
            message.Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("channel", channel) });

            try
            {
                var response = await client.SendAsync(message);
            }
            catch(Exception e)
            {
                throw new RequestFailedException("Wns channel post failed.", e);
            }
        }
        public static async Task ReportException(Exception ex)
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Message:");
                builder.AppendLine(ex.Message);
                builder.AppendLine("Stack Trace:");
                builder.AppendLine(ex.StackTrace);
                builder.AppendLine("Source:");
                builder.AppendLine(ex.Source);

                HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, "/api/ExceptionReport");
                message.Content = new StringContent(builder.ToString());
                await client.SendAsync(message);
            }
            catch (Exception) { }
        }
    }

    class RequestFailedException : Exception
    {
        public int Status { get; set; }

        public RequestFailedException() : base() { }
        public RequestFailedException(string message) : base(message) { }
        public RequestFailedException(string message, Exception innerException) : base(message, innerException) { }
        public RequestFailedException(string message, Exception innerException, int status) : base(message, innerException)
        {
            Status = status;
        }
    }
}
