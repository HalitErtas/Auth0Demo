using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

var Auth0Section = builder.Configuration.GetSection("Auth0");
var Domain = Auth0Section["Domain"];
var ClientId = Auth0Section["ClientId"];
var ClientSecret = Auth0Section["ClientSecret"];


// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie().AddOpenIdConnect("Auth0", options =>
{
    //set the auhtority to the Auth0 domain
    options.Authority = $"https://{Domain}";

    //configure Auth0 credentials
    options.ClientId = ClientId ;
    options.ClientSecret = ClientSecret;

    //set response type to code
    options.ResponseType = OpenIdConnectResponseType.Code;

    //set scope 
    options.Scope.Clear();
    options.Scope.Add("openid");

    /*
     * options.CallbackPath = new PathString("/callback"); ifadesi, 
     * options nesnesinin "CallbackPath" özelliğine "/callback" yolunu atar. 
     * Yani, yetkilendirme işlemi tamamlandıktan sonra kullanıcı uygulamaya yönlendirildiğinde, 
     * bu yol kullanılır. Bu yol, yetkilendirme sunucusundan gelen yanıttaki geri çağrı URL'sini belirtir 
     * ve uygulamanın bu URL'yi işleyerek gerekli işlemleri gerçekleştirmesini sağlar.
     */
    options.CallbackPath = new PathString("/callback");

    options.ClaimsIssuer = "Auth0";

    options.Events = new Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectEvents
    {
        OnRedirectToIdentityProviderForSignOut = (context) =>
        {
            var logouturi = $"http://{Domain}/v2/logout?client_id={ClientId}/client_secret={ClientSecret}";

            var postLogoutUri = context.Properties.RedirectUri;
            if (!string.IsNullOrEmpty(postLogoutUri))
            {
                if (postLogoutUri.StartsWith("/"))
                {
                    var request = context.Request;
                    postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase + postLogoutUri;
                }

                var uri = Uri.EscapeDataString(postLogoutUri);
                logouturi += $"&returnTo={uri}";
            }

            context.Response.Redirect(postLogoutUri);
            context.HandleResponse();

            return Task.CompletedTask;
        }
    };

});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
