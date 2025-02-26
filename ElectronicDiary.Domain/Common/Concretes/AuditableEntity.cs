﻿using ElectronicDiary.Domain.Common.Abstracts;

namespace ElectronicDiary.Domain.Common.Concretes
{
    public abstract class AuditableEntity : IAuditableEntity
    {
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
