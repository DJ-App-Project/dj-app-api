using dj_api.Models;

namespace dj_api.Repositories
{
    public interface ISongRepository
    {
        Task<Song> GetSongByIdAsync(string objectId);
        Task<List<Song>> GetAllSongsAsync();
        Task<List<Song>> GetPaginatedSongsAsync(int page, int pageSize);
        Task CreateSongAsync(Song newSong);
        Task UpdateSongAsync(string ObjectId, Song updatedSong);
        Task DeleteSongAsync(string ObjectId);
        Task<List<Song>> FindSongsByArtistAsync(string artist);
        Task<Song?> FindSongByTitleAsync(string title);
    }
} 