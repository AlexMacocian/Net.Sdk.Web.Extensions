namespace AspNetCore.Extensions.Options;

public sealed class CorrelationVectorOptions
{
    public string Header { get; set; } = "X-Correlation-Vector";
}
