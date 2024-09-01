using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CoinyProject.Application.Dto.Album
{
    public class AlbumElementEditDto
    {
        public int Id { get; set; }

        [MaxLength(20)]
        [Required(ErrorMessage = "The name field is required")]
        [RegularExpression("^[^!@#$%^&*()_+\\-=\\[\\]{};:'\"\\\\|,.<>\\/?]+$", ErrorMessage = "The field must not contain special characters")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [MaxLength(100)]
        [Display(Name = "Description")]
        public string? Description { get; set; }
        public int AlbumId { get; set; }

        [Display(Name = "Image")]
        public IFormFile? Image { get; set; }
        public string ImageURL { get; set; }
    }
}
