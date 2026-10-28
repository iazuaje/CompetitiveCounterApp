namespace CompetitiveCounterApp.Data
{
    public static class Constants
    {
        public const string DatabaseFilename = "AppSQLite.db3";

        public static string DatabaseFilePath => Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

        public static string DatabasePath => $"Data Source={DatabaseFilePath}";

        /// <summary>Imágenes de juego permanentes (las rutas absolutas se guardan en <c>Game.ImagePath</c>).</summary>
        public static string GameImagesDirectory => Path.Combine(FileSystem.AppDataDirectory, "GameImages");
    }
}
