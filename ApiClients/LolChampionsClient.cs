using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using Objects;

namespace ApiClients
{
    public class LolChampionsClient
    {
        private static string champsUrl = "http://ddragon.leagueoflegends.com/cdn/6.24.1/data/en_US/champion.json";
        private HttpClient httpClient;
        private ChampionData champData;

        public LolChampionsClient()
        {
            this.httpClient = new HttpClient();
        }

        public async Task retrieveChampionData()
        {
            try	
            {
                string responseBody = await httpClient.GetStringAsync(champsUrl);
                var champData = JsonSerializer.Deserialize<ChampionData>(responseBody);
                this.champData = champData;
            }
            catch(HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");	
                Console.WriteLine("Message :{0} ",e.Message);
            }
        }

        public Champion retrieveChampionByKey(string key)
        {
            foreach(KeyValuePair<string, Champion> entry in this.champData.data) {
                if (entry.Value.key.Equals(key)) {
                    return entry.Value;
                }
            }

            return null;
        }
    }
}