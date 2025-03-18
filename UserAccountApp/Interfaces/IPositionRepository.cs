using System.Collections.Generic;
using System.Threading.Tasks;
using UserAccountApp.Model;

namespace UserAccountApp.Interfaces
{
    public interface IPositionRepository
    {
        Task<List<Position>> GetAllPositionsAsync();
        Task<Position> GetByIdAsync(int id);
        Task<int> AddAsync(Position position);
        Task UpdateAsync(Position position);
        Task DeleteAsync(int id);
    }
} 