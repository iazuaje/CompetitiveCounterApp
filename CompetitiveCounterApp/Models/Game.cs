namespace CompetitiveCounterApp.Models
{
    public class Game
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<Player> Players { get; set; } = [];
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public override string ToString() => Name;
    }
}