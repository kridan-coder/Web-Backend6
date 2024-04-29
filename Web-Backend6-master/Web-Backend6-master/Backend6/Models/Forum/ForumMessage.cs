using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Backend6.Models
{
    public class ForumMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ForumTopicId { get; set; }

        public ForumTopic ForumTopic { get; set; }

        [Required]
        public String CreatorId { get; set; }

        public ApplicationUser Creator { get; set; }

        [Required]
        public String Text { get; set; }


        public DateTime Created { get; set; }

        public DateTime? Modified { get; set; }

        public ICollection<ForumMessageAttachment> ForumMessageAttachments { get; set; }
    }
}
