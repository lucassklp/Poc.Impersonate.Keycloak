using Poc.Impersonate.Models.Keycloak;
using Rx.Http;
using Rx.Http.Extensions;
using Rx.Http.Logging;
using System.Reactive.Linq;

namespace Poc.Impersonate.Consumers
{
    public class KeycloakConsumer : RxHttpClient
    {
        private readonly string realm;
        private readonly string clientId;
        private HttpClient httpClient;
        public KeycloakConsumer(HttpClient httpClient, RxHttpLogger logger, IConfiguration configuration) : base(httpClient, logger)
        {

            httpClient.BaseAddress = new Uri(configuration.GetSection("Keycloak")
                .GetValue<string>("auth-server-url")!);
            
            this.realm = configuration.GetSection("Keycloak")
                .GetValue<string>("realm")!;

            this.clientId = configuration.GetSection("Keycloak")
                .GetValue<string>("resource")!;

            this.httpClient = httpClient;
        }

        public IObservable<KeycloakToken> Login(string username, string password)
        {
            var body = new Dictionary<string, string>();
            body.Add("client_id", clientId);
            body.Add("grant_type", "password");
            body.Add("username", username);
            body.Add("password", password);

            return this.Post<KeycloakToken>($"realms/{realm}/protocol/openid-connect/token", new FormUrlEncodedContent(body));
        }

        public IObservable<KeycloakUser> GetUser(string username, string token) 
        {
            return this.Get<List<KeycloakUser>>($"admin/realms/{realm}/users", options => options
                .AddQueryString("username", username)
                .UseBearerAuthorization(token)
            ).Select(x => x.First());
        }

        public async Task<String> ImpersonateDeprecated(string adminToken, string userId) 
        {
            var impersonate = await Post($"admin/realms/{realm}/users/{userId}/impersonation", options => options.UseBearerAuthorization(adminToken));
            
            var response = await Get($"realms/{realm}/protocol/openid-connect/auth", options => {
                options.AddQueryString(new 
                {
                    response_mode = "fragment",
                    response_type = "token",
                    client_id = clientId,
                    redirect_uri = "http://localhost:4200/"
                });

                impersonate.Headers.GetValues("Set-Cookie").ToList().ForEach(cookie => options.AddHeader("Cookie", cookie));
            });

            return response.RequestMessage.RequestUri!.Fragment;
        }

        public async Task<KeycloakToken> Impersonate(string adminToken, string userId) 
        {
            var response = await Post<KeycloakToken>($"realms/{realm}/protocol/openid-connect/token", new FormUrlEncodedContent(new Dictionary<string, string> 
            {
                { "grant_type", "urn:ietf:params:oauth:grant-type:token-exchange" },
                { "client_id", clientId },
                { "requested_subject", userId },
                { "subject_token", adminToken }
            }));

            return response;
        }
    }
}