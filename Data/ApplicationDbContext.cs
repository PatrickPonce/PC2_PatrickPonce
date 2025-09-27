using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace PC2_PatrickPonce.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Inmueble> Inmuebles { get; set; }
        public DbSet<Visita> Visitas { get; set; }
        public DbSet<Reserva> Reservas { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Inmueble>().HasData(
                new Inmueble
                {
                    Id = 1,
                    Codigo = "A001",
                    Titulo = "Apartamento céntrico",
                    Imagen = "departamento1.jpg",
                    Tipo = TipoInmueble.Departamento,
                    Ciudad = "Lima",
                    Direccion = "Av. Principal 123",
                    Dormitorios = 2,
                    Banos = 1,
                    MetrosCuadrados = 80,
                    Precio = 150000,
                    Activo = true
                },
                new Inmueble
                {
                    Id = 2,
                    Codigo = "C001",
                    Titulo = "Casa con jardín",
                    Imagen = "casa1.jpg",
                    Tipo = TipoInmueble.Casa,
                    Ciudad = "Arequipa",
                    Direccion = "Calle Los Olivos 456",
                    Dormitorios = 3,
                    Banos = 2,
                    MetrosCuadrados = 120,
                    Precio = 250000,
                    Activo = true
                }
            );
        }
    }
}

