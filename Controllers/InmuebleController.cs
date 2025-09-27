using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PC2_PatrickPonce.Data;
using PC2_PatrickPonce.Models;
using PC2_PatrickPonce.ViewModels;
using System.Linq;
using System.Threading.Tasks;

public class InmuebleController : Controller
{
    private readonly ApplicationDbContext _context;

    public InmuebleController(ApplicationDbContext context)
    {
        _context = context;
    }

// InmuebleController.cs
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] FiltrosInmuebleViewModel model)
    {
        // Las validaciones de Data Annotations se aplican aquí automáticamente.
        if (!ModelState.IsValid)
        {
            model.Inmuebles = new List<Inmueble>();
            return View(model);
        }
        
        // ... Tu lógica de filtrado aquí ...
        var query = _context.Inmuebles.Where(i => i.Activo);

        if (!string.IsNullOrEmpty(model.Ciudad))
        {
            query = query.Where(i => i.Ciudad.Contains(model.Ciudad));
        }
        // ... Continúa con el resto de los filtros usando 'model' ...
        
        // Asigna los resultados y los datos de paginación al ViewModel
        model.Inmuebles = await query.ToListAsync(); // O la lista paginada
        model.TotalPaginas = 10; // Ejemplo
        
        return View(model);
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
}