﻿using System;
using System.Collections.Generic;
using MitigatingCircumstances.Models.Enum;
using Newtonsoft.Json;

namespace MitigatingCircumstances.Models
{
    public class ExtensionRequest
    {
        public int Id { get; set; }

        [JsonIgnore]
        public virtual ApplicationUser StudentCreatedBy { get; set; }

        [JsonIgnore]
        public virtual ApplicationUser TutorAssignedTo { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        [JsonIgnore]
        public virtual ICollection<UploadedDocument> UploadedDocuments { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow.ToLocalTime();

        public virtual ExtensionRequestStatus? Status { get; set; }
    }
}
