using System.Reactive.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Poc.Impersonate.Consumers;
using Poc.Impersonate.Models.Keycloak;

namespace Poc.Impersonate.Controllers
{
    [ApiController]
    [Route("/api/keycloak")]
    public class KeycloakApiController
    {
        private KeycloakConsumer consumer;
        public KeycloakApiController(KeycloakConsumer consumer)
        {
            this.consumer = consumer;
        }

        [HttpGet("impersonate")]
        [AllowAnonymous]
        public async Task<KeycloakToken> Get([FromQuery] string username)
        {
            var adminToken = await consumer.Login("keycloak", "P@ssw0rd2023!");
            var user = await consumer.GetUser(username, adminToken.AccessToken);
            return await consumer.Impersonate(adminToken.AccessToken, user.Id);
        }

        [HttpGet("deprecated")]
        [AllowAnonymous]
        public async Task<string> GetDeprecated([FromQuery] string username)
        {
            var adminToken = await consumer.Login("keycloak", "P@ssw0rd2023!");
            var user = await consumer.GetUser(username, adminToken.AccessToken);
            return await consumer.ImpersonateDeprecated(adminToken.AccessToken, user.Id);
        }
    }
}
