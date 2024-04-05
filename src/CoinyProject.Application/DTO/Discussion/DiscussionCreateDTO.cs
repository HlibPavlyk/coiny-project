using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Application.DTO.Discussion
{
    public class DiscussionCreateDTO
    {
        [MaxLength(80)]
        [Required(ErrorMessage = "The name field is required")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "The topic field is required")]
        [Display(Name = "Topic")]
        public int DiscussionTopicId { get; set; } 

        public IEnumerable<DiscussionTopicDTO> AvailableTopics { get; set; }
    }
}
