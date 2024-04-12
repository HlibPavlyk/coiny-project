using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Application.DTO.Album
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
