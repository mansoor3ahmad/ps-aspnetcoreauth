using Globomantics.Models;
using Globomantics.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Globomantics.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserRepository _userRepository;

        public AccountController() 
        {
            _userRepository = new UserRepository();
        }

        [Route("/Login")]
        public IActionResult Login(string ReturnUrl = "/")
        {
            LoginModel model = new LoginModel();
            model.ReturnUrl = ReturnUrl;
            return View(model);
        }




        //You can access the User claims using User.Claims object both in the view and the controller

        [Route("/Login")]
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel loginUser) 
        {
            UserModel? user = _userRepository.GetByUserAndPassword(loginUser.Username, loginUser.Password);
            if (user == null)
            {
                return Unauthorized();
            }


            //To login user ClaimsPrincipal and scheme is neede
            //To create scheme List<Claim>,ClaimIdentity with schemeName, and ClaimPrincipal is created in the respective order


            //Claims can be of Default types or custom types like "Favorite Color"
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Name,user.Name),
                new Claim(ClaimTypes.Role,user.Role),
                new Claim("Favorite Color",user.FavoriteColor)
            };


            //Authentication scheme is mandatory
            var identity = new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);

            var pricipal = new ClaimsPrincipal(identity);
            
            //!!Need to pass the Scheme name while signin in the user
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, pricipal, new AuthenticationProperties()
            {
                IsPersistent = loginUser.RememberLogin
            });


            //Mention an redirect url to redirect after loginin
            return LocalRedirect(loginUser.ReturnUrl);
        }


        //custom trigger the OAuth Challenge
        //asp-action="LoginWithGoogle"
        //asp-route-returnUrl="@Model.ReturnUrl" is used to pass the returnUrl parameter value
        public IActionResult LoginWithGoogle(string returnUrl = "/")
        {
            //Need to mention an redirectUrl or else the challenge goes in a loop due to lack of redirection
            var prop = new AuthenticationProperties
            {
                //Redirect Uri prop is used to redirect the page after the login is successful
                //Redirected to GoogleCallBackFunction to have our customclaims instead of the google claims

                //Uncommnet this to see the default external google claims in the Conference.Index page
                //RedirectUri = returnUrl,
                RedirectUri = Url.Action("GoogleCallBackFunction"),

                //Items Prop is used to pass the desired valeus thorugh context object after the authentication
                Items =
                {
                    { "returnUrl" , returnUrl }
                },
            };

            //!! "Challenge method is used to trigger the OAuth Challenge by passing the SchemeName of the OAuth Provider
            return Challenge(prop,GoogleDefaults.AuthenticationScheme);
        }



        //Difference between AuthenticateAsync and SignInAsync, SignInAsync takes the ClaimsPrincipal and persists it
        //                                          but AuthenticateAsync takes the claimsPrincipal without persisting

        //In the above case the SignInAsync takes the ClaimsPrincipal and persists it with a identity cookie but in this case
        //          the google already will have a cookie we just need to access the calims provided by the google

        //Here we are trying to assign our own custom claims instead on reling on the external google claims 
        public async Task<IActionResult> GoogleCallBackFunction()
        {
            var result = await  HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if(result.Principal == null)
                throw new Exception("Could not create a principal");


            //Can read the claims from the result.Principal.Claims object
            var ExternalClaims = result.Principal.Claims.ToList();

            var SubjectId = ExternalClaims.FirstOrDefault(x=>x.Type==ClaimTypes.NameIdentifier);

            if (SubjectId == null)
                throw new Exception("subject Id cannot be extracted");

            var subjectIdValue = SubjectId.Value;


            //Getting the user form the local db by id claim,
            //      we can corelate the local user details and google is by storing the NameIdentifier at the first time in the db or by mapping the emailids 
            var User = _userRepository.GetGoogleId(subjectIdValue);

            if (User == null)
                throw new Exception("Local User not Found!!");

            List<Claim> claims = new List<Claim>() {
                new Claim(ClaimTypes.NameIdentifier,User.Id.ToString()),
                new Claim(ClaimTypes.Name,User.Name),
                new Claim(ClaimTypes.Role,User.Role),
                new Claim("Favorite Color",User.FavoriteColor),
                new Claim(ClaimTypes.Email,ExternalClaims.FirstOrDefault(x=>x.Type==ClaimTypes.Email)?.Value??"No Email Found")
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            return LocalRedirect(result.Properties?.Items["returnUrl"] ?? "/");
        }


        [Route("/LogOut")]
        public async Task<IActionResult> LogOut()
        {
            //Though the signout Async is done on the CookieAuthentication, the google Authentication is also signedout
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return LocalRedirect("/");
        }
    }
}
