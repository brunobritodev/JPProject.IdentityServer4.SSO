using IdentityModel;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Jp.Ldap;
using Jp.UI.SSO.Models;
using Jp.UI.SSO.Util;
using JPProject.Domain.Core.Bus;
using JPProject.Domain.Core.Interfaces;
using JPProject.Domain.Core.Notifications;
using JPProject.Domain.Core.Util;
using JPProject.Domain.Core.ViewModels;
using JPProject.Sso.Application.Interfaces;
using JPProject.Sso.Application.ViewModels;
using JPProject.Sso.Application.ViewModels.UserViewModels;
using JPProject.Sso.AspNetIdentity.Models.Identity;
using JPProject.Sso.Domain.ViewModels.Settings;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Jp.UI.SSO.Controllers.Account
{
    public class AccountController : Controller
    {
        private readonly IMediatorHandler Bus;
        private readonly SignInManager<UserIdentity> _signInManager;
        private readonly UserManager<UserIdentity> _userManager;
        private readonly IUserAppService _userAppService;
        private readonly IGlobalConfigurationAppService _globalConfigurationAppService;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;
        private readonly IConfiguration _configuration;
        private readonly IUserManageAppService _userManageAppService;
        private readonly ISystemUser _user;
        private readonly ILogger<AccountController> _logger;
        private readonly DomainNotificationHandler _notifications;

        public AccountController(
            SignInManager<UserIdentity> signInManager,
            UserManager<UserIdentity> userManager,
            IUserAppService userAppService,
            IGlobalConfigurationAppService globalConfigurationAppService,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events,
            INotificationHandler<DomainNotification> notifications,
            IMediatorHandler bus,
            IConfiguration configuration,
            IUserManageAppService userManageAppService,
            ISystemUser user,
            ILogger<AccountController> logger)
        {
            Bus = bus;
            _signInManager = signInManager;
            _userManager = userManager;
            _userAppService = userAppService;
            _globalConfigurationAppService = globalConfigurationAppService;
            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
            _configuration = configuration;
            _userManageAppService = userManageAppService;
            _user = user;
            _logger = logger;
            _notifications = (DomainNotificationHandler)notifications;
        }

        /// <summary>
        /// Show login page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            // build a model so we know what to show on the login page
            var vm = await BuildLoginViewModelAsync(returnUrl);

            if (vm.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return await ExternalLogin(vm.ExternalLoginScheme, returnUrl);
            }

            return View(vm);
        }

        [HttpGet]
        public IActionResult Register()
        {
            var url = $"{_configuration.GetValue<string>("ApplicationSettings:UserManagementURL")}/register";
            return Redirect(url);
        }

        public IActionResult ForgotPassword()
        {
            var url = $"{_configuration.GetValue<string>("ApplicationSettings:UserManagementURL")}/recover";
            return Redirect(url);
        }
        /// <summary>
        /// Handle postback from username/password login
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model, string button)
        {
            // the user clicked the "cancel" button
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
            if (button != "login")
            {
                if (context != null)
                {
                    // if the user cancels, send a result back into IdentityServer as if they 
                    // denied the consent (even if this client does not require consent).
                    // this will send back an access denied OIDC error response to the client.
                    await _interaction.GrantConsentAsync(context, ConsentResponse.Denied);

                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    return Redirect(model.ReturnUrl);
                }
                else
                {
                    // since we don't have a valid context, then we just go back to the home page
                    return Redirect("~/");
                }
            }

            if (!ModelState.IsValid)
            {
                // something went wrong, show form with error
                var vm = await BuildLoginViewModelAsync(model);
                return View(vm);
            }


            var privateSettings = await _globalConfigurationAppService.GetPrivateSettings();

            if (privateSettings.LoginStrategy == LoginStrategyType.Ldap)
                return await LoginByLdap(model, context);

            return await LoginByAspNetIdentity(model, context);



        }

        private async Task<IActionResult> LoginByLdap(LoginInputModel model, AuthorizationRequest context)
        {
            var privateSettings = await _globalConfigurationAppService.GetPrivateSettings();
            var ldap = new NovelLdapAuthentication(privateSettings.LdapSettings);
            UserViewModel userIdentity = null;
            try
            {
                userIdentity = ldap.Login(model.Username, model.Password);

                if (userIdentity.CustomClaims.ExistType(JwtClaimTypes.Name))
                    userIdentity.Name = userIdentity.CustomClaims.GetValue(JwtClaimTypes.Name);
                if (userIdentity.CustomClaims.ExistType("mail", JwtClaimTypes.Email))
                {
                    userIdentity.Email = userIdentity.CustomClaims.GetValue("mail", JwtClaimTypes.Email);
                    userIdentity.EmailConfirmed = true;
                }

                userIdentity.CustomClaims.Remove("mail", JwtClaimTypes.Email, JwtClaimTypes.Name);
            }
            catch (Exception e)
            {
                await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, e.Message));
                ModelState.AddModelError("", AccountOptions.InvalidCredentialsErrorMessage);
            }

            if (userIdentity != null)
            {
                var result = await DoLogin(userIdentity, model.RememberLogin);
                if (result.Succeeded)
                {
                    return await SuccessfullLogin(model, userIdentity, context);
                }
                else
                {
                    await FailedLogin(model, result, userIdentity);
                }
            }
            // something went wrong, show form with error
            var vm = await BuildLoginViewModelAsync(model);
            return View(vm);

        }

        private async Task<SignInResult> DoLogin(UserViewModel userIdentity, bool rememberLogin)
        {
            var user = await _userManager.FindByNameAsync(userIdentity.UserName);
            if (user == null)
            {
                await _userAppService.RegisterWithoutPassword(new RegisterWithoutPasswordViewModel()
                {
                    Username = userIdentity.UserName,
                    Name = userIdentity.Name,
                    Email = userIdentity.Email
                });

                user = await _userManager.FindByNameAsync(userIdentity.UserName);
            }

            await _userManageAppService.SynchronizeClaims(user.UserName, userIdentity.CustomClaims.Select(s => new ClaimViewModel(s.Type, s.Value)));

            var claims = new List<Claim>()
            {
                new Claim("amr", "pwd"),
                new Claim("amr", "ldap")
            };

            await _signInManager.SignInWithClaimsAsync(user, rememberLogin, claims.ToArray());

            return SignInResult.Success;
        }

        private async Task<IActionResult> LoginByAspNetIdentity(LoginInputModel model, AuthorizationRequest context)
        {
            UserViewModel userIdentity;
            if (model.IsUsernameEmail())
            {
                userIdentity = await _userManageAppService.FindByEmailAsync(model.Username);
            }
            else
            {
                userIdentity = await _userManageAppService.FindByUsernameAsync(model.Username);
            }

            if (userIdentity == null)
            {
                await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials"));
                ModelState.AddModelError("", AccountOptions.InvalidCredentialsErrorMessage);
            }

            if (userIdentity != null)
            {
                var result = await _signInManager.PasswordSignInAsync(userIdentity.UserName, model.Password, model.RememberLogin, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    return await SuccessfullLogin(model, userIdentity, context);
                }
                else
                {
                    await FailedLogin(model, result, userIdentity);
                }
            }

            // something went wrong, show form with error
            var vm = await BuildLoginViewModelAsync(model);
            return View(vm);
        }

        private async Task<IActionResult> SuccessfullLogin(LoginInputModel model, UserViewModel userIdentity, AuthorizationRequest context)
        {
            await _events.RaiseAsync(new UserLoginSuccessEvent(userIdentity.UserName, userIdentity.UserName, userIdentity.Name, clientId: context == null ? userIdentity.UserName : context.ClientId));

            if (context != null)
            {
                if (await _clientStore.IsPkceClientAsync(context.ClientId))
                {
                    // if the client is PKCE then we assume it's native, so this change in how to
                    // return the response is for better UX for the end user.
                    return View("Redirect", new RedirectViewModel { RedirectUrl = model.ReturnUrl });
                }

                // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                return Redirect(model.ReturnUrl);
            }

            // request for a local page
            if (Url.IsLocalUrl(model.ReturnUrl))
            {
                _logger.LogInformation($"Redirecting to ReturnUrl: {model.ReturnUrl}");
                return Redirect(model.ReturnUrl);
            }

            if (!ValidateUrl(model.ReturnUrl))
            {
                _logger.LogInformation($"Invalid return URL Redirecting to: {model.ReturnUrl}");
                return RedirectToAction("Index", "Grants");
            }

            // user might have clicked on a malicious link - should be logged
            await _events.RaiseAsync(new MaliciousRedirectUrlEvent(model.ReturnUrl, model.Username));
            throw new Exception("invalid return URL");
        }
        /// <summary>
        /// Validates a URL.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private bool ValidateUrl(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri validatedUri)) //.NET URI validation.
            {
                //If true: validatedUri contains a valid Uri. Check for the scheme in addition.
                return (validatedUri.Scheme == Uri.UriSchemeHttp || validatedUri.Scheme == Uri.UriSchemeHttps);
            }
            return false;
        }

        private async Task FailedLogin(LoginInputModel model, SignInResult result, UserViewModel userIdentity)
        {
            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", AccountOptions.AccountBlocked);
                await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, AccountOptions.AccountBlocked));
            }
            else
            {
                //if (!userIdentity.EmailConfirmed || !userIdentity.PhoneNumberConfirmed)
                if (!userIdentity.EmailConfirmed) // In case only e-mail to be confirmed
                {
                    ModelState.AddModelError("", AccountOptions.AccountNotConfirmedMessage);
                    await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, AccountOptions.AccountNotConfirmedMessage));
                }
                else
                {
                    ModelState.AddModelError("", AccountOptions.InvalidCredentialsErrorMessage);
                    await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, AccountOptions.InvalidCredentialsErrorMessage));
                }
            }
        }


        /// <summary>
        /// initiate roundtrip to external authentication provider
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExternalLogin(string provider, string returnUrl)
        {
            if (returnUrl.IsMissing()) returnUrl = "~/";

            // validate returnUrl - either it is a valid OIDC URL or back to a local page
            if (Url.IsLocalUrl(returnUrl) == false && _interaction.IsValidReturnUrl(returnUrl) == false)
            {
                // user might have clicked on a malicious link - should be logged
                throw new Exception("invalid return URL");
            }

            if (AccountOptions.WindowsAuthenticationSchemeName == provider)
            {
                // windows authentication needs special handling
                return await ProcessWindowsLoginAsync(returnUrl);
            }
            else
            {
                // start challenge and roundtrip the return URL and 
                var props = new AuthenticationProperties()
                {
                    RedirectUri = Url.Action(nameof(ExternalLoginCallback)),
                    Items =
                    {
                        { "returnUrl", returnUrl },
                        { "scheme", provider },
                    }
                };
                return Challenge(props, provider);
            }
        }

        /// <summary>
        /// Post processing of external authentication
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback()
        {
            // read external identity from the temporary cookie
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
            if (result?.Succeeded != true)
            {
                throw new Exception("External authentication error");
            }

            // lookup our user and external provider info
            var (user, provider, providerUserId, claims) = await FindUserFromExternalProviderAsync(result);
            if (user == null)
            {
                // this might be where you might initiate a custom workflow for user registration
                // in this sample we don't show how that would be done, as our sample implementation
                // simply auto-provisions new external user
                user = await AutoProvisionUserAsync(provider, providerUserId, claims);
                if (user == null)
                    return RedirectToAction("LoginError", "Home", new { Error = string.Join(" ", _notifications.GetNotifications().Select(a => $"{a.Key}: {a.Value}")) });

            }

            // this allows us to collect any additonal claims or properties
            // for the specific prtotocols used and store them in the local auth cookie.
            // this is typically used to store data needed for signout from those protocols.
            var additionalLocalClaims = new List<Claim>();
            var localSignInProps = new AuthenticationProperties();
            ProcessLoginCallbackForOidc(result, additionalLocalClaims, localSignInProps);
            ProcessLoginCallbackForWsFed(result, additionalLocalClaims, localSignInProps);
            ProcessLoginCallbackForSaml2p(result, additionalLocalClaims, localSignInProps);

            // issue authentication cookie for user

            var s = await _userManager.FindByNameAsync(user.UserName);
            var principal = await _signInManager.CreateUserPrincipalAsync(s);
            additionalLocalClaims.AddRange(principal.Claims);
            var name = principal.FindFirst(JwtClaimTypes.Name)?.Value ?? s.Id.ToString();

            await _events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, s.Id.ToString(), name));
            await HttpContext.SignInAsync(s.Id.ToString(), name, provider, localSignInProps, additionalLocalClaims.ToArray());

            // delete temporary cookie used during external authentication
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // validate return URL and redirect back to authorization endpoint or a local page
            var returnUrl = result.Properties.Items["returnUrl"];

            // check if external login is in the context of an OIDC request
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context != null)
            {
                if (await _clientStore.IsPkceClientAsync(context.ClientId))
                {
                    // if the client is PKCE then we assume it's native, so this change in how to
                    // return the response is for better UX for the end user.
                    return View("Redirect", new RedirectViewModel { RedirectUrl = returnUrl });
                }
            }

            return Redirect(returnUrl);
        }

        /// <summary>
        /// Show logout page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            // build a model so the logout page knows what to display
            var vm = await BuildLogoutViewModelAsync(logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await Logout(vm);
            }

            return View(vm);
        }

        /// <summary>
        /// Handle logout page postback
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // build a model so the logged out page knows what to display
            var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await _signInManager.SignOutAsync();

                // raise the logout event
                await _events.RaiseAsync(new UserLogoutSuccessEvent(_user.Username, User.GetDisplayName()));
            }

            // check if we need to trigger sign-out at an upstream identity provider
            if (vm.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect back
                // to us after the user has logged out. this allows us to then
                // complete our single sign-out processing.
                string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

                // this triggers a redirect to the external provider for sign-out
                return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
            }

            return View("LoggedOut", vm);

        }

        /*****************************************/
        /* helper APIs for the AccountController */
        /*****************************************/
        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null)
            {
                var local = context.IdP == IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                var client = await _clientStore.FindClientByIdAsync(context?.ClientId);
                var vm = new LoginViewModel
                {
                    EnableLocalLogin = local,
                    ReturnUrl = returnUrl,
                    Username = context?.LoginHint,
                    ShowDefaultUserPass = _configuration["ApplicationSettings:ShowDefaultUserPass"] == "true",
                    ClientLogo = client.LogoUri

                };

                if (!local)
                {
                    vm.ExternalProviders = new[] { new ExternalProvider { AuthenticationScheme = context.IdP } };
                }

                return vm;
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null ||
                            (x.Name.Equals(AccountOptions.WindowsAuthenticationSchemeName, StringComparison.OrdinalIgnoreCase))
                )
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;
            var clientLogo = string.Empty;
            if (context?.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.ClientId);
                if (client != null)
                {
                    clientLogo = client.LogoUri;
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            return new LoginViewModel
            {
                EnableExternalProviders = providers.Any() && _configuration.GetValue<bool>("ApplicationSettings:EnableExternalProviders"),
                AllowRememberLogin = AccountOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                Username = context?.LoginHint,
                ExternalProviders = providers.ToArray(),
                ShowDefaultUserPass = _configuration["ApplicationSettings:ShowDefaultUserPass"] == "true",
                ClientLogo = clientLogo
            };
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
        {
            var vm = await BuildLoginViewModelAsync(model.ReturnUrl);
            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;
            return vm;
        }

        private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
        {
            var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

            if (User?.Identity.IsAuthenticated != true)
            {
                // if the user is not authenticated, then just show logged out page
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            //if (context?.ShowSignoutPrompt == false)
            //{
            //    // it's safe to automatically sign-out
            //    vm.ShowLogoutPrompt = false;
            //    return vm;
            //}

            vm.Client = context?.ClientName;
            vm.PostLogoutRedirectUri = context?.PostLogoutRedirectUri;
            // show the logout prompt. this prevents attacks where the user
            // is automatically signed out by another malicious web page.
            return vm;
        }

        private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            if (User?.Identity.IsAuthenticated == true)
            {
                var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServer4.IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            // if there's no current logout context, we need to create one
                            // this captures necessary info from the current logged in user
                            // before we signout and redirect away to the external IdP for signout
                            vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }

        private async Task<IActionResult> ProcessWindowsLoginAsync(string returnUrl)
        {
            // see if windows auth has already been requested and succeeded
            var result = await HttpContext.AuthenticateAsync(AccountOptions.WindowsAuthenticationSchemeName);
            if (result?.Principal is WindowsPrincipal wp)
            {
                // we will issue the external cookie and then redirect the
                // user back to the external callback, in essence, tresting windows
                // auth the same as any other external authentication mechanism
                var props = new AuthenticationProperties()
                {
                    RedirectUri = Url.Action(nameof(ExternalLoginCallback)),
                    Items =
                    {
                        { "returnUrl", returnUrl },
                        { "scheme", AccountOptions.WindowsAuthenticationSchemeName },
                    }
                };

                var id = new ClaimsIdentity(AccountOptions.WindowsAuthenticationSchemeName);
                id.AddClaim(new Claim(JwtClaimTypes.Subject, wp.Identity.Name));
                id.AddClaim(new Claim(JwtClaimTypes.Name, wp.Identity.Name));

                // add the groups as claims -- be careful if the number of groups is too large
                if (AccountOptions.IncludeWindowsGroups)
                {
                    var wi = wp.Identity as WindowsIdentity;
                    var groups = wi.Groups.Translate(typeof(NTAccount));
                    var roles = groups.Select(x => new Claim(JwtClaimTypes.Role, x.Value));
                    id.AddClaims(roles);
                }

                await HttpContext.SignInAsync(
                    IdentityConstants.ExternalScheme,
                    new ClaimsPrincipal(id),
                    props);
                return Redirect(props.RedirectUri);
            }
            else
            {
                // trigger windows auth
                // since windows auth don't support the redirect uri,
                // this URL is re-triggered when we call challenge
                return Challenge(AccountOptions.WindowsAuthenticationSchemeName);
            }
        }

        private async Task<(UserViewModel user, string provider, string providerUserId, List<Claim> claims)>
            FindUserFromExternalProviderAsync(AuthenticateResult result)
        {
            var externalUser = result.Principal;

            // try to determine the unique id of the external user (issued by the provider)
            // the most common claim type for that are the sub claim and the NameIdentifier
            // depending on the external provider, some other claim type might be used
            var userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ??
                              externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
                              throw new Exception("Unknown userid");

            // remove the user id claim so we don't include it as an extra claim if/when we provision the user
            var claims = externalUser.Claims.ToList();
            claims.Remove(userIdClaim);

            var provider = result.Properties.Items["scheme"];
            var providerUserId = userIdClaim.Value;

            // find external user
            var user = await _userManageAppService.FindByProviderAsync(provider, providerUserId);

            return (user, provider, providerUserId, claims);
        }

        private async Task<UserViewModel> AutoProvisionUserAsync(string provider, string providerUserId, List<Claim> claims)
        {
            // create a list of claims that we want to transfer into our store
            var filtered = new List<Claim>();

            // user's display name
            var name = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Name)?.Value ?? claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;

            var username = claims.FirstOrDefault(x => x.Type == "user_name" || x.Type == "username")?.Value ?? claims.FirstOrDefault(x => x.Type == ClaimTypes.Sid)?.Value;

            if (name != null)
            {
                filtered.Add(new Claim(JwtClaimTypes.Name, name));
            }
            else
            {
                var first = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName)?.Value ??
                    claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value;
                var last = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.FamilyName)?.Value ??
                    claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value;
                if (first != null && last != null)
                {
                    filtered.Add(new Claim(JwtClaimTypes.Name, first + " " + last));
                }
                else if (first != null)
                {
                    filtered.Add(new Claim(JwtClaimTypes.Name, first));
                }
                else if (last != null)
                {
                    filtered.Add(new Claim(JwtClaimTypes.Name, last));
                }
            }

            // email
            var email = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Email)?.Value ?? claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            if (email != null)
            {
                filtered.Add(new Claim(JwtClaimTypes.Email, email));
            }

            //picture
            var picture = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Picture)?.Value ?? claims.FirstOrDefault(x => x.Type == "image")?.Value;

            var user = new SocialViewModel()
            {
                Username = username ?? email,
                Name = name,
                Email = email,
                Picture = picture,
                Provider = provider,
                ProviderId = providerUserId
            };

            var userExist = await _userAppService.CheckUsername(user.Username) ||
                            await _userAppService.CheckEmail(user.Email);

            if (userExist)
                await _userAppService.AddLogin(user);
            else
                await _userAppService.RegisterWithoutPassword(user);

            var claimsFromUser = filtered.Select(f => new SaveUserClaimViewModel() { Type = f.Type, Username = user.Username, Value = f.Value });
            foreach (var saveUserClaimViewModel in claimsFromUser)
            {
                await _userManageAppService.SaveClaim(saveUserClaimViewModel);
            }

            return await _userManageAppService.FindByProviderAsync(provider, providerUserId);
        }

        private void ProcessLoginCallbackForOidc(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
            // if the external system sent a session id claim, copy it over
            // so we can use it for single sign-out
            var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
            if (sid != null)
            {
                localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
            }

            // if the external provider issued an id_token, we'll keep it for signout
            var id_token = externalResult.Properties.GetTokenValue("id_token");
            if (id_token != null)
            {
                localSignInProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = id_token } });
            }
        }

        private void ProcessLoginCallbackForWsFed(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
        }

        private void ProcessLoginCallbackForSaml2p(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
        }
    }
}
