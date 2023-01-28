using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class PerkCategory : EntityObject
    {
        public PerkCategory()
        {
            Perks = new HashSet<Perk>();
        }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public ICollection<Perk> Perks { get; set; }
    }
}
