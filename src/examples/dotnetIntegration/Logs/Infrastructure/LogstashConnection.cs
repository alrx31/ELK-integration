using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Logs.Infrastructure;

internal sealed class LogstashConnection : IDisposable
{
    private readonly LogstashOptions _options;
    private TcpClient? _client;
    private Stream? _stream;
    private readonly object _lock = new();
    private X509Certificate2? _caCertificate;

    public LogstashConnection(LogstashOptions options)
    {
        _options = options;
    }

    public void WriteLine(string line)
    {
        EnsureConnected();

        if (_stream == null)
        {
            return;
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(line + Environment.NewLine);
        _stream.Write(bytes, 0, bytes.Length);
        _stream.Flush();
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _stream?.Dispose();
            _client?.Dispose();
            _stream = null;
            _client = null;
        }
    }

    private void EnsureConnected()
    {
        lock (_lock)
        {
            if (_client?.Connected == true && _stream != null)
            {
                return;
            }

            Dispose();

            _client = new TcpClient
            {
                ReceiveTimeout = (int)_options.ConnectTimeout.TotalMilliseconds,
                SendTimeout = (int)_options.WriteTimeout.TotalMilliseconds
            };

            _client.Connect(_options.Host, _options.Port);
            var networkStream = _client.GetStream();

            if (_options.UseSsl)
            {
                _caCertificate ??= LoadCaCertificate(_options.CaCertificatePath);
                var sslStream = new SslStream(
                    networkStream,
                    false,
                    ValidateServerCertificate);

                sslStream.AuthenticateAsClient(new SslClientAuthenticationOptions
                {
                    TargetHost = _options.Host,
                    EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                    CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
                    RemoteCertificateValidationCallback = ValidateServerCertificate
                });

                _stream = sslStream;
            }
            else
            {
                _stream = networkStream;
            }
        }
    }

    private bool ValidateServerCertificate(
        object sender,
        X509Certificate? certificate,
        X509Chain? chain,
        SslPolicyErrors sslPolicyErrors)
    {
        if (_options.AcceptAnyCertificate)
        {
            return true;
        }

        if (certificate == null)
        {
            return false;
        }

        if (_caCertificate == null)
        {
            return sslPolicyErrors == SslPolicyErrors.None;
        }

        if (sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateNameMismatch))
        {
            return false;
        }

        using var customChain = new X509Chain();
        customChain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
        customChain.ChainPolicy.CustomTrustStore.Add(_caCertificate);
        customChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        customChain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;

        return customChain.Build(new X509Certificate2(certificate));
    }

    private static X509Certificate2? LoadCaCertificate(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return File.Exists(path) ? new X509Certificate2(path) : null;
    }
}
