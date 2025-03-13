using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eUseControl.Domain.Entities.User;

namespace eUseControl.BusinessLogic
{
    public interface ISession
    {
        UserLogin UserLogin(ULoginData data);
    }
}
