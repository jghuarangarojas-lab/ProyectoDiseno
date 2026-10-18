namespace MsDelivery.Infrastructure.WhatsApp;

public sealed class WhatsAppOptions
{
    public const string SectionName = "WhatsApp";

    /// <summary> Cuando es true solo se registra el mensaje en log (sin llamadas externas). </summary>
    public bool Mock { get; set; } = true;
    public string ApiUrl { get; set; } = "https://graph.facebook.com";
    public string ApiVersion { get; set; } = "v20.0";
    public string PhoneNumberId { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string DefaultCountryCode { get; set; } = "51";
}
