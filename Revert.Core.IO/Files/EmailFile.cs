using System;
using System.Collections.Generic;
using Revert.Core.IO.Serialization;

namespace Revert.Testing.TestConsole
{
    public class EmailFile : SerializableBase<EmailFile>
    {
        public string Id { get; set; }
        public string Subject { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public string Text { get; set; }
        public DateTime Sent { get; set; }
        public DateTime Received { get; set; }
        public List<EmailAttachment> Attachments { get; set; } = new List<EmailAttachment>();
        public string FileName { get; set; }
    }

    public class EmailAttachment : SerializableBase<EmailAttachment>
    {
        public string AttachmentPath { get; set; }
        public string AttachmentText { get; set; }
    }
}