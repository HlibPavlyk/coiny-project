using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Application.DTO
{
    public class AlbumEditDTO
    {
        public int Id { get; set; }

        [Required, MaxLength(20)]
        [RegularExpression("^[^!@#$%^&*()_+\\-=\\[\\]{};:'\"\\\\|,.<>\\/?]+$", ErrorMessage = "The field must not contain special characters")]
        public string Name { get; set; }
        
        [MaxLength(100)]
        public string? Description { get; set; }
    }

}
