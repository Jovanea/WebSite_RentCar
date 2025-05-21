using System;
using System.Collections.Generic;
using System.Linq;
using Web.Interfaces;
using eUseControl.BusinessLogic.DBModel;
using System.Data.Entity;

namespace Web.BusinessLogic
{
    public class BookingApi : IBookingApi
    {
        private readonly ApplicationDbContext _context;

        public BookingApi()
        {
            _context = new ApplicationDbContext();
        }

        public bool CreateBooking(Booking booking)
        {
            try
            {
                // First check if there's enough stock
                var carApi = new Web.BusinessLogic.CarApi();
                int currentStock = carApi.GetCarStock(booking.CarId);
                
                if (currentStock <= 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Cannot create booking: Car {booking.CarId} is out of stock");
                    return false;
                }
                
                // Create the booking
                _context.Bookings.Add(booking);
                _context.SaveChanges();
                
                // Now decrease the car stock
                bool stockUpdated = carApi.UpdateCarStock(booking.CarId, -1);
                if (!stockUpdated)
                {
                    System.Diagnostics.Debug.WriteLine($"Warning: Booking {booking.BookingId} created but failed to update car stock");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating booking: {ex.Message}");
                return false;
            }
        }

        public bool UpdateBooking(Booking booking)
        {
            try
            {
                var existingBooking = _context.Bookings.Find(booking.BookingId);
                if (existingBooking == null) return false;

                _context.Entry(existingBooking).CurrentValues.SetValues(booking);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating booking: {ex.Message}");
                return false;
            }
        }

        public bool CancelBooking(int bookingId)
        {
            try
            {
                var booking = _context.Bookings.Find(bookingId);
                if (booking == null) return false;

                // Only increment stock if status wasn't already Cancelled
                bool shouldIncrementStock = booking.Status != "Cancelled";
                
                booking.Status = "Cancelled";
                _context.SaveChanges();
                
                // Increment car stock when booking is cancelled
                if (shouldIncrementStock)
                {
                    var carApi = new Web.BusinessLogic.CarApi();
                    bool stockUpdated = carApi.UpdateCarStock(booking.CarId, 1);
                    if (!stockUpdated)
                    {
                        System.Diagnostics.Debug.WriteLine($"Warning: Booking {bookingId} cancelled but failed to update car stock");
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cancelling booking: {ex.Message}");
                return false;
            }
        }

        public bool CancelBookingByUserAndStatus(int userId, string status = "Pending")
        {
            try
            {
                // First get all affected bookings to update stock later
                var bookingsToCancel = new List<Booking>();
                var connection = _context.Database.Connection;
                connection.Open();
                
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT BookingId, CarId FROM Bookings WHERE UserId = @userId AND Status = @status";
                    
                    var userIdParam = command.CreateParameter();
                    userIdParam.ParameterName = "@userId";
                    userIdParam.Value = userId;
                    command.Parameters.Add(userIdParam);
                    
                    var statusParam = command.CreateParameter();
                    statusParam.ParameterName = "@status";
                    statusParam.Value = status;
                    command.Parameters.Add(statusParam);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bookingsToCancel.Add(new Booking
                            {
                                BookingId = Convert.ToInt32(reader["BookingId"]),
                                CarId = Convert.ToInt32(reader["CarId"])
                            });
                        }
                    }
                }
                
                // Update the status to Cancelled
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE Bookings SET Status = 'Cancelled' WHERE UserId = @userId AND Status = @status";
                    
                    var userIdParam = command.CreateParameter();
                    userIdParam.ParameterName = "@userId";
                    userIdParam.Value = userId;
                    command.Parameters.Add(userIdParam);
                    
                    var statusParam = command.CreateParameter();
                    statusParam.ParameterName = "@status";
                    statusParam.Value = status;
                    command.Parameters.Add(statusParam);
                    
                    int rowsAffected = command.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine($"Cancelled {rowsAffected} bookings for user {userId}");
                }
                
                connection.Close();
                
                // Update car stock for each cancelled booking
                if (bookingsToCancel.Count > 0)
                {
                    var carApi = new Web.BusinessLogic.CarApi();
                    foreach (var booking in bookingsToCancel)
                    {
                        bool stockUpdated = carApi.UpdateCarStock(booking.CarId, 1);
                        if (!stockUpdated)
                        {
                            System.Diagnostics.Debug.WriteLine($"Warning: Failed to update stock for car {booking.CarId} after cancelling booking {booking.BookingId}");
                        }
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cancelling bookings: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public List<Booking> GetUserBookings(int userId)
        {
            try
            {
                // Get the data directly from the database without Entity Framework conversion
                var bookings = new List<Booking>();

                // Use a raw SQL query to avoid EF conversion issues
                var connection = _context.Database.Connection;
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT BookingId, CarId, UserId, PickupDate, ReturnDate, TotalAmount, Status FROM Bookings WHERE UserId = @userId";
                    
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@userId";
                    parameter.Value = userId;
                    command.Parameters.Add(parameter);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                // Manual conversion to handle any type mismatches
                                var booking = new Booking
                                {
                                    BookingId = Convert.ToInt32(reader["BookingId"]),
                                    CarId = Convert.ToInt32(reader["CarId"]),
                                    UserId = Convert.ToInt32(reader["UserId"]),
                                    PickupDate = Convert.ToDateTime(reader["PickupDate"]),
                                    ReturnDate = Convert.ToDateTime(reader["ReturnDate"]),
                                    Status = reader["Status"].ToString()
                                };

                                // Handle TotalAmount specifically to convert from decimal to int if needed
                                if (reader["TotalAmount"] != DBNull.Value)
                                {
                                    if (reader["TotalAmount"] is decimal)
                                    {
                                        booking.TotalAmount = Convert.ToInt32((decimal)reader["TotalAmount"]);
                                    }
                                    else
                                    {
                                        booking.TotalAmount = Convert.ToInt32(reader["TotalAmount"]);
                                    }
                                }
                                
                                bookings.Add(booking);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error converting booking: {ex.Message}");
                                // Continue to next booking instead of failing
                            }
                        }
                    }
                }
                connection.Close();
                
                return bookings;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting user bookings: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return new List<Booking>();
            }
        }

        public Booking GetBookingById(int bookingId)
        {
            try
            {
                // Use raw SQL instead of Entity Framework to avoid conversion issues
                var connection = _context.Database.Connection;
                connection.Open();
                Booking booking = null;
                
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT BookingId, CarId, UserId, PickupDate, ReturnDate, TotalAmount, Status FROM Bookings WHERE BookingId = @bookingId";
                    
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@bookingId";
                    parameter.Value = bookingId;
                    command.Parameters.Add(parameter);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            booking = new Booking
                            {
                                BookingId = Convert.ToInt32(reader["BookingId"]),
                                CarId = Convert.ToInt32(reader["CarId"]),
                                UserId = Convert.ToInt32(reader["UserId"]),
                                PickupDate = Convert.ToDateTime(reader["PickupDate"]),
                                ReturnDate = Convert.ToDateTime(reader["ReturnDate"]),
                                Status = reader["Status"].ToString()
                            };

                            // Handle TotalAmount specifically to convert from decimal to int if needed
                            if (reader["TotalAmount"] != DBNull.Value)
                            {
                                if (reader["TotalAmount"] is decimal)
                                {
                                    booking.TotalAmount = Convert.ToInt32((decimal)reader["TotalAmount"]);
                                }
                                else
                                {
                                    booking.TotalAmount = Convert.ToInt32(reader["TotalAmount"]);
                                }
                            }
                        }
                    }
                }
                connection.Close();
                
                return booking;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting booking by ID: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }
    }
}
