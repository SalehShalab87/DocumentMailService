using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using DocumentMailService.Models;
using DocumentAutomation.Library.Services;
using DocumentAutomation.Library.Models;

namespace DocumentMailService.Services
{
    public interface IEmailService
    {
        Task<EmailResponse> SendEmailAsync(EmailRequest request);
        Task<EmailResponse> SendEmailWithEmbeddingAsync(EmailWithEmbeddingRequest request);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailConfiguration _emailConfig;
        private readonly ITemplateService _templateService;
        private readonly IDocumentGenerationService _documentService;
        private readonly IDocumentEmbeddingService _embeddingService;

        public EmailService(
            EmailConfiguration emailConfig,
            ITemplateService templateService,
            IDocumentGenerationService documentService,
            IDocumentEmbeddingService embeddingService)
        {
            _emailConfig = emailConfig;
            _templateService = templateService;
            _documentService = documentService;
            _embeddingService = embeddingService;
        }

        public async Task<EmailResponse> SendEmailAsync(EmailRequest request)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailConfig.FromName, _emailConfig.FromEmail));
            
            // Add TO recipients
            foreach (var to in request.To)
                message.To.Add(MailboxAddress.Parse(to));
            
            // Add CC recipients
            if (request.Cc != null)
            {
                foreach (var cc in request.Cc)
                    message.Cc.Add(MailboxAddress.Parse(cc));
            }
            
            // Add BCC recipients
            if (request.Bcc != null)
            {
                foreach (var bcc in request.Bcc)
                    message.Bcc.Add(MailboxAddress.Parse(bcc));
            }
            
            message.Subject = request.Subject;

            var bodyBuilder = new BodyBuilder();

            // Generate document if templateId is provided
            if (!string.IsNullOrEmpty(request.TemplateId))
            {
                var docRequest = new DocumentGenerationRequest
                {
                    TemplateId = request.TemplateId,
                    PlaceholderValues = request.TemplateValues.Select(kv => new PlaceholderValue
                    {
                        Placeholder = kv.Key,
                        Value = kv.Value
                    }).ToList(),
                    ExportFormat = request.ExportFormat
                };

                string generatedPath = _documentService.GenerateDocument(docRequest);

                if (request.ExportFormat == ExportFormat.HtmlEmail)
                {
                    bodyBuilder.HtmlBody = System.IO.File.ReadAllText(generatedPath);
                }
                else
                {
                    bodyBuilder.TextBody = request.BodyContent ?? "See attached document.";
                    bodyBuilder.Attachments.Add(generatedPath);
                }
            }
            else if (!string.IsNullOrEmpty(request.BodyContent))
            {
                bodyBuilder.TextBody = request.BodyContent;
            }

            // Add attachments
            if (request.Attachments != null)
            {
                foreach (var attachment in request.Attachments)
                {
                    if (!string.IsNullOrEmpty(attachment.Base64Content))
                    {
                        var data = Convert.FromBase64String(attachment.Base64Content);
                        bodyBuilder.Attachments.Add(attachment.FileName, data, ContentType.Parse(attachment.ContentType));
                    }
                    else if (!string.IsNullOrEmpty(attachment.FilePath) && System.IO.File.Exists(attachment.FilePath))
                    {
                        bodyBuilder.Attachments.Add(attachment.FilePath);
                    }
                }
            }

            message.Body = bodyBuilder.ToMessageBody();

            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.SmtpPort, _emailConfig.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                if (!string.IsNullOrEmpty(_emailConfig.SmtpUsername))
                {
                    await client.AuthenticateAsync(_emailConfig.SmtpUsername, _emailConfig.SmtpPassword);
                }
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return new EmailResponse
                {
                    Success = true,
                    Message = "Email sent successfully"
                };
            }
            catch (Exception ex)
            {
                return new EmailResponse
                {
                    Success = false,
                    Message = "Failed to send email",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<EmailResponse> SendEmailWithEmbeddingAsync(EmailWithEmbeddingRequest request)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailConfig.FromName, _emailConfig.FromEmail));
            
            // Add TO recipients
            foreach (var to in request.To)
                message.To.Add(MailboxAddress.Parse(to));
            
            // Add CC recipients
            if (request.Cc != null)
            {
                foreach (var cc in request.Cc)
                    message.Cc.Add(MailboxAddress.Parse(cc));
            }
            
            // Add BCC recipients
            if (request.Bcc != null)
            {
                foreach (var bcc in request.Bcc)
                    message.Bcc.Add(MailboxAddress.Parse(bcc));
            }
            
            message.Subject = request.Subject;
            var bodyBuilder = new BodyBuilder();

            try
            {
                // Create embedding request
                var embeddingRequest = new DocumentEmbeddingRequest
                {
                    MainTemplateId = request.MainTemplateId,
                    MainTemplateValues = request.MainTemplateValues.Select(kv => new PlaceholderValue
                    {
                        Placeholder = kv.Key,
                        Value = kv.Value
                    }).ToList(),
                    Embeddings = request.Embeddings.Select(e => new DocumentAutomation.Library.Models.EmbedInfo
                    {
                        EmbedTemplateId = e.EmbedTemplateId,
                        EmbedPlaceholder = e.EmbedPlaceholder,
                        EmbedTemplateValues = e.EmbedTemplateValues.Select(kv => new PlaceholderValue
                        {
                            Placeholder = kv.Key,
                            Value = kv.Value
                        }).ToList()
                    }).ToList(),
                    ExportFormat = request.ExportFormat
                };

                // Generate document with embeddings
                string generatedPath = _embeddingService.GenerateDocumentWithEmbedding(embeddingRequest);

                if (request.ExportFormat == ExportFormat.HtmlEmail)
                {
                    bodyBuilder.HtmlBody = System.IO.File.ReadAllText(generatedPath);
                }
                else
                {
                    bodyBuilder.TextBody = request.BodyContent ?? "Please see attached document with embedded content.";
                    bodyBuilder.Attachments.Add(generatedPath);
                }

                // Add additional attachments
                if (request.Attachments != null)
                {
                    foreach (var attachment in request.Attachments)
                    {
                        if (!string.IsNullOrEmpty(attachment.Base64Content))
                        {
                            var data = Convert.FromBase64String(attachment.Base64Content);
                            bodyBuilder.Attachments.Add(attachment.FileName, data, ContentType.Parse(attachment.ContentType));
                        }
                        else if (!string.IsNullOrEmpty(attachment.FilePath) && System.IO.File.Exists(attachment.FilePath))
                        {
                            bodyBuilder.Attachments.Add(attachment.FilePath);
                        }
                    }
                }

                message.Body = bodyBuilder.ToMessageBody();

                // Send email
                using var client = new SmtpClient();
                await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.SmtpPort, _emailConfig.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                if (!string.IsNullOrEmpty(_emailConfig.SmtpUsername))
                {
                    await client.AuthenticateAsync(_emailConfig.SmtpUsername, _emailConfig.SmtpPassword);
                }
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return new EmailResponse
                {
                    Success = true,
                    Message = "Email with embedded documents sent successfully",
                    GeneratedDocumentPath = generatedPath
                };
            }
            catch (Exception ex)
            {
                return new EmailResponse
                {
                    Success = false,
                    Message = "Failed to send email with embedded documents",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}