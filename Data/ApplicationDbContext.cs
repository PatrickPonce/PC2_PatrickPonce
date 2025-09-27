using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PC2_PatrickPonce.Data;

public class ApplicationDbContext : IdentityDbContext {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options) {
    }
    public DbSet<Inmueble> Inmuebles { get; set; }
    public DbSet<Visita> Visitas { get; set; }
    public DbSet<Reserva> Reservas { get; set; }
}
