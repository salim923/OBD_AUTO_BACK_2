using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using OBD.Domain.Entities;

namespace OBD.Infrastructure.Persistence
{
    public class ObdDbContext : DbContext, IInfrastructure<IServiceProvider>, IResettableService, IDisposable, IAsyncDisposable
    {
        public ObdDbContext(DbContextOptions<ObdDbContext> options) : base(options)
        {
        }

        internal DbSet<User> Users { get; set; }
        internal DbSet<Cars> Cars { get; set; }
        internal DbSet<AllCars> OBD2 { get; set; }
    }
}
