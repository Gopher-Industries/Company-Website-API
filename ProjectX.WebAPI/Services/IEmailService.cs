using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Net.Mail;
using System.Net;
using SendGrid.Helpers.Mail;
using SendGrid;
using ProjectX.WebAPI.Models.Database.Authentication;

namespace ProjectX.WebAPI.Services
{
    public interface IEmailService
    {

        public Task SendConfirmationEmail(UserModel User);

    }

    public class GmailService : IEmailService
    {

        static string[] Scopes = { Google.Apis.Gmail.v1.GmailService.Scope.GmailSend };
        static string ApplicationName = "Gmail API .NET Quickstart";

        public async Task SendConfirmationEmail(UserModel User)
        {

            //
            // https://www.twilio.com/blog/send-emails-using-the-sendgrid-api-with-dotnetnet-6-and-csharp
            //

            var apiKey = "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"; // Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("tester.noreply.gopherindustries@gmail.com", "No Reply"),
                Subject = "Email Verification",
                PlainTextContent = "Please click the link to confirm your email: https://localhost:7080/api/v1/registration/validateemail/UserId=" + User.UserId
            };
            msg.AddTo(new EmailAddress(User.Email, User.Username));
            var response = await client.SendEmailAsync(msg);

            // A success status code means SendGrid received the email request and will process it.
            // Errors can still occur when SendGrid tries to send the email. 
            // If email is not received, use this URL to debug: https://app.sendgrid.com/email_activity 
            Console.WriteLine(response.IsSuccessStatusCode ? "Email queued successfully!" : "Something went wrong!");

        }

        /// <summary>
        /// The old way of sending confirmation emails
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public async Task SendConfirmationEmailOld(UserModel User)
        {
            try
            {
                UserCredential credential;
                // Load client secrets.
                using (var stream =
                       new FileStream("Credentials\\prototypeprojectx-gmail-access.json", FileMode.Open, FileAccess.Read))
                {
                    /* The file token.json stores the user's access and refresh tokens, and is created
                     automatically when the authorization flow completes for the first time. */

                    string credPath = "Credentials\\token.json";
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;

                    Console.WriteLine("Credential file saved to: " + credPath);
                }

                // Create Gmail API service.
                var service = new Google.Apis.Gmail.v1.GmailService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName
                });

                var Request = service.Users.Messages.Send(new Message()
                {
                    Id = Guid.NewGuid().ToString(),
                    Payload = new MessagePart()
                    {
                        Headers = new List<MessagePartHeader>()
                        {
                            new MessagePartHeader()
                            {
                                Name = "To",
                                Value = User.Email
                            },
                            new MessagePartHeader()
                            {
                                Name = "From",
                                Value = "backend-access@prototypeprojectx.iam.gserviceaccount.com"
                            },
                            new MessagePartHeader()
                            {
                                Name = "Subject",
                                Value = "Email Confirmation"
                            }
                        },
                        Body = new MessagePartBody()
                        {
                            Data = "Please click the link to confirm your email: https://localhost:7080/api/v1/registration/UserId=" + User.UserId
                        },
                    }
                }, "nathan@johansen.ws");

                var Result = await Request.ExecuteAsync().ConfigureAwait(false);

            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
