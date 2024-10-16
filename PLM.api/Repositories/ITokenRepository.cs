using Microsoft.AspNetCore.Identity;

namespace PLM.api.Repositories
{
    public interface ITokenRepository
    {
        string CreateJWTToken(IdentityUser user, List<string> roles);

    }
}
