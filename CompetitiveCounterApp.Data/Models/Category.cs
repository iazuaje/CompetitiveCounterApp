namespace CompetitiveCounterApp.Models
{
    /// <summary>Etiqueta para agrupar y filtrar juegos (un juego puede tener varias).</summary>
    public class Category
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;

        public List<Game> Games { get; set; } = [];

        public override string ToString() => Name;
    }
}
