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
                if (!ValidateCard(cardDetails))
                    return false;

                if (amount <= 0)
                    return false;

                var payment = new Payment
                {
                    Amount = amount,
                    CardNumber = MaskCardNumber(cardDetails.CardNumber),
                    PaymentDate = DateTime.Now,
                    Status = "Completed"
                };

                _context.Payments.Add(payment);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ValidateCard(CardDetails cardDetails)
        {
            if (string.IsNullOrEmpty(cardDetails.CardNumber) ||
                string.IsNullOrEmpty(cardDetails.CVV) ||
                string.IsNullOrEmpty(cardDetails.ExpiryMonth) ||
                string.IsNullOrEmpty(cardDetails.ExpiryMonth) ||
                string.IsNullOrEmpty(cardDetails.CardHolderName))
                return false;

            // Validare număr card
            if (cardDetails.CardNumber.Length != 16 ||
                !cardDetails.CardNumber.All(char.IsDigit))
                return false;

            // Validare CVV
            if (cardDetails.CVV.Length != 3 ||
                !cardDetails.CVV.All(char.IsDigit))
                return false;

            // Validare data expirare
            if (!IsValidExpiryDate(cardDetails.ExpiryMonth))
                return false;

            if (!IsValidExpiryDate(cardDetails.ExpiryYear))
                return false;

            // Validare nume titular
            if (!IsValidCardHolderName(cardDetails.CardHolderName))
                return false;

            return true;
        }

        private bool IsValidExpiryDate(string expiryDate)
        {
            if (string.IsNullOrEmpty(expiryDate) || expiryDate.Length != 5)
                return false;

            if (!expiryDate.Contains("/"))
                return false;

            var parts = expiryDate.Split('/');
            if (parts.Length != 2)
                return false;

            if (!int.TryParse(parts[0], out int month) ||
                !int.TryParse(parts[1], out int year))
                return false;

            if (month < 1 || month > 12)
                return false;

            var currentYear = DateTime.Now.Year % 100;
            if (year < currentYear || year > currentYear + 10)
                return false;

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

            return new string('*', cardNumber.Length - 4) + cardNumber.Substring(cardNumber.Length - 4);
        }

        public Payment GetPaymentStatus(int paymentId)
        {
            var payment = _context.Payments.Find(paymentId);
            if (payment == null)
                return null;

            return new Payment
            {
                PaymentId = payment.PaymentId,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate
            };
        }
    }
}
