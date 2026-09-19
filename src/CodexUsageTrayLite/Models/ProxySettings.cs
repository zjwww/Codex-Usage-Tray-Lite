namespace CodexUsageTrayLite.Models
{
    internal enum ProxyMode
    {
        System,
        Direct,
        Http,
        Socks5
    }

    internal sealed class ProxySettings
    {
        public ProxyMode mode { get; set; }
        public string host { get; set; }
        public int port { get; set; }

        public ProxySettings Clone()
        {
            return new ProxySettings { mode = mode, host = host, port = port };
        }
    }
}
