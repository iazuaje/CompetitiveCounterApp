using System.Diagnostics.CodeAnalysis;
using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.Utilities
{
    /// <summary>
    /// Game Model Extensions
    /// </summary>
    public static class GameExtensions
    {
        /// <summary>
        /// Check if the game is null or new.
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public static bool IsNullOrNew([NotNullWhen(false)] this Game? game)
        {
            return game is null || game.ID == 0;
        }
    }
}