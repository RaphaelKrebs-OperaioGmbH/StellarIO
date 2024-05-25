using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace StellarIO.Models
{
    public class User : IdentityUser
    {
        public List<Planet> Planets { get; set; } = new List<Planet>();
        public List<Science> Sciences { get; set; } = new List<Science>();
        public int Points { get; set; }
        public int? ActiveScienceId { get; set; }
        public Science ActiveScience { get; set; }
    }
}
