using System.Net;
using System.Net.Mail;

namespace Main.EmailSender
{
    public class EmailSender : IEmailSender
    {

        public async Task SendEmailAsync(string email, string name, string pwd)
        {
            string message = $"Gentile {name}, " +
                $"\n All'interno di questo di questa password troverà una " +
                $"password provvisoria con cui potrà accedere nuovamento al suo profilo." +
                $"\nLa invitiamo a cambiarla con una nuova di sua preferenza il prima possibile." +
                $"\n " +
                $"\nPassword: {pwd}" +
                $"\n " +
                $"\nCi scusiamo per il disagio." +
                $"\nCordialmente," +
                $"\nBetacomio Torino";

            string subject = "Betacomio Torino, password provvisoria";

            await ExecuteSending(email, subject, message);
        }
        private Task ExecuteSending(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("betacomio.torino@gmail.com", "siryyttbumgmzsic")
            };

            return client.SendMailAsync(
                new MailMessage(from: "betacomio.torino@gmail.com",
                                to: "betacomio.torino@gmial.com",//non metto email perchè le email nel db non esistono davvero
                                subject,
                                message
                                ));
        }
    }

}
