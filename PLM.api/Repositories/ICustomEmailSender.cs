namespace PLM.api.Repositories
{
    public interface ICustomEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
