namespace StellarIO.Models
{
    public class Science
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public int Level { get; set; }
    }

}