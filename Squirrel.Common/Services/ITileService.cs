using System.Threading.Tasks;
using Squirrel.Model;

namespace Squirrel.Services
{
    public interface ITileService
    {
        void ChangeTileBackground(TileColour colour);
        Task UpdateTile(int queuedCount, int favouriteCount, int archiveCount, TileColour tileColour);
        bool IncludeCounts { get; set; }
    }
}