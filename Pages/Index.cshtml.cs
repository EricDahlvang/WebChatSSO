using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace WebChatSSO.Pages
{
    public class IndexModel : PageModel
    {
        IConfiguration _configuration;

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            GetToken().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task GetToken()
        {
            var secret = _configuration.GetSection("ClientDirectLineSecret")?.Value;
            var endpoint = _configuration.GetSection("ClientDirectLineEndpoint")?.Value;
            ClientId = _configuration.GetSection("ClientId")?.Value;
            TenantId = _configuration.GetSection("TenantId")?.Value;

            UserId = "dl_" + Guid.NewGuid().ToString();

            using (HttpClient client = new HttpClient())
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{endpoint}/tokens/generate"))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", secret);
                    request.Headers.Add("Origin", "https://originbot.azurewebsites.net");
                    // TrustedOrigins = new string[] { "http://localhost:4275", "https://githubauthbotprod.azurewebsites.net/" }
                    request.Content = new StringContent(JsonConvert.SerializeObject(new { User = new { Id = UserId } }),
                                            Encoding.UTF8,
                                            "application/json");

                    var response = await client.SendAsync(request);
                    string token = String.Empty;

                    if (response.IsSuccessStatusCode)
                    {
                        var body = await response.Content.ReadAsStringAsync();
                        var tokenResponse = JsonConvert.DeserializeObject<DirectLineToken>(body);
                        Token = tokenResponse.token;
                        ConversationId = tokenResponse.conversationId;
                    }
                }
            }
            //var config = new ChatConfig()
            //{
            //    Token = token,
            //    Domain = endpoint,
            //    UserId = userId,
            //    ClientId = clientId,
            //    TenantId = tenantId
            //};
        }

        class DirectLineToken
        {
            public string userId { get; set; }
            public string conversationId { get; set; }
            public string token { get; set; }
            public int expires_in { get; set; }
            public string streamUrl { get; set; }
        }

        public string Token { get; set; }

        public string Domain { get; set; }

        public string UserId { get; set; }

        public string ClientId { get; set; }

        public string TenantId { get; set; }

        public string ConversationId { get; set; }
    }
}
