using System.Web.Mvc;
using Unity;
using Unity.Mvc5;
using Unity.Lifetime; 
using eUseControl.BusinessLogic.Interfaces; 
using eUseControl.BusinessLogic.DBModel; 
using eUseControl.BusinessLogic.Core; 
using eUseControl.BusinessLogic;
using Web.Interfaces;
using Web.BusinessLogic;

namespace Web.App_Start
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            var container = new UnityContainer();

            container.RegisterType<UserContext>(new Unity.Lifetime.PerThreadLifetimeManager());

            container.RegisterType<IUserApi, UserApi>();
            container.RegisterType<ICarApi, CarApi>();
            container.RegisterType<IBookingApi, BookingApi>();
            container.RegisterType<IPaymentApi, PaymentApi>();
            container.RegisterType<IAdminApi, AdminApi>();
            container.RegisterType<ISession, SessionBL>();

            // enable the controller factory
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}