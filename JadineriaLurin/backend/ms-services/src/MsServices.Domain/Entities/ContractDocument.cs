using MsServices.Domain.Exceptions;

namespace MsServices.Domain.Entities;

public class ContractDocument
{
    public Guid ContractId { get; set; }
    public Guid ProjectId { get; set; }
    public ServiceProject? Project { get; set; }
    public string? GeneratedPdfUrl { get; set; }
    public string? DigitalSignatureUrl { get; set; }
    public DateTime? SignedAt { get; set; }
    public string? SignedBy { get; set; }
    public DateTime GeneratedAt { get; set; }

    public bool IsSigned => SignedAt.HasValue && !string.IsNullOrWhiteSpace(DigitalSignatureUrl);

    public void AttachGeneratedPdf(string pdfUrl, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(pdfUrl))
            throw new DomainException("No se recibió la ruta del PDF generado.");

        GeneratedPdfUrl = pdfUrl;
        GeneratedAt = now;
    }

    public void Sign(string signatureImageUrl, string? signedBy, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(signatureImageUrl))
            throw new DomainException("No se recibió la imagen de la firma digital.");

        DigitalSignatureUrl = signatureImageUrl;
        SignedBy = signedBy;
        SignedAt = now;
    }
}
