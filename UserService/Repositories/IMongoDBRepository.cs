using System;
using System.Collections.Generic;
using Models;
using System.Threading.Tasks;

namespace Repositories
{
    public interface IMongoDBRepository
    {
        Task<bool> CheckIfUserExists(string Username);
        Task AddUserAsync(UserModel login);
        Task<UserModel> FindUserAsync(Guid id);
        Task UpdateUserAsync(UserModel login);
        Task DeleteUserAsync(Guid id);
    }
}