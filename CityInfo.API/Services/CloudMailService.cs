namespace CityInfo.API.Services
{
    public class CloudMailService : IMailService
    {
        readonly string mailTo = string.Empty;
        readonly string mailFrom = string.Empty;

        public CloudMailService(IConfiguration configuration)
        {
            mailTo = configuration["mailSettings:mailToAddress"];
            mailFrom = configuration["mailSettings:mailFromAddress"];
        }

        public void Send(string subject, string message)
        {
            // send mail -  output to console window
            Console.WriteLine($"Mail from {mailFrom} to {mailTo} with {nameof(CloudMailService)}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Message: {message}");
        }
    }
}
