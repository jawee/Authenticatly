namespace Authenticatly.Services.Interfaces;

public interface ISendSmsService
{
    Task<bool> SendSms(string smscode, string phoneNumber, string userId);
}
