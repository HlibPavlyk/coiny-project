namespace CoinyProject.Application.Features.Users.Models;

public class UserProfileModel
{
    public string Username { get; set; }
    public int DiscussionRate { get; set; }
    public int LikesCount { get; set; }
    public int AlbumsCount { get; set; }
    public int AlbumsElementsCount { get; set; }
}