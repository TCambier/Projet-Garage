using System.Linq;
using Garage.Data;

namespace Garage.Services
{

    public class AuthService
    {
        private readonly string _connectionString;
        public User CurrentUser { get; private set; }

        public AuthService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool Login(string username, string password)
        {
            using (var ctx = new GarageDbContext(_connectionString))
            {
                var hash = HashHelper.Sha256(password);

                var user = ctx.Users.FirstOrDefault(u =>
                    u.Identifiant == username &&
                    u.Mdp == hash
                );

                if (user != null)
                {
                    CurrentUser = user;
                    return true;
                }

                return false;
            }

        }

        public void Logout()
        {
            CurrentUser = null;
        }
    }
}
