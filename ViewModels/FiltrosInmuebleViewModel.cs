using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PC2_PatrickPonce.Models;

namespace PC2_PatrickPonce.ViewModels
{
    public class FiltrosInmuebleViewModel
    {
        public string Ciudad { get; set; }
        public TipoInmueble? Tipo { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El precio mínimo debe ser un valor no negativo.")]
        public double? PrecioMin { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El precio máximo debe ser un valor no negativo.")]
        public double? PrecioMax { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El número de dormitorios no puede ser negativo.")]
        public int? Dormitorios { get; set; }

        public IEnumerable<Inmueble> Inmuebles { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
    }
}