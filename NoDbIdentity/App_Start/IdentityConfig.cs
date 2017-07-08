using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using NoDbIdentity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;


namespace NoDbIdentity
{
    
    public class InMemoryUserStore<T> : IUserStore<T> where T : ApplicationUser
    {
        private readonly static IList<T> _users = new List<T>();

        public IQueryable<T> Users
        {
            get
            {
                return _users.AsQueryable();
            }
        }
        private T FindUser(T user)
        {
            return _users.SingleOrDefault(x => x.Id == user.Id)?? _users.SingleOrDefault(x=>x.UserName == user.UserName);

        }
        private T FindUser(string userId)
        {
            return _users.SingleOrDefault(x => x.Id == userId);

        }
       
        public Task DeleteAsync(T user)
        {
            lock (_users)
            {
                var existingUser = FindUser(user);
                if (existingUser == null)
                {
                    return Task.FromResult(IdentityResult.Failed("User not found"));
                }
                _users.Remove(existingUser);
            }
            return Task.FromResult(IdentityResult.Success);
        }

        public Task<T> FindByIdAsync(string userId)
        {
            return Task.FromResult(FindUser(userId));
        }

        public Task<T> FindByNameAsync(string userName)
        {
            return Task.FromResult(_users.SingleOrDefault(x => x.UserName == userName));
        }

        public Task UpdateAsync(T user)
        {
            throw new NotImplementedException();
        }


        void IDisposable.Dispose()
        {
            // throw new NotImplementedException();
            
        }

        Task IUserStore<T, string>.CreateAsync(T user)
        {
            lock (_users)
            {
                var existingUser = FindUser(user);
                if (existingUser == null)
                {
                    _users.Add(user);
                    existingUser = user;

                }
                return Task.FromResult(existingUser);
            }
        }
    }
    public class InMemoryUserManager : UserManager<ApplicationUser>
    {
        public InMemoryUserManager(InMemoryUserStore<ApplicationUser> store)
            : base(store)
        {

        }
        public static InMemoryUserManager Create(IdentityFactoryOptions<InMemoryUserManager> options, IOwinContext context)
        {

            return new InMemoryUserManager(new InMemoryUserStore<ApplicationUser>());
        }
        //
        // Summary:
        //     Returns the user associated with this login
        //public virtual Task<TUser> FindAsync(UserLoginInfo login);
        public Task<ApplicationUser> FindAsync(ExternalLoginInfo ext)
        {
            return Store.FindByIdAsync(ext.Login.ProviderKey);

        }
        public Task<ApplicationUser> FindAByNameAsync(string username)
        {
            return Store.FindByNameAsync(username);

        }
        public Task<ApplicationUser> CreateAsync(ExternalLoginInfo ext)
        {
            var newUser = new ApplicationUser { Id = ext.Login.ProviderKey, UserName = ext.DefaultUserName };

            return (Task<ApplicationUser>)Store.CreateAsync(newUser);
        }

        public Task<ApplicationUser> CreateAsync(string userName)
        {
            var newUser = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = userName };

            return (Task<ApplicationUser>)Store.CreateAsync(newUser);
        }
       

    }
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(InMemoryUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((InMemoryUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<InMemoryUserManager>(), context.Authentication);
        }

        public async Task<SignInStatus> SignInMemoryUserAsync(ApplicationUser user, bool isPersistent,bool remenberMe)
        {

            await SignInAsync(user, isPersistent, remenberMe);
            return SignInStatus.Success;
        }
    }

}