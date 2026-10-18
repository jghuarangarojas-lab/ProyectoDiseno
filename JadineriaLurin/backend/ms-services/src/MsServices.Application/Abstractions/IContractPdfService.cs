using MsServices.Domain.Entities;

namespace MsServices.Application.Abstractions;

/// <summary>
/// Genera el PDF del contrato a partir de una plantilla HTML/texto,
/// rellenada con los datos del proyecto y su plan de cuotas.
/// </summary>
public interface IContractPdfService
{
    Task<byte[]> GenerateAsync(
        ServiceProject project,
        IReadOnlyList<PaymentInstallment> installments,
        CancellationToken cancellationToken = default);
}
