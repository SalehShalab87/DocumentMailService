using System.Text.Json.Serialization;
using DocumentAutomation.Library.Models;

namespace DocumentMailService.Models
{
    public class EmailRequest
    {
        [JsonPropertyName("to")]
        public List<string> To { get; set; } = new();

        [JsonPropertyName("cc")]
        public List<string>? Cc { get; set; }

        [JsonPropertyName("bcc")]
        public List<string>? Bcc { get; set; }

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("templateId")]
        public string? TemplateId { get; set; }

        [JsonPropertyName("templateValues")]
        public Dictionary<string, string> TemplateValues { get; set; } = new();

        [JsonPropertyName("bodyContent")]
        public string? BodyContent { get; set; }

        [JsonPropertyName("exportFormat")]
        public ExportFormat ExportFormat { get; set; } = ExportFormat.HtmlEmail;

        [JsonPropertyName("attachments")]
        public List<AttachmentRequest>? Attachments { get; set; }
    }

    public class EmailWithEmbeddingRequest
    {
        [JsonPropertyName("to")]
        public List<string> To { get; set; } = new();

        [JsonPropertyName("cc")]
        public List<string>? Cc { get; set; }

        [JsonPropertyName("bcc")]
        public List<string>? Bcc { get; set; }

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("mainTemplateId")]
        public string MainTemplateId { get; set; } = string.Empty;

        [JsonPropertyName("mainTemplateValues")]
        public Dictionary<string, string> MainTemplateValues { get; set; } = new();

        [JsonPropertyName("embeddings")]
        public List<EmbedInfo> Embeddings { get; set; } = new();

        [JsonPropertyName("bodyContent")]
        public string? BodyContent { get; set; }

        [JsonPropertyName("exportFormat")]
        public ExportFormat ExportFormat { get; set; } = ExportFormat.Pdf;

        [JsonPropertyName("attachments")]
        public List<AttachmentRequest>? Attachments { get; set; }
    }

    public class EmbedInfo
    {
        [JsonPropertyName("embedTemplateId")]
        public string EmbedTemplateId { get; set; } = string.Empty;

        [JsonPropertyName("embedPlaceholder")]
        public string EmbedPlaceholder { get; set; } = string.Empty;

        [JsonPropertyName("embedTemplateValues")]
        public Dictionary<string, string> EmbedTemplateValues { get; set; } = new();
    }

    public class AttachmentRequest
    {
        [JsonPropertyName("fileName")]
        public string FileName { get; set; } = string.Empty;

        [JsonPropertyName("base64Content")]
        public string? Base64Content { get; set; }

        [JsonPropertyName("filePath")]
        public string? FilePath { get; set; }

        [JsonPropertyName("contentType")]
        public string ContentType { get; set; } = "application/octet-stream";
    }

    public class EmailResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("messageId")]
        public string? MessageId { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("generatedDocumentPath")]
        public string? GeneratedDocumentPath { get; set; }

        [JsonPropertyName("errors")]
        public List<string>? Errors { get; set; }
    }

    public class EmailConfiguration
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public bool UseSsl { get; set; } = true;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
    }
}