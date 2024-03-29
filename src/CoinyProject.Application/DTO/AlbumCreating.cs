using CoinyProject.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Application.DTO
{
    public class AlbumCreating
    {
        [MaxLength(20)]
        [Required(ErrorMessage = "The name field is required")]
        [RegularExpression("^[^!@#$%^&*()_+\\-=\\[\\]{};:'\"\\\\|,.<>\\/?]+$", ErrorMessage = "The field must not contain special characters")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [MaxLength(100)]
        [Display(Name = "Description")]
        public string? Description { get; set; }
    }
}
