using System;
using eUseControl.BusinessLogic.DBModel;

namespace Web.Interfaces
{
    public interface IPaymentApi
    {
        bool ProcessPayment(CardDetails cardDetails, int amount);
        bool ValidateCard(CardDetails cardDetails);
        Payment GetPaymentStatus(int paymentId);
    }
}
