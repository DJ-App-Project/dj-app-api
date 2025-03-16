using dj_api.Models;

namespace dj_api.Repositories
{
    public interface ISongPlayRepository
    {
        Task RecordPlayAsync(string songId);
        Task<Song> GetMostPLayedSongAsync();
    }
}