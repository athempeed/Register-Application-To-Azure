using ADReg.Code;
using ADReg.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ADReg.Controllers
{
    public class RegisterController : Controller
    {
        public IActionResult Index()
        {
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(ClientRegistrationInput input)
        {
            var tokenInput = new AzureAdInput
            {
                BaseUrl = "https://login.microsoftonline.com/8b24551d-7c2c-4beb-8b61-95f32d9929ef/oauth2/",
                ClientID = "2b5f2d47-87a0-4a89-9b53-44ff3649a8aa",
                ClientSeceret = "8~8kd1~_266YiXZrhOE.QUuqtwMtA2~OG0",
                GrantType = "client_credentials",
                Scope = "https://graph.microsoft.com/.default"
            };
            var token = await AzureAuthentication.Instance.GetAccessToken(tokenInput);
           var response =  await RegisterApplication(input.Name, token.AccessToken);
            ViewBag.Response = response;
            return View();
        }


        private async Task<RegisterResponse> RegisterApplication(string applicationName, string token)
        {
            using(HttpClient client = new HttpClient())
            {
                client.BaseAddress =new Uri("https://graph.microsoft.com/beta/");
                
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");

                var inp = ReadFile("createApplication.json");
                inp = inp.Replace("<%applicationName%>", applicationName);
                var stringContent = new StringContent(inp, Encoding.UTF8,"application/json");
                HttpResponseMessage response = await client.PostAsync("applications", stringContent);
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var applicationInfo = JsonConvert.DeserializeObject<AzureApplicationModel>(responseJson);
                    if(applicationInfo != null)
                    {
                        var servicePrincipal = await GetServicePrincipal(applicationInfo.ApplicationID, token);
                        if(servicePrincipal == null || servicePrincipal.Value.Count<=0)
                        {
                            var serviceprincipalInfo = await CreateServicePrincipal(applicationName, applicationInfo.ApplicationID,token);

                            
                            //servicePrincipal = await GetServicePrincipal("ec9c5bb5-261d-4086-b111-b85ba91a8a21", token);
                            //if (serviceprincipalInfo != null)
                            //{
                            //    await GiveOAuthPermissions(serviceprincipalInfo.ID, servicePrincipal.Value.FirstOrDefault().ID, "App.Read", token);
                            //}
                            var secret = await GenerateClientSecret(applicationInfo.ID, token);

                            return new RegisterResponse
                            {
                                ApplicationID = applicationInfo.ApplicationID,
                                ClientSecret = secret,
                                TenantID = "8b24551d-7c2c-4beb-8b61-95f32d9929ef",
                                ServicePrincipal = JsonConvert.SerializeObject(serviceprincipalInfo)
                            };
                        }
                    }
                }
            }

            return null;
        }

        private async Task<ServicePrincipalValue> CreateServicePrincipal(string appName,string appID,string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://graph.microsoft.com/beta/");

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");

                var inp = ReadFile("createServiceprincipal.json");
                inp = inp.Replace("<%applicationName%>", appName).Replace("<%appID%>",appID);
                
                var stringContent = new StringContent(inp, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("serviceprincipals", stringContent);
                if (response.IsSuccessStatusCode)
                {
                    var responseJSON = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ServicePrincipalValue>(responseJSON);
                }
            }
            return null;
        }
        private async Task<ServicePrincipalModel> GetServicePrincipal(string appID, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://graph.microsoft.com/beta/");

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
                HttpResponseMessage response = await client.GetAsync("serviceprincipals?$filter=appId%20eq%20'"+appID+"'");
                if (response.IsSuccessStatusCode)
                {
                    var responseJSON = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ServicePrincipalModel>(responseJSON);
                }
            }
            return null;
        }

        private async Task GiveOAuthPermissions(string appID,string resourceID,string scopeName,string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://graph.microsoft.com/beta/");

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");

                var inp = ReadFile("grantOAuthPermission.json");
                inp = inp.Replace("<%appID%>", appID).Replace("<%resourceID%>", resourceID).Replace("<%scopeName%>",scopeName);

                var stringContent = new StringContent(inp, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("oauth2PermissionGrants", stringContent);   
            }
        }
        
        private string ReadFile(string fileName)
        {
            var location = Assembly.GetExecutingAssembly().Location.Substring(0, Assembly.GetExecutingAssembly().Location.LastIndexOf("\\"));
            location = Path.Combine(location+"\\files", fileName);
            using (StreamReader sr = new StreamReader(location))
            {
                return sr.ReadToEnd();
            }
            return null;
        }


        private async Task<string> GenerateClientSecret(string appID,string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://graph.microsoft.com/beta/");

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");

                var inp = ReadFile("createClientSecret.json");
                

                var stringContent = new StringContent(inp, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync($"applications/{appID}/addPassword", stringContent);
                if (response.IsSuccessStatusCode)
                {
                    var responseJSON = await response.Content.ReadAsStringAsync();
                    var secretModel = JsonConvert.DeserializeObject<ClientSecretModel>(responseJSON);
                    return secretModel.SecretText;
                }
            }
            return null;
        }
    }
}
