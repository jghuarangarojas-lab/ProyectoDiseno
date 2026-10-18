using MsServices.Application.DTOs;
using MsServices.Domain.Entities;

namespace MsServices.Application.Interfaces.External;

/// <summary>
/// Expone el plan de pagos hacia ms-commercial (caja) para que sepa cuándo y cuánto cobrar.
/// </summary>
public interface IBillingIntegration
{
    Task<BillingSyncResultDto> SendInstallmentsAsync(
        ServiceProject project,
        IReadOnlyList<PaymentInstallment> installments,
        CancellationToken cancellationToken = default);
}
