// MyanvieBE/Services/IEmailService.cs
using System.Threading.Tasks;

namespace MyanvieBE.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
    }
}