using Repository.Interfaces;
using Repository.Model;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public class AuthenService : IAuthenService
    {
        private readonly IAuthenRepository _authenRepository;
        public AuthenService(IAuthenRepository authenRepository)
        {
            _authenRepository = authenRepository ?? throw new ArgumentNullException(nameof(authenRepository));
        }
        public async Task<string> Login(LoginModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "Login model cannot be null.");
            }
            return await _authenRepository.Login(model);
        }
        public async Task<string> Register(RegisterModel registerDTO)
        {
            if (registerDTO == null)
            {
                throw new ArgumentNullException(nameof(registerDTO), "Register model cannot be null.");
            }
            return await _authenRepository.Register(registerDTO);
        }
    }
}
