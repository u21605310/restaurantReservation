using System.ComponentModel.DataAnnotations;

namespace restaurantReservation.ViewModels
{
    public class EditPasswordViewModel
    {
        [Required(ErrorMessage = "Email Address is required.")]
        public string EmailAddress { get; set; }

        [Required(ErrorMessage = "Current password is required.")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [MinLength(6, ErrorMessage = "New password must be at least 6 characters long.")]
        public string NewPassword { get; set; }
    }
}
