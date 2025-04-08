using eUseControl.Domain.Entities.User;
using eUseControl.BusinessLogic.Models;

namespace eUseControl.BusinessLogic.Interfaces
{
    public interface IUserApi
    {
        UserRegister UserRegister(URegisterData data);
    }
}