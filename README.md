# InMemoryIdentity

There are cases where we need the Identity functionality but without the database!.
For example we cannot have windows authentication but we want to authenticate users from LDAP in our application.

The following are the steps to InMemoryIdentity...

1. Create your custom ApplicationUser, in Models\IdentityModels

	public class ApplicationUser : IUser
    {
        public string Id { get; set; }
        public string UserName { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            
            return userIdentity;
        }
    }

2. Replace the contents of IdentityConfig.cs in App_Start folder.

Create a class InMemoryUserStore<T>

	public class InMemoryUserStore<T> : IUserStore<T> where T : ApplicationUser {...}

and an InMemoryUserManager

	 public class InMemoryUserManager : UserManager<ApplicationUser>{...}

then add a method SignInMemoryUserAsync

	public async Task<SignInStatus> SignInMemoryUserAsync(ApplicationUser user, bool isPersistent,bool remenberMe)
        {

            await SignInAsync(user, isPersistent, remenberMe);
            return SignInStatus.Success;
        }

in your SignInManager.

3. Configure user manager and signin manager to use a single instance per request 
	
	app.CreatePerOwinContext<InMemoryUserManager>(InMemoryUserManager.Create);
    app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

in your Startup class.

4. Finally you can use your custom Identity in your Login Action
	
	 	[HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            IPrincipal iUser = HttpContext.User;
            if (!iUser.Identity.IsAuthenticated)
            {
            	//######### Add your user authentication here #####
                var user = await UserManager.FindByNameAsync(model.Email) ?? await UserManager.CreateAsync(model.Email);
                var result = await SignInManager.SignInMemoryUserAsync(user,false, model.RememberMe);

                switch (result)
                {
                    case SignInStatus.Success:
                        return RedirectToLocal(returnUrl);

                    case SignInStatus.Failure:
                    default:
                        ModelState.AddModelError("", "Invalid login attempt.");
                        return View(model);
                }
            }
            return View(model);
           
        }

Enjoy!