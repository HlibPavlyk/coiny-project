using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CoinyProject.Application.Dto.Album
{
    public record AlbumElementCreating(
        [MaxLength(20)]
        [Required(ErrorMessage = "The name field is required")]
        [RegularExpression("^[^!@#$%^&*()_+\\-=\\[\\]{};:'\"\\\\|,.<>\\/?]+$", ErrorMessage = "The field must not contain special characters")]
        [Display(Name = "Name")]
        string Name,

        [MaxLength(100)]
        [Display(Name = "Description")]
        string? Description,
        int AlbumId,

        [Required(ErrorMessage = "The image field is required")]
        [Display(Name = "Image")]
        IFormFile Image)
    { }
}
