namespace WebApplication1.Services
{
    public class RetryService
    {
        private readonly HttpClient _client;

        public RetryService(HttpClient client)
        {
            _client = client;
        }

        public async Task<string> Test()
        {
            var result = "done";
            try
            {
                var result1 = await _client.GetAsync("/getException");
            }
            catch (ApplicationException)
            {
                return "We cannot get cars from the server. Please try again";
            }

            catch (Polly.CircuitBreaker.BrokenCircuitException)
            {
                return "Server is busy... Please wait, we will try to send the request again soon...";
            }
            return result;
        }
    }
}
