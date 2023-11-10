using System.Security.Claims;

namespace ZATCA.API.Auth
{
    public interface IJWTManager
    {
        AuthTokens Authenticate(IEnumerable<Claim> claims);
    }
}
