using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reportz.Scripting;
using Reportz.Scripting.Classes;
using Reportz.Scripting.Interfaces;

namespace Reportz.Plugin.ProjectPlace
{
    public class TimeReportExtractor : IExecutable
    {
        public TimeReportExtractor()
        {
            Url = "https://service.projectplace.com/";
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public string Url { get; set; }

        public IExecutableResult Execute(IExecutableArgs args)
        {
            // do stuff...

            // todo:
            // ---TEMPORARY---
            if (Username != null && Username.StartsWith("$"))
                Username = args.Scope.GetVariable(Username.Substring(1))?.Value?.ToString() ?? Username;
            if (Password != null && Password.StartsWith("$"))
                Password = args.Scope.GetVariable(Password.Substring(1))?.Value?.ToString() ?? Password;
            if (Url != null && Url.StartsWith("$"))
                Url = args.Scope.GetVariable(Url.Substring(1))?.Value?.ToString() ?? Url;
            // ---END TEMPORARY---

            try
            {
                var handler = new HttpClientHandler();
                //handler.Credentials = 
                handler.CookieContainer = new CookieContainer();

                var client = new HttpClient(handler);
                client.BaseAddress = new Uri("https://service.projectplace.com/");


                var authenticate = AuthenticateUser(client);
                if (!authenticate || handler.CookieContainer.Count <= 0)
                {
                    throw new Exception("Error authenticating user to ProjectPlace.");
                }

                DateTime startDate, endDate;
                if (!DateTime.TryParse(args.Arguments?.FirstOrDefault(x => x.Key == "startDate")?.Value?.ToString(), out startDate))
                {
                    throw new ArgumentException("Error getting paramter 'startDate'.", "startDate");
                }
                if (!DateTime.TryParse(args.Arguments?.FirstOrDefault(x => x.Key == "endDate")?.Value?.ToString(), out endDate))
                {
                    throw new ArgumentException("Error getting paramter 'endDate'.", "endDate");
                }
                
                var timeReports = GetTimeReports(client, startDate, endDate);

                var result = args.CreateResult(timeReports);
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        protected virtual bool AuthenticateUser(HttpClient client)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "https://service.projectplace.com/login");
                request.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                request.Headers.Host = "service.projectplace.com";
                request.Headers.Referrer = new Uri("https://service.projectplace.com/login?lang=english");
                
                var nameValueCollection = new Dictionary<string, string>();
                nameValueCollection.Add("uname", Username);
                nameValueCollection.Add("password", Password);
                nameValueCollection.Add("keepmeloggedin", "yes");
                nameValueCollection.Add("lang", "english");
                request.Content = new FormUrlEncodedContent(nameValueCollection);

                var task = client.SendAsync(request);
                task.Wait();
                var response = task.Result;
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                throw;
                return false;
            }
        }
        

        protected virtual JArray GetTimeReports(HttpClient client, DateTime startDate, DateTime endDate)
        {
            try
            {
                //var requestUri = new Uri($"https://service.projectplace.com/api/v1/timereports/users/me?from_date={startDate.ToString("yyyy-MM-dd")}&to_date={endDate.ToString("yyyy-MM-dd")}&grid=1&_=1484511005385");
                var requestUri = "https://service.projectplace.com/api/v1/timereports/users/me";
                requestUri += $"?from_date={startDate.ToString("yyyy-MM-dd")}" +
                              $"&to_date={endDate.ToString("yyyy-MM-dd")}" +
                              $"&grid=1";

                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.Accept.ParseAdd("application/json, text/javascript, */*; q=0.01");
                request.Headers.Host = "service.projectplace.com";
                request.Headers.Referrer = new Uri("https://service.projectplace.com/");
                
                var responseTask = client.SendAsync(request);
                responseTask.Wait();
                var response = responseTask.Result;
                response.EnsureSuccessStatusCode();

                var contentTask = response.Content.ReadAsStringAsync();
                contentTask.Wait();
                var content = contentTask.Result;

                var settings = JsonConvert.DefaultSettings?.Invoke() ?? new JsonSerializerSettings();
                var result = JsonConvert.DeserializeObject<JArray>(content, settings);

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
