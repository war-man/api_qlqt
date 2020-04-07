using Api.ManagerGift.Entities;
using System;

namespace Api.ManagerGift.DTO
{
    public class UserDTO
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public Position Position { get; set; }
        public Organization Organization { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public bool Status { get; set; }
        public int MonthId { get; set; }
        public string PositionId { get; set; }
        public string PositionCode { get; set; }
        public Guid OrganizationId { get; set; }
        public string OrganizationCode { get; set; }
        public int PermisionId { get; set; }
        public bool IsUser { get; set; }

    }
}
