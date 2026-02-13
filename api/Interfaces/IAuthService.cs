using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTOs.Auth;

namespace api.Interfaces
{
    public interface IAuthService
    {
        Task<string?> LoginWithTelegramAsync(string initData);
    }
}