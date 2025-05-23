using System;
using System.Web.Mvc;
using System.Web.Routing;
using eUseControl.BusinessLogic;
using eUseControl.BusinessLogic.Interfaces;
using Web.Controllers;

namespace Web
{
    public class CustomControllerFactory : DefaultControllerFactory
    {
        private readonly eUseControl.BusinessLogic.BusinessLogic _businessLogic = new eUseControl.BusinessLogic.BusinessLogic();

        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            if (controllerType == typeof(HomeController))
            {
                return new HomeController(
                    _businessLogic.UserApiBL,
                    _businessLogic.CarApiBL,
                    _businessLogic.BookingApiBL,
                    _businessLogic.PaymentApiBL,
                    _businessLogic.AdminApiBL
                );
            }
            else if (controllerType == typeof(LogicController))
            {
                return new LogicController(
                    _businessLogic.SessionBL,
                    _businessLogic.UserApiBL
                );
            }

            return base.GetControllerInstance(requestContext, controllerType);
        }
    }
}