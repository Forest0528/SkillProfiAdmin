using System;

namespace SkillProfiAdmin.Models
{
    public enum RequestStatus
    {
        Received,
        InProgress,
        Completed,
        Rejected,
        Cancelled
    }

    public class Request
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public RequestStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
