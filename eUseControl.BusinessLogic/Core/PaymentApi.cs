using System;
using System.Linq;
using Web.Interfaces;
using eUseControl.BusinessLogic.DBModel;

namespace Web.BusinessLogic
{
    public class PaymentApi : IPaymentApi
    {
        private readonly ApplicationDbContext _context;

        public PaymentApi()
        {
            _context = new ApplicationDbContext();
        }

        public bool ProcessPayment(CardDetails cardDetails, int amount)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Processing payment for amount: {amount}");
                System.Diagnostics.Debug.WriteLine($"Card holder: {cardDetails.CardHolderName}");
                System.Diagnostics.Debug.WriteLine($"Card number: {cardDetails.CardNumber?.Length} digits");
                System.Diagnostics.Debug.WriteLine($"Expiry: {cardDetails.ExpiryMonth}/{cardDetails.ExpiryYear}");
                
                Booking booking = null;
                try 
                {
                    if (cardDetails.BookingId > 0)
                    {
                        var connection = _context.Database.Connection;
                        
                        // Ensure connection is open
                        if (connection.State != System.Data.ConnectionState.Open)
                        {
                            connection.Open();
                        }
                        
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT BookingId, CarId, UserId, PickupDate, ReturnDate, TotalAmount, Status FROM Bookings WHERE BookingId = @bookingId";
                            
                            var parameter = command.CreateParameter();
                            parameter.ParameterName = "@bookingId";
                            parameter.Value = cardDetails.BookingId;
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
                    }
                    
                    if (booking == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Booking not found by ID, looking for pending bookings...");
                        
                        var connection = _context.Database.Connection;
                        if (connection.State != System.Data.ConnectionState.Open)
                        {
                            connection.Open();
                        }
                        
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT BookingId, CarId, UserId, PickupDate, ReturnDate, TotalAmount, Status FROM Bookings WHERE Status = 'Pending' AND TotalAmount = @amount";
                            
                            var parameter = command.CreateParameter();
                            parameter.ParameterName = "@amount";
                            parameter.Value = amount;
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
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error finding booking: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                }

                if (booking == null)
                {
                    System.Diagnostics.Debug.WriteLine("No pending booking found for this payment. Creating standalone payment.");
                }

                // Create and save the payment
                var payment = new Payment
                {
                    Amount = amount,
                    CardNumber = MaskCardNumber(cardDetails.CardNumber),
                    PaymentDate = DateTime.Now,
                    Status = "Completed",
                    TransactionId = Guid.NewGuid().ToString(),
                    BookingId = booking?.BookingId ?? 0
                };

                try
                {
                    var connection = _context.Database.Connection;
                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        connection.Open();
                    }
                    
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            INSERT INTO Payments (Amount, CardNumber, PaymentDate, Status, TransactionId, BookingId)
                            VALUES (@amount, @cardNumber, @paymentDate, @status, @transactionId, @bookingId);
                        ";
                        
                        var paramAmount = command.CreateParameter();
                        paramAmount.ParameterName = "@amount";
                        paramAmount.Value = payment.Amount;
                        command.Parameters.Add(paramAmount);
                        
                        var paramCardNumber = command.CreateParameter();
                        paramCardNumber.ParameterName = "@cardNumber";
                        paramCardNumber.Value = payment.CardNumber;
                        command.Parameters.Add(paramCardNumber);
                        
                        var paramPaymentDate = command.CreateParameter();
                        paramPaymentDate.ParameterName = "@paymentDate";
                        paramPaymentDate.Value = payment.PaymentDate;
                        command.Parameters.Add(paramPaymentDate);
                        
                        var paramStatus = command.CreateParameter();
                        paramStatus.ParameterName = "@status";
                        paramStatus.Value = payment.Status;
                        command.Parameters.Add(paramStatus);
                        
                        var paramTransactionId = command.CreateParameter();
                        paramTransactionId.ParameterName = "@transactionId";
                        paramTransactionId.Value = payment.TransactionId;
                        command.Parameters.Add(paramTransactionId);
                        
                        var paramBookingId = command.CreateParameter();
                        paramBookingId.ParameterName = "@bookingId";
                        paramBookingId.Value = payment.BookingId;
                        command.Parameters.Add(paramBookingId);
                        
                        int rowsAffected = command.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"Payment inserted: {rowsAffected} rows affected");
                    }
                    
                    // Update the booking status if found
                    if (booking != null)
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "UPDATE Bookings SET Status = 'Paid' WHERE BookingId = @bookingId";
                            
                            var paramBookingId = command.CreateParameter();
                            paramBookingId.ParameterName = "@bookingId";
                            paramBookingId.Value = booking.BookingId;
                            command.Parameters.Add(paramBookingId);
                            
                            int rowsAffected = command.ExecuteNonQuery();
                            System.Diagnostics.Debug.WriteLine($"Booking updated: {rowsAffected} rows affected");
                        }
                    }
                    
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        connection.Close();
                    }
                    
                    System.Diagnostics.Debug.WriteLine("Payment processed successfully");
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error saving payment: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Payment processing error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public bool ValidateCard(CardDetails cardDetails)
        {
            if (cardDetails == null)
            {
                System.Diagnostics.Debug.WriteLine("Card details object is null");
                return false;
            }
            
            if (string.IsNullOrEmpty(cardDetails.CardNumber) ||
                string.IsNullOrEmpty(cardDetails.CVV) ||
                string.IsNullOrEmpty(cardDetails.ExpiryMonth) ||
                string.IsNullOrEmpty(cardDetails.ExpiryYear) ||
                string.IsNullOrEmpty(cardDetails.CardHolderName))
            {
                System.Diagnostics.Debug.WriteLine("One or more card fields are empty");
                return false;
            }

            var cleanCardNumber = cardDetails.CardNumber.Replace(" ", "");
            if (cleanCardNumber.Length != 16 ||
                !cleanCardNumber.All(char.IsDigit))
            {
                System.Diagnostics.Debug.WriteLine("Invalid card number format");
                return false;
            }

            // Validare CVV
            if (cardDetails.CVV.Length != 3 ||
                !cardDetails.CVV.All(char.IsDigit))
            {
                System.Diagnostics.Debug.WriteLine("Invalid CVV format");
                return false;
            }

            // Validare data expirare
            int month;
            if (!int.TryParse(cardDetails.ExpiryMonth, out month) || month < 1 || month > 12)
            {
                System.Diagnostics.Debug.WriteLine("Invalid expiry month");
                return false;
            }

            int year;
            if (!int.TryParse(cardDetails.ExpiryYear, out year))
            {
                System.Diagnostics.Debug.WriteLine("Invalid expiry year");
                return false;
            }
            
            // Convert 2-digit year to 4-digit
            int fullYear = 2000 + year;
            
            // Check if card is expired
            var currentDate = DateTime.Now;
            var expiryDate = new DateTime(fullYear, month, DateTime.DaysInMonth(fullYear, month));
            
            if (expiryDate < currentDate)
            {
                System.Diagnostics.Debug.WriteLine($"Card expired: {month}/{year} (current: {currentDate.Month}/{currentDate.Year})");
                return false;
            }

            // Validare nume titular
            if (!IsValidCardHolderName(cardDetails.CardHolderName))
            {
                System.Diagnostics.Debug.WriteLine("Invalid card holder name");
                return false;
            }

            return true;
        }

        private bool IsValidCardHolderName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            // Verifică dacă numele conține doar litere și spații
            if (!name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
                return false;

            // Verifică dacă numele are cel puțin două cuvinte
            var words = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length < 2)
                return false;

            return true;
        }

        private string MaskCardNumber(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 4)
                return cardNumber;

            var cleanCard = cardNumber.Replace(" ", "");
            return new string('*', cleanCard.Length - 4) + cleanCard.Substring(cleanCard.Length - 4);
        }

        public Payment GetPaymentStatus(int paymentId)
        {
            try
            {
                var payment = _context.Payments.Find(paymentId);
                if (payment == null)
                    return null;

                return new Payment
                {
                    PaymentId = payment.PaymentId,
                    Amount = payment.Amount,
                    PaymentDate = payment.PaymentDate,
                    Status = payment.Status,
                    TransactionId = payment.TransactionId,
                    BookingId = payment.BookingId
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting payment status: {ex.Message}");
                return null;
            }
        }
    }
}
