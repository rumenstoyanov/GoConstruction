using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using GoLibrary.Data.Constants;
using GoLibrary.Data.Internals;
using MimeKit;
using Newtonsoft.Json;

namespace GoApp.Console
{
    public class ServiceBusConsumer : BackgroundService
    {
        private readonly IQueueClient _queueClient;
        private readonly MailSettings _mailSettings;
        public ServiceBusConsumer(IQueueClient queueClient, MailSettings mailSettings)
        {
            _queueClient = queueClient;
            _mailSettings = mailSettings;
        }


        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            RegisterOnMessageHandlerAndReceiveMessages();
            return Task.CompletedTask;
        }

        void RegisterOnMessageHandlerAndReceiveMessages()
        {
            // Configure the MessageHandler Options in terms of exception handling, number of concurrent messages to deliver etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of Concurrent calls to the callback `ProcessMessagesAsync`, set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                AutoComplete = false
            };

            // Register the function that will process messages
            _queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }




        async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            // Process the message
            var emailMessage = JsonConvert.DeserializeObject<EmailMessageDto>(Encoding.UTF8.GetString(message.Body));
            await SendMailAsync(emailMessage);


            // Complete the message so that it is not received again.
            // This can be done only if the queueClient is created in ReceiveMode.PeekLock mode (which is default).
            await _queueClient.CompleteAsync(message.SystemProperties.LockToken);

            // Note: Use the cancellationToken passed as necessary to determine if the queueClient has already been closed.
            // If queueClient has already been Closed, you may chose to not call CompleteAsync() or AbandonAsync() etc. calls 
            // to avoid unnecessary exceptions.
        }

        Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            System.Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            System.Console.WriteLine("Exception context for troubleshooting:");
            System.Console.WriteLine($"- Endpoint: {context.Endpoint}");
            System.Console.WriteLine($"- Entity Path: {context.EntityPath}");
            System.Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }

        async Task SendMailAsync(EmailMessageDto emailMessage)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_mailSettings.SenderName, _mailSettings.Email));
            message.To.Add(new MailboxAddress(emailMessage.ToName, emailMessage.ToAddress));
            message.Subject = emailMessage.Subject;
            message.Body = new TextPart("plain")
            {
                Text = emailMessage.Text
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_mailSettings.SmtpServer, _mailSettings.Port);

                client.AuthenticationMechanisms.Remove("XOAUTH2");

                await client.AuthenticateAsync(_mailSettings.Email, _mailSettings.Password);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }


    }
}
