using Microsoft.AspNetCore.Identity;

namespace restaurantReservation.Models
{
    public interface IRepository
    {
        //User
        Task<User[]> GetAllUsersAsync();
        Task<User> GetUserByEmailAsync(string email);
        Task<bool> DeleteUserAsync(string email, UserManager<User> userManager);

        //Reservation
        void AddReservation<T>(T entity) where T : class;
        void DeleteReservation<T>(T entity) where T : class;
        Task<Reservation[]> GetAllReservationsAsync();
        Task<Reservation> GetReservationAsync(int reservationId);
        Task<Reservation> GetReservationByCustomerNameAsync(string customerName);

        //Saving changes onto DbContext
        Task<bool> SaveChangesAsync();
    }
}
