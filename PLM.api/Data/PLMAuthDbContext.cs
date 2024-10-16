using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace PLM.api.Data
{
    public class PLMAuthDbContext : IdentityDbContext       
    {
        public PLMAuthDbContext(DbContextOptions<PLMAuthDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            var UserRoleId = "e2150bc3-1789-4ad0-bf5b-5b84242089e7";            
            var AdminRoleId = "ffce0d45-fb01-4934-b27c-91cb1ccf19ba";
            var roles = new List<IdentityRole>
            {
                new IdentityRole
                {
                    Id = UserRoleId,
                    ConcurrencyStamp = UserRoleId,
                    Name = "User",
                    NormalizedName = "User".ToUpper()
                },                
                new IdentityRole
                {
                    Id = AdminRoleId,
                    ConcurrencyStamp = AdminRoleId,
                    Name = "Admin",
                    NormalizedName = "Admin".ToUpper()
                }


            };
            builder.Entity<IdentityRole>().HasData(roles);

        }
    }
}
