namespace MsServices.Infrastructure.Billing;

public sealed class MsCommercialOptions
{
    public const string SectionName = "MsCommercial";

    public string BaseUrl { get; set; } = "http://localhost:3000";

    public string InstallmentsPath { get; set; } = "/api/commercial/billing/installments";

    public int TimeoutSeconds { get; set; } = 10;
}
