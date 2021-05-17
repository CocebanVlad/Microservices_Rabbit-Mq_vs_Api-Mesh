using Client.WebApp.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Client
{
    public class ClientService
    {
        private readonly string TEXT_PROCESSOR_URL = Environment.GetEnvironmentVariable("TEXT_PROCESSOR_URL");
        private readonly string BUBBLE_SORT_URL = Environment.GetEnvironmentVariable("BUBBLE_SORT_URL");

        public async Task<List<string>> SplitAndSortWordsUsingWebAPIsAsync(string text)
        {
            using (var client = new HttpClient())
            {
                HttpResponseMessage resp;

                resp = await client.PostAsync(CombineUrlParts(TEXT_PROCESSOR_URL, "/api/v1/Split"), JsonContent.Create(text));
                resp.EnsureSuccessStatusCode();
                var words = await resp.Content.ReadFromJsonAsync<List<string>>();

                resp = await client.PostAsJsonAsync(CombineUrlParts(BUBBLE_SORT_URL, "/api/v1/sort"), words);
                resp.EnsureSuccessStatusCode();

                return await resp.Content.ReadFromJsonAsync<List<string>>();
            }
        }

        public async Task<List<string>> SplitAndSortWordsUsingRabbitMQAsync(string text)
        {
            var wordsArrayAsJson = await MqClient.Default.CallAsync("text-processor", JsonConvert.SerializeObject(text));
            return JsonConvert.DeserializeObject<List<string>>(await MqClient.Default.CallAsync("bubble-sort", wordsArrayAsJson));
        }

        public string CombineUrlParts(params string[] parts) => string.Join("/", parts.Select(p => p.Trim('/')));
    }
}