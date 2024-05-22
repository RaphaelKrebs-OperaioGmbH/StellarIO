namespace StellarIO.Models
{
    public class Ship
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AttackPoints { get; set; }
        public int DefensePoints { get; set; }
        public int Speed { get; set; }
        public int CargoCapacity { get; set; }
        public int IronCost { get; set; }
        public int SilverCost { get; set; }
        public int AluminiumCost { get; set; }
        public int H2Cost { get; set; }
        public int EnergyCost { get; set; }
        public int ScienceRequiredId { get; set; }
        public Science ScienceRequired { get; set; }
        public int Points { get; set; }
    }
}