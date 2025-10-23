//using Domain.Entities;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Application.IRepository
//{
//    public interface ITokenRepository
//    {
//        public Task<UserToken?> GetUserTokenAsync(string UserEmail, string source);
//        public Task<UserToken?> GetUserTokenAsync(string Token);
//        public Task<bool> CheckValidTokenAsync(string Token);
//        public Task<UserToken> createUserTokenAsync(UserToken userToken);
//        public Task<bool> DeleteUserToken(string Token);
//        public Task<bool> DeleteUserToken(UserToken Token);
//        public Task<bool> DeleteUserTokens(ICollection<UserToken> userTokens);


//    }
//}
