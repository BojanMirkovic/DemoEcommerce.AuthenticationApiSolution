using AuthenticationApi.Domain.EntityModel;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationApi.Infrastructure.Database
{
    public class AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options) : DbContext(options)
    {
       public DbSet<AppUserModel> Users { get; set; }
    }
}
