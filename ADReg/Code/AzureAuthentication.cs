using ADReg.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ADReg.Code
{
    public class AzureAuthentication
    {
        private static Lazy<AzureAuthentication> _instance = new Lazy<AzureAuthentication>();

        public static AzureAuthentication Instance
        {
            get
            {
                return _instance.Value;
            }
        }


        private static DateTime _expireTime;
        private static string _accessToken;
        private AzureTokenModel tokenModel;


        public async Task<AzureTokenModel> GetAccessToken(AzureAdInput input)
        {
            try
            {
                if (_expireTime == DateTime.MinValue || _expireTime < DateTime.Now)
                {

                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(input.BaseUrl);
                        client.DefaultRequestHeaders.Accept.Clear();

                        var requestInput = new Dictionary<string,string>()
                        {
                            { "client_id", input.ClientID },
                            { "client_secret",input.ClientSeceret },
                            { "grant_type",input.GrantType },
                            { "scope",input.Scope }
                        };

                        var stringContent = new StringContent(JsonConvert.SerializeObject(requestInput), Encoding.UTF8);
                       
                        
                        HttpContent content = new FormUrlEncodedContent(requestInput);

                        HttpResponseMessage response = await client.PostAsync("v2.0/token", content);
                    
                        if (response.IsSuccessStatusCode)
                        {
                            var tokenStr = await response.Content.ReadAsStringAsync();
                            tokenModel = JsonConvert.DeserializeObject<AzureTokenModel>(tokenStr);
                            _expireTime = DateTime.Now.AddSeconds(tokenModel.ExpiresIn);
                            _accessToken = tokenModel.AccessToken;
                            return tokenModel;
                        }

                    }
                                          
                }

            }
            catch (Exception)
            {

            }
            return tokenModel;
        }
    }
}
