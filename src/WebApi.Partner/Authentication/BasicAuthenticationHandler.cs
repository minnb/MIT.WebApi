using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using WCM.EntityFrameworkCore.EntityFrameworkCore.Partner;

namespace VCM.Partner.API.Authentication
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IMemoryCache _cache;
        private readonly PartnerDbContext _dbContext;
        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IMemoryCache cache,
            PartnerDbContext dbContext
            )
            : base(options, logger, encoder, clock)
        {
            _cache = cache;
            _dbContext = dbContext;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return Task.FromResult(AuthenticateResult.Fail("Authorization header was not found"));
            try
            {
                var authHeaderValue = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var bytes = Convert.FromBase64String(authHeaderValue.Parameter);
                string[] credentials = Encoding.UTF8.GetString(bytes).Split(":");
                string client_code = credentials[0];
                string hash_key = credentials[1];

                var key_Authorization = string.Format("{0}:{1}", client_code, hash_key);

                if (!_cache.TryGetValue(key_Authorization, out string cacheEntry))
                {
                    var app_user = _dbContext.User.Where(x => x.UserName == client_code && x.Password == hash_key).SingleOrDefault();
                    if (app_user != null)
                    {
                        cacheEntry = app_user.UserName.Trim() + ":" + app_user.Password.Trim();
                    }
                    else
                    {
                        return Task.FromResult(AuthenticateResult.Fail("Wrong authentication code"));
                    }
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSize(1)
                        .SetSlidingExpiration(TimeSpan.FromHours(24));
                    _cache.Set(key_Authorization, cacheEntry, cacheEntryOptions);
                }

                if (cacheEntry == client_code + ":" + hash_key)
                {
                    var claims = new[] { new Claim(ClaimTypes.Name, client_code) };
                    var identity = new ClaimsIdentity(claims, Scheme.Name);
                    var principal = new ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, Scheme.Name);

                    return Task.FromResult(AuthenticateResult.Success(ticket));
                }
                else
                {
                    return Task.FromResult(AuthenticateResult.Fail("Wrong authentication code"));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(AuthenticateResult.Fail("HandleAuthenticateAsync Exception " + ex.Message.ToString()));
            }

        }
    }
}
