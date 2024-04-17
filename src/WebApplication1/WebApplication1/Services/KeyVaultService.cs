using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace WebApplication1.Services
{
    public class KeyVaultService
    {
        private readonly IConfiguration _configuration;

        public KeyVaultService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Test()
        {
            try
            {
                string kvUri = _configuration["AppSettings:KeyValueEndpoint"];
                SecretClient client =
                    new SecretClient(new Uri(kvUri), new DefaultAzureCredential());

                string secret = client.GetSecretAsync("KVSecret").Result.Value.Value;
                return $"{_configuration["MySecret"]}+{secret}";
            }
            catch (Exception ex)
            {
                return ex.Message ;
            }
        }
    }
}
