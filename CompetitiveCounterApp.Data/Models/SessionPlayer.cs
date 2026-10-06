using CommunityToolkit.Mvvm.ComponentModel;

namespace CompetitiveCounterApp.Models
{
    public partial class SessionPlayer : ObservableObject
    {
        public int ID { get; set; }
        public int SessionID { get; set; }
        public int PlayerID { get; set; }

        [ObservableProperty]
        private int _wins;

        public Session? Session { get; set; }
        public Player? Player { get; set; }

        public override string ToString() => $"{Player?.Name ?? "Unknown"} - {Wins} wins";
    }
}
