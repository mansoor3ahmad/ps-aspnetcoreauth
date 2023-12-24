using Globomantics.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc.Authorization;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

//DefaultScheme names are available for all the Authentication Providers after including the neccesary packages from NuGet(NPM)

//set global authorization instead of individual controller or view
//need to use "using Microsoft.AspNetCore.Mvc.Authorization;"
//builder.Services.AddControllersWithViews(o => o.Filters.Add(new AuthorizeFilter()));

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

//to make the claims available other than for controllers and views
builder.Services.AddHttpContextAccessor();

//Make the Authentication a cookie type scheme
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o => {
//    //default Login path
//    //o.LoginPath = "/Account/Login";
//    //o.LogoutPath = "/Account/Logout";

//    o.LoginPath = "/Login";
//    //o.ReturnUrlParameter = "/";
//    o.Cookie.SameSite = SameSiteMode.Strict;

//    //can have events for valiadting the authentications
//    o.Events = new()
//    {
//    };
//});


//When default ChalleengeScheme is selected when the user is tried to access auntorized views or apis the mentioned scheme will is prompted
builder.Services.AddAuthentication(o =>
{
    o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    //o.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
    .AddCookie(
    o =>
    {
        //Adding login path here will redirect the user to the view where we can add differnet third party OAuth authentication challenges
        o.LoginPath = "/Login";
        //Setting the samesite property to strict is causing the challenge to redirect infinetly 
        //o.Cookie.SameSite = SameSiteMode.Strict;
        o.Events = new()
        {
        };
        //o.ReturnUrlParameter = "/";
    }
    )
//You can register for google OAuth and get the clientId and clientSecret
.AddGoogle(o =>
{
    o.ClientId = "810659392283-jd41i9bpnoe8s2ffnh2n95ahujhh780u.apps.googleusercontent.com";
    o.ClientSecret = "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";
});

builder.Services.AddSingleton<IConferenceRepository, ConferenceRepository>();
builder.Services.AddSingleton<IProposalRepository, ProposalRepository>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("default","{controller=Conference}/{action=Index}/{id?}");

app.Run();
