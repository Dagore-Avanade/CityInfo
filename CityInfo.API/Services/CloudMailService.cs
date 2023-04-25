﻿namespace CityInfo.API.Services
{
    public class CloudMailService : IMailService
    {
        string mailTo = "admin@mycompany.com";
        string mailFrom = "noreply@mycompany.com";

        public void Send(string subject, string message)
        {
            // send mail -  output to console window
            Console.WriteLine($"Mail from {mailFrom} to {mailTo} with {nameof(CloudMailService)}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Message: {message}");
        }
    }
}
