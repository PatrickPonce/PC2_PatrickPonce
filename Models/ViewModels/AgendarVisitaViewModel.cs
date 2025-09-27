using System;
using System.ComponentModel.DataAnnotations;

public class AgendarVisitaViewModel
{
    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    [DataType(DataType.DateTime)]
    public DateTime FechaInicio { get; set; }

    [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
    [DataType(DataType.DateTime)]
    public DateTime FechaFin { get; set; }

    public string Notas { get; set; }
}