using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace restaurantReservation.Models
{
    public class Repository : IRepository
    {
        private readonly AppDbContext _appDbContext;

        public Repository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _appDbContext.SaveChangesAsync() > 0;
        }

        //User

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _appDbContext.Users.FirstOrDefaultAsync(p => p.Email == email);
        }

        public async Task<User[]> GetAllUsersAsync()
        {
            IQueryable<User> query = _appDbContext.Users;
            return await query.ToArrayAsync();
        }



        public async Task<bool> DeleteUserAsync(string email, UserManager<User> userManager)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return false;
            }

            var result = await userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        //Reservations
        public void AddReservation<T>(T entity) where T : class
        {
            _appDbContext.Add(entity);
        }

        public void DeleteReservation<T>(T entity) where T : class
        {
            _appDbContext.Remove(entity);
        }

        public async Task<Reservation[]> GetAllReservationsAsync()
        {
            IQueryable<Reservation> query = _appDbContext.Reservations;
            return await query.ToArrayAsync();
        }

        public async Task<Reservation> GetReservationAsync(int reservationId)
        {
            IQueryable<Reservation> query = _appDbContext.Reservations.Where(c => c.reservationId == reservationId);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<Reservation> GetReservationByCustomerNameAsync(string customerName)
        {
            return await _appDbContext.Reservations
                .FirstOrDefaultAsync(r => r.CustomerName == customerName);
        }
    }
}
