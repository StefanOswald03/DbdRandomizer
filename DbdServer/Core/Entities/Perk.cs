using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Perk : EntityObject
    {
        public Perk()
        {
            Categories = new HashSet<Category>();
        }

        [Required]
        public string Role { get; set; } = string.Empty;

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        public ICollection<Category> Categories { get; set; }

        [NotMapped]
        public int Page { get; set; }
    }
}
