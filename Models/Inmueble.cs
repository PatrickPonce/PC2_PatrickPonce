using System.ComponentModel.DataAnnotations;

public enum TipoInmueble {
    Departamento,
    Casa,
    Oficina,
    Local
}

public class Inmueble {
    public int Id { get; set; }
    [Required]
    [MaxLength(50)]
    public string Codigo { get; set; }
    [Required]
    public string Titulo { get; set; }
    public string Imagen { get; set; }
    public TipoInmueble Tipo { get; set; }
    public string Ciudad { get; set; }
    public string Direccion { get; set; }
    [Range(0, int.MaxValue)]
    public int Dormitorios { get; set; }
    [Range(0, int.MaxValue)]
    public int Banos { get; set; }
    [Range(0, double.MaxValue)]
    public double MetrosCuadrados { get; set; }
    [Range(0, double.MaxValue)]
    public double Precio { get; set; }
    public bool Activo { get; set; }
}