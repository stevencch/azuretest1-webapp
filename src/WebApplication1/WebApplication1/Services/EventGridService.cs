using Azure;
using Azure.Messaging.EventGrid;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace WebApplication1.Services;

public class EventGridService
{
    private readonly string _endpoint = "";
    private readonly string _key = "";
    public EventGridService(IConfiguration configuration)
    {
        _endpoint = configuration["AppSettings:EventGridTopicEndPoint"];
        _key = configuration["AppSettings:EventGridTopicKey"];
    }

    public async Task Publish()
    {
        EventGridEvent acct = new EventGridEvent(
                        subject: "New Dev Event",
                        data: new DevMessage (DateTime.Now.ToString()),
                        eventType: "NewDevEventCreated",
        dataVersion: "1.0");

        EventGridPublisherClient client = new EventGridPublisherClient(new Uri(_endpoint), new AzureKeyCredential(_key));

        await client.SendEventAsync(acct);
    }
}
