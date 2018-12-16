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
        public static async Task<Model.Score> GetScoreAsync(string token)
        {
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, "/api/Score");
            message.Headers.Add("Cookie", $"token={token}");
            var response = await client.SendAsync(message);
            string responseString = await response.Content.ReadAsStringAsync();
            if (responseString == "1" || responseString == "2" || responseString == "3" || responseString == "4")
            {
                // 1: UserNotFound, 2: ServerNetworkError, 3: NoData, 4: SessionInvalid
                throw new RequestFailedException("Request failed.", null, int.Parse(responseString));
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
