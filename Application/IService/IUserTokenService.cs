using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IUserTokenService
    {
        public Task<string> createTokenAsync(Member member, IList<string> roles, string Mode,string source);
        public bool checkTokenValid(string token);

        //public  Task<string> getUserTokenAsync(string Email, string source);
        //public Task<UserToken> getTokenAsync(string Token);
        //public Task<bool> deleteTokenAsync(UserToken userToken);

    }
}
