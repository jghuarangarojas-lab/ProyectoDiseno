namespace Iot.Infrastructure.External;

public sealed class IrrigationApiOptions
{
    public const string SectionName = "IrrigationApi";

    public string BaseUrl { get; set; } = "http://localhost:5083/";
    public int TimeoutSeconds { get; set; } = 10;
}
