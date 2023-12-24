using Globomantics.Models;

namespace Globomantics.Repositories
{
    public interface IUserRepository
    {
        public UserModel? GetByUserAndPassword(string username, string password);
    }
}
