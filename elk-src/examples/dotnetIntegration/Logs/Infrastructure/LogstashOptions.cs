namespace Logs.Infrastructure;

public sealed class LogstashOptions
{
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 5000;
    public bool UseSsl { get; init; } = true;
    public string? CaCertificatePath { get; init; }
    public bool AcceptAnyCertificate { get; init; } = false;
    public TimeSpan ConnectTimeout { get; init; } = TimeSpan.FromSeconds(5);
    public TimeSpan WriteTimeout { get; init; } = TimeSpan.FromSeconds(5);
}
