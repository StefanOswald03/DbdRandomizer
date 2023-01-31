using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    [Index(nameof(PerkCategory.Name), IsUnique = true)]
    [Index(nameof(PerkCategory.MyName), IsUnique = true)]
    public class PerkCategory : EntityObject
    {
        public PerkCategory()
        {
            Perks = new HashSet<Perk>();
        }

        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Role { get; set; } = string.Empty;
        public string? MyName { get; set; }

        [Required]
        public ICollection<Perk> Perks { get; set; }
    }
}
