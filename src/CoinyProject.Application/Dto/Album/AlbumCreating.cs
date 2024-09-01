using System.ComponentModel.DataAnnotations;

namespace CoinyProject.Application.Dto.Album
{
    public record AlbumCreating(

        [MaxLength(30)]
        [Required(ErrorMessage = "The name field is required")]
        [RegularExpression("^[^!@#$%^&*()_+\\-=\\[\\]{};:'\"\\\\|,.<>\\/?]+$", ErrorMessage = "The field must not contain special characters")]
        [Display(Name = "Name")]
        string Name,

        [MaxLength(100)]
        [Display(Name = "Description")]
        string? Description);
}
