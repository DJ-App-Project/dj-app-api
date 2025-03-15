using dj_api.Models;

namespace dj_api.Repositories
{
    public interface IPlaylistRepository
    {
        Task<List<Playlist>> GetAllPlaylist();
    }
}