namespace Api.ManagerGift.Entities
{
    public class PermisionDetail
    {
        public virtual int Id { get; set; }
        public virtual int ParentId { get; set; }
        public virtual int PermisionId { get; set; }
        public virtual string ActionName { get; set; }
        public virtual string ActionCode { get; set; }
        public virtual bool CheckAction { get; set; }
        public virtual string Url { get; set; }
        public virtual string NavName { get; set; } 
    }
}
