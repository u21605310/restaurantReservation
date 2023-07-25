using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using restaurantReservation.Models;
using restaurantReservation.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace restaurantReservation.Controllers
{
    [Route("api/reservations")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly IRepository _reservationRepository;

        public ReservationController(IRepository reservationRepository)
        {
            _reservationRepository = reservationRepository;
        }

        [HttpGet]
        [Route("GetAllReservations")]
        public async Task<IActionResult> GetAllReservations()
        {
            try
            {
                // Get all existing reservations
                var reservations = await _reservationRepository.GetAllReservationsAsync();

                // Create a list of ReservationViewModel containing existing reservations
                var results = new List<ReservationViewModel>();
                foreach (var reservation in reservations)
                {
                    results.Add(new ReservationViewModel
                    {
                        CustomerName = reservation.CustomerName,
                        ReservationDateTime = reservation.ReservationDateTime,
                        NumberOfGuests = reservation.NumberOfGuests,
                        TableNumber = reservation.TableNumber
                    });
                }

                return Ok(results);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal Server Error. Please contact support.");
            }
        }



        [HttpGet]
        [Route("GetReservationByCustomerName/{customerName}")]
        public async Task<IActionResult> GetReservationByCustomerNameAsync(string customerName)
        {
            try
            {
                var result = await _reservationRepository.GetReservationByCustomerNameAsync(customerName);

                if (result == null) return NotFound("Reservation does not exist. You need to create it first");

                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal Server Error. Please contact support");
            }
        }


        [HttpPost]
        [Route("AddReservation")]
        public async Task<IActionResult> AddReservation(ReservationViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                // Model validation failed, return Bad Request with validation errors
                return BadRequest(ModelState);
            }

            try
            {
                // The model is valid, proceed with creating the reservation
                var reservation = new Reservation
                {
                    CustomerName = vm.CustomerName,
                    ReservationDateTime = vm.ReservationDateTime,
                    NumberOfGuests = vm.NumberOfGuests,
                    TableNumber = vm.TableNumber,
                };

                // Check if the selected table is available at the specified reservation date and time
                var availableTables = await GetAvailableTablesAsync(reservation.ReservationDateTime);
                if (!availableTables.Contains(reservation.TableNumber))
                {
                    return BadRequest("The selected table is not available at the specified date and time.");
                }

                // Check if the user has already made a reservation using the customer name as the email
                var existingReservation = await _reservationRepository.GetReservationByCustomerNameAsync(vm.CustomerName);
                if (existingReservation != null)
                {
                    return BadRequest("You have already made a reservation. You can only make one reservation.");
                }

                _reservationRepository.AddReservation(reservation);
                await _reservationRepository.SaveChangesAsync();

                return Ok(reservation);
            }
            catch (DbUpdateException ex)
            {
                // Handle database-related exceptions
                return BadRequest("Error updating the database. Please try again later.");
            }
            catch (ValidationException ex)
            {
                // Handle validation-related exceptions
                return BadRequest(ex.Message); // Return the specific validation error message
            }
            catch (Exception ex)
            {
                // Handle other types of exceptions
                return BadRequest("An error occurred. Please try again later.");
            }
        }




        private async Task<List<int>> GetAvailableTablesAsync(DateTime reservationDateTime)
        {
            var reservations = await _reservationRepository.GetAllReservationsAsync();
            var bookedTables = reservations.Where(r => r.ReservationDateTime == reservationDateTime).Select(r => r.TableNumber).ToList();
            var allTables = Enumerable.Range(1, 15).ToList();
            return allTables.Except(bookedTables).ToList();
        }




        [HttpPut]
        [Route("EditReservationByCustomerName/{customerName}")]
        public async Task<ActionResult<ReservationViewModel>> EditReservationByCustomerName(string customerName, ReservationViewModel vm)
        {
            try
            {
                var existingReservation = await _reservationRepository.GetReservationByCustomerNameAsync(customerName);
                if (existingReservation == null)
                {
                    return NotFound($"The reservation for customer '{customerName}' does not exist");
                }

                // Update the existing reservation with the data from the view model
                existingReservation.CustomerName = vm.CustomerName;
                existingReservation.ReservationDateTime = vm.ReservationDateTime;
                existingReservation.TableNumber = vm.TableNumber;
                existingReservation.NumberOfGuests = vm.NumberOfGuests;

                if (await _reservationRepository.SaveChangesAsync())
                {
                    return Ok(existingReservation);
                }
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal Server Error. Please contact support.");
            }

            return BadRequest("Your request is invalid.");
        }


        [HttpDelete]
        [Route("DeleteReservationByCustomerName/{customerName}")]
        public async Task<IActionResult> DeleteReservationByCustomerName(string customerName)
        {
            try
            {
                var existingReservation = await _reservationRepository.GetReservationByCustomerNameAsync(customerName);

                if (existingReservation == null)
                {
                    return NotFound($"The reservation does not exist for customer '{customerName}'");
                }

                // Store the table number to be used later
                int deletedTableNumber = existingReservation.TableNumber;

                _reservationRepository.DeleteReservation(existingReservation);

                if (await _reservationRepository.SaveChangesAsync())
                {
                    // Get the reservationDateTime from the deleted reservation
                    DateTime reservationDateTime = existingReservation.ReservationDateTime;

                    // Get the list of available tables at the same reservationDateTime
                    var availableTables = await GetAvailableTablesAsync(reservationDateTime);

                    // Check if the deleted table number is not already in the availableTables list
                    if (!availableTables.Contains(deletedTableNumber))
                    {
                        // Add the deleted table number back to the availableTables list
                        availableTables.Add(deletedTableNumber);
                    }

                    // Return the availableTables list to the frontend
                    return Ok(availableTables);
                }
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal Server Error. Please contact support.");
            }

            return BadRequest("Your request is invalid.");
        }
    }

}
