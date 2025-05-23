using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using eUseControl.BusinessLogic;
using eUseControl.Domain.Entities.User;
using eUseControl.Domain.Entities.Car;
using eUseControl.BusinessLogic.Interfaces;
using eUseControl.BusinessLogic.DBModel;


namespace Web.Controllers
{
    public class LogicController : Controller
    {
        private readonly ISession _session;
        private readonly IUserApi _userApi;
        public LogicController()
        {
            var bl = new eUseControl.BusinessLogic.BusinessLogic();
            _session = bl.GetSessionBL();
            var userContext = new UserContext();
            _userApi = new UserApi(userContext);
        }

        public ActionResult Index()
        {
            return View();
        }
    }
}