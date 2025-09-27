using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PC2_PatrickPonce.Data;
using PC2_PatrickPonce.Models;
using System.Linq;
using System.Threading.Tasks;

public class InmuebleController : Controller
{
    private readonly ApplicationDbContext _context;

    public InmuebleController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string ciudad, TipoInmueble? tipo, double? precioMin, double? precioMax, int? dormitorios, int pagina = 1)
    {
        // 1. Validaciones del lado del servidor
        if (precioMin.HasValue && precioMax.HasValue && precioMin > precioMax)
        {
            ModelState.AddModelError("precioMax", "El precio máximo debe ser mayor o igual al precio mínimo.");
        }
        if (precioMin.HasValue && precioMin < 0)
        {
            ModelState.AddModelError("precioMin", "El precio mínimo no puede ser negativo.");
        }
        if (dormitorios.HasValue && dormitorios < 0)
        {
            ModelState.AddModelError("dormitorios", "El número de dormitorios no puede ser negativo.");
        }
        // Si hay errores de validación, se retornará la vista con los mensajes.
        if (!ModelState.IsValid)
        {
            return View(); // Retorna la vista con los errores
        }

        // 2. Lógica de filtrado
        var query = _context.Inmuebles.Where(i => i.Activo);

        if (!string.IsNullOrEmpty(ciudad))
        {
            query = query.Where(i => i.Ciudad.Contains(ciudad));
        }
        if (tipo.HasValue)
        {
            query = query.Where(i => i.Tipo == tipo.Value);
        }
        if (precioMin.HasValue)
        {
            query = query.Where(i => i.Precio >= precioMin.Value);
        }
        if (precioMax.HasValue)
        {
            query = query.Where(i => i.Precio <= precioMax.Value);
        }
        if (dormitorios.HasValue)
        {
            query = query.Where(i => i.Dormitorios >= dormitorios.Value);
        }

        // 3. Paginación simple (ejemplo)
        int pageSize = 10;
        var totalInmuebles = await query.CountAsync();
        var inmuebles = await query.Skip((pagina - 1) * pageSize).Take(pageSize).ToListAsync();

        // Pasa los datos a la vista
        ViewData["TotalPaginas"] = (int)Math.Ceiling(totalInmuebles / (double)pageSize);
        ViewData["PaginaActual"] = pagina;

        return View(inmuebles);
    }
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var inmueble = await _context.Inmuebles.FirstOrDefaultAsync(m => m.Id == id);
        if (inmueble == null)
        {
            return NotFound();
        }
        return View(inmueble);
    }

    [HttpPost]
    [Authorize] // Restringe el acceso a usuarios autenticados
    public async Task<IActionResult> AgendarVisita(int inmuebleId, AgendarVisitaViewModel model)
    {
        // Obtener el inmueble
        var inmueble = await _context.Inmuebles.FindAsync(inmuebleId);
        if (inmueble == null) return NotFound();

        // Validaciones del lado del servidor
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Hay errores en el formulario. Por favor, corrígelos.";
            return View("Details", inmueble);
        }

        // Validación 1: Fecha de inicio < Fecha de fin
        if (model.FechaInicio >= model.FechaFin)
        {
            ModelState.AddModelError(string.Empty, "La fecha de inicio debe ser anterior a la fecha de fin.");
        }
        
        // Validación 2: Horario laboral (ej. 8:00 a 19:00)
        if (model.FechaInicio.Hour < 8 || model.FechaInicio.Hour > 19 || model.FechaFin.Hour < 8 || model.FechaFin.Hour > 19)
        {
            ModelState.AddModelError(string.Empty, "Las visitas solo se pueden agendar en horario laboral (08:00 - 19:00).");
        }

        // Validación 3: No permitir dos visitas solapadas
        var visitasSolapadas = await _context.Visitas
            .Where(v => v.InmuebleId == inmuebleId && 
                        v.Estado != EstadoVisita.Cancelada &&
                        (model.FechaInicio < v.FechaFin && model.FechaFin > v.FechaInicio))
            .AnyAsync();

        if (visitasSolapadas)
        {
            ModelState.AddModelError(string.Empty, "Ya existe una visita agendada en ese intervalo de tiempo.");
        }

        // Si las validaciones fallan, redirige con los errores
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "No se pudo agendar la visita debido a los siguientes errores.";
            return View("Details", inmueble);
        }

        // Crear y guardar la visita
        var visita = new Visita
        {
            InmuebleId = inmuebleId,
            UsuarioId = _userManager.GetUserId(User), // Obtener el ID del usuario actual
            FechaInicio = model.FechaInicio,
            FechaFin = model.FechaFin,
            Notas = model.Notas,
            Estado = EstadoVisita.Solicitada
        };

        _context.Visitas.Add(visita);
        await _context.SaveChangesAsync();

        TempData["Success"] = "¡Visita agendada con éxito! Espera la confirmación del broker.";
        return RedirectToAction("Details", new { id = inmuebleId });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Reservar(int inmuebleId)
    {
        // Validación 1: Rechazar si ya existe una reserva activa para el inmueble
        var reservaActiva = await _context.Reservas
            .Where(r => r.InmuebleId == inmuebleId && r.FechaExpiracion > DateTime.Now)
            .AnyAsync();

        if (reservaActiva)
        {
            TempData["Error"] = "El inmueble ya tiene una reserva activa.";
            return RedirectToAction("Details", new { id = inmuebleId });
        }

        // Crear la nueva reserva por 48 horas
        var reserva = new Reserva
        {
            InmuebleId = inmuebleId,
            UsuarioId = _userManager.GetUserId(User),
            FechaCreacion = DateTime.Now,
            FechaExpiracion = DateTime.Now.AddHours(48)
        };

        _context.Reservas.Add(reserva);
        await _context.SaveChangesAsync();

        TempData["Success"] = "¡Inmueble reservado por 48 horas! Por favor, contacta con un broker para la documentación.";
        return RedirectToAction("Details", new { id = inmuebleId });
    }
}