namespace ElectronicDiary.Domain.Common.Concretes
{
    public abstract class BaseEntity<Tkey> : AuditableEntity
       where Tkey : struct
    {
        public Tkey Id { get; set; }
    }
}
