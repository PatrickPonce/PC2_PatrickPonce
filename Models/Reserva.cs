using System;
using Microsoft.AspNetCore.Identity;

public class Reserva {
    public int Id { get; set; }
    public int InmuebleId { get; set; }
    public Inmueble Inmueble { get; set; }
    public string UsuarioId { get; set; }
    public IdentityUser Usuario { get; set; }
    public DateTime FechaExpiracion { get; set; }
    public DateTime FechaCreacion { get; set; }
}