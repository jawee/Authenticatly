using Authenticatly.Services.Interfaces;

namespace ExampleApi.Services;

public class SmsService : ISendSmsService
{
    public Task<bool> SendSms(string smscode, string phoneNumber, string userId)
    {
        return Task.FromResult(true);
    }
}
