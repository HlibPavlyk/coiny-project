using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Application.DTO.Discussion
{
    public record DiscussionCreateDTO(
        [MaxLength(80)]
        [Required(ErrorMessage = "The name field is required")]
        [Display(Name = "Name")]
        string Name,

        [Required(ErrorMessage = "The topic field is required")]
        [Display(Name = "Topic")]
        int DiscussionTopicId
    ){ }
}
