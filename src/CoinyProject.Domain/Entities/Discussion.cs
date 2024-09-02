using CoinyProject.Domain.Enums;

namespace CoinyProject.Domain.Entities
{
    public class Discussion
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid UserId { get; set; }
        public DiscussionTopic Topic { get; set; }
        public DiscussionStatus Status { get; set; }
        public DiscussionTopic DiscussionTopic { get; set; }
        
        public User? User { get; set; }
        public ICollection<DiscussionMessage>? Messages { get; set; }
    }
}
