using CoinyProject.Domain.Enums;

namespace CoinyProject.Domain.Entities
{
    public class Discussion
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public Guid UserId { get; init; }
        public DiscussionTopic Topic { get; init; }
        public DiscussionStatus Status { get; init; }
        
        public User User { get; set; }
        public ICollection<DiscussionMessage> Messages { get; set; }
    }
}
