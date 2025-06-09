using Repository.Entities;
using Repository.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interfaces
{
    public interface IAuthenRepository
    {
        Task<string> Login(LoginModel model);
        Task<string> Register(RegisterModel registerDTO);

        string GenerateJwtToken(User user);
    }
}
