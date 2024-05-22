using Microsoft.AspNetCore.Identity;
using System.Numerics;

namespace StellarIO.Models
{
    public class User : IdentityUser
    {
        public List<Planet> Planets { get; set; } = new List<Planet>();
        public List<Science> Sciences { get; set; } = new List<Science>();
        public int Points { get; set; }
    }
}