
namespace CoinyProject.Domain.Entities
{
    public class DiscussionMessage
    {
        public Guid Id { get; init; }
        public string Message { get; init; }
        public Guid DiscussionId { get; init; }
        public Guid UserId { get; init; }

        public User? User { get; init; }
        public Discussion Discussion { get; init; }
    }
}
