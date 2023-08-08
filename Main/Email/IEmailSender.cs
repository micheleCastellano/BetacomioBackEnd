namespace Main.EmailSender
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string name, string pwd);
    }
}