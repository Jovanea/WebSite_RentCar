using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eUseControl.BusinessLogic;
using eUseControl.BusinessLogic.Interfaces;
using eUseControl.BusinessLogic.Core;
using eUseControl.BusinessLogic.DBModel;
using Web.BusinessLogic;
using Web.Interfaces;

namespace eUseControl.BusinessLogic
{
    public class BusinessLogic
    {
        public ISession SessionBL { get; }
        public IUserApi UserApiBL { get; }
        public ICarApi CarApiBL { get; }
        public IBookingApi BookingApiBL { get; }
        public IPaymentApi PaymentApiBL { get; }
        public IAdminApi AdminApiBL { get; }

        public BusinessLogic()
        {
            var userContext = new UserContext();
            SessionBL = new SessionBL(new UserApi(userContext), userContext);
            UserApiBL = new UserApi(userContext);
            CarApiBL = new CarApi(); 
            BookingApiBL = new BookingApi();
            PaymentApiBL = new PaymentApi();
            AdminApiBL = new AdminApi();
        }
    }
}


