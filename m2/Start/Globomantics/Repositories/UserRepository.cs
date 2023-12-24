using Globomantics.Models;

namespace Globomantics.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly List<UserModel> _users = new List<UserModel>();

        public UserRepository(IHttpContextAccessor accessor)
        {
            var loggedInUser = accessor?.HttpContext?.User.Identity?.Name;
            _users.Add(new UserModel() { Name = "TekkeMansoorAhmad", Password = "XohImNooBHFR0OVvjcYpJ3NgPQ1qq73WKhHvch0VQtg=", FavoriteColor = "Merun Red" });
        }

        public UserRepository()
        {
            _users.Add(new UserModel() {GoogleId= "109878214007737964499", Name = "TekkeMansoorAhmad", Password = "XohImNooBHFR0OVvjcYpJ3NgPQ1qq73WKhHvch0VQtg=", FavoriteColor = "Merun Red" });
        }

        public UserModel? GetByUserAndPassword(string username, string password)
        {
            //var temp = password.sh256();
            return _users.FirstOrDefault(x => x.Name == username && x.Password == password.sh256());   
        }

        public UserModel? GetGoogleId(string subjectIdValue)
        {
            return _users.FirstOrDefault(x => x.GoogleId == subjectIdValue);
        }
    }
}
