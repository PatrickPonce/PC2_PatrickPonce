using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PC2_PatrickPonce.Data;
using PC2_PatrickPonce.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http; // Necesario para IHttpContextAccessor y Session
using System.Text.Json; // Necesario para serialización
using Microsoft.AspNetCore.Identity;
using System.Security.Claims; // Necesario para User.FindFirstValue
using Microsoft.Extensions.Caching.Distributed;

public class InmuebleController : Controller
{
    private readonly ApplicationDbContext _context;

    public InmuebleController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private readonly IDistributedCache _cache;

    public InmuebleController(ApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
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

        if (string.IsNullOrEmpty(ciudad) && !tipo.HasValue && !precioMin.HasValue && !precioMax.HasValue && !dormitorios.HasValue)
        {
            var filtrosJson = HttpContext.Session.GetString("FiltrosCatalogo");
            if (!string.IsNullOrEmpty(filtrosJson))
            {
                var filtrosGuardados = JsonSerializer.Deserialize<dynamic>(filtrosJson);
                ciudad = filtrosGuardados.Ciudad;
                tipo = filtrosGuardados.Tipo;
                // Asigna el resto de los filtros
            }
        }

        // 3. Paginación simple (ejemplo)
        int pageSize = 10;
        var totalInmuebles = await query.CountAsync();
        var inmuebles = await query.Skip((pagina - 1) * pageSize).Take(pageSize).ToListAsync();
        var filtros = new
        {
            Ciudad = ciudad,
            Tipo = tipo,
            PrecioMin = precioMin,
            PrecioMax = precioMax,
            Dormitorios = dormitorios
        };

        HttpContext.Session.SetString("FiltrosCatalogo", JsonSerializer.Serialize(filtros));

        // Pasa los datos a la vista
        ViewData["TotalPaginas"] = (int)Math.Ceiling(totalInmuebles / (double)pageSize);
        ViewData["PaginaActual"] = pagina;

        return View(inmuebles);

        var cacheKey = $"inmuebles_ciudad_{ciudad}_tipo_{tipo}_precioMin_{precioMin}_precioMax_{precioMax}_dorm_{dormitorios}_page_{pagina}";

        // Intentar obtener el listado de la caché de Redis
        var cachedInmuebles = await _cache.GetStringAsync(cacheKey);

        if (cachedInmuebles != null)
        {
            var inmuebles = JsonSerializer.Deserialize<List<Inmueble>>(cachedInmuebles);
            // Pasa los datos de la caché a la vista
            return View(inmuebles);
        }

        // Si no está en caché, consulta la base de datos
        var query = _context.Inmuebles.Where(i => i.Activo);
        // ... (lógica de filtrado existente) ...

        var inmueblesDesdeDb = await query.ToListAsync();

        // Guardar los resultados en la caché de Redis por 60 segundos
        var cacheOptions = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(60));

        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(inmueblesDesdeDb), cacheOptions);

        return View(inmueblesDesdeDb);
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
        // ... (dentro de la acción Details, después de encontrar el inmueble) ...

        HttpContext.Session.SetInt32("UltimoInmuebleId", inmueble.Id);
        HttpContext.Session.SetString("UltimoInmuebleTitulo", inmueble.Titulo);
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
    
    // En las acciones de CRUD del InmuebleController
    [HttpPost]
    public async Task<IActionResult> Edit(Inmueble inmueble)
    {
        if (ModelState.IsValid)
        {
            // ... (lógica para guardar el inmueble) ...
            await _cache.RemoveAsync("inmuebles_all_filters"); // Clave general de invalidación

            // O mejor aún, puedes usar un patrón para invalidar todas las claves relacionadas
            // (esto requiere una biblioteca adicional o un script de Redis)

            return RedirectToAction(nameof(Index));
        }
        return View(inmueble);
    }
}