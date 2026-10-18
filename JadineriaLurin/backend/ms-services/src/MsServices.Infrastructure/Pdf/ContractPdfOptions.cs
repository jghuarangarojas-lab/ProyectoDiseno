namespace MsServices.Infrastructure.Pdf;

public sealed class ContractPdfOptions
{
    public const string SectionName = "ContractPdf";

    /// <summary>Ruta opcional a una plantilla HTML/texto externa. Si está vacía se usa la plantilla embebida.</summary>
    public string TemplatePath { get; set; } = string.Empty;

    public string CompanyName { get; set; } = "Jardinería Lurín Smart";

    public string CompanyRuc { get; set; } = "20600000001";

    public string CompanyAddress { get; set; } = "Lurín, Lima - Perú";

    public string ContactEmail { get; set; } = "contacto@lurinsmart.pe";
}
