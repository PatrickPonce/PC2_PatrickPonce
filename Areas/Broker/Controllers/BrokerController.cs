// Areas/Broker/Controllers/BrokerController.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using PC2_PatrickPonce.Data;
using PC2_PatrickPonce.Models;
using System.Linq;
using System.Threading.Tasks;

[Area("Broker")]
[Authorize(Roles = "Broker")]
public class BrokerController : Controller
{
    private readonly ApplicationDbContext _context;

    public BrokerController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Vista principal del panel
    public IActionResult Index()
    {
        return View();
    }
    
    // CRUD de Inmuebles
    public async Task<IActionResult> Inmuebles()
    {
        var inmuebles = await _context.Inmuebles.ToListAsync();
        return View(inmuebles);
    }

    // Acción para activar/desactivar un inmueble
    [HttpPost]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var inmueble = await _context.Inmuebles.FindAsync(id);
        if (inmueble == null) return NotFound();

        inmueble.Activo = !inmueble.Activo;
        _context.Inmuebles.Update(inmueble);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Estado del inmueble actualizado.";
        return RedirectToAction(nameof(Inmuebles));
    }
    
    // Vista de agenda y reservas
    public async Task<IActionResult> Agenda()
    {
        var today = DateTime.Today;
        var visitas = await _context.Visitas
            .Where(v => v.FechaInicio.Date == today)
            .Include(v => v.Inmueble)
            .Include(v => v.Usuario)
            .ToListAsync();
        
        var reservas = await _context.Reservas
            .Where(r => r.FechaExpiracion > DateTime.Now)
            .Include(r => r.Inmueble)
            .Include(r => r.Usuario)
            .ToListAsync();
            
        ViewBag.Visitas = visitas;
        ViewBag.Reservas = reservas;
        
        return View();
    }
    
    // Acción para confirmar una visita
    [HttpPost]
    public async Task<IActionResult> ConfirmarVisita(int id)
    {
        var visita = await _context.Visitas.FindAsync(id);
        if (visita == null) return NotFound();

        visita.Estado = EstadoVisita.Confirmada;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Visita confirmada.";
        return RedirectToAction(nameof(Agenda));
    }
    
    // Acción para liberar (cancelar) una reserva
    [HttpPost]
    public async Task<IActionResult> LiberarReserva(int id)
    {
        var reserva = await _context.Reservas.FindAsync(id);
        if (reserva == null) return NotFound();
        
        // Simplemente cambia la fecha de expiración para que no esté activa
        reserva.FechaExpiracion = DateTime.Now.AddMinutes(-1);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Reserva liberada.";
        return RedirectToAction(nameof(Agenda));
    }
}