using System;

namespace OwnCloud.Net
{
    /// <summary>
    /// Stellt die Adresse zu einem Owncloud Server da
    /// </summary>
    class OwncloudAddress
    {
        public OwncloudAddress(string owncloudAddress)
        {
            OcAddress = owncloudAddress.Trim("/".ToCharArray());
        }

        /// <summary>
        /// Die komplette Adresse zu der Owncloud Instanz
        /// </summary>
        public string OcAddress { get; private set; }

        private string _serverAddress;
        /// <summary>
        /// Die Adresse zu dem Owncloud Server (nich zu der Instanz. ALso evt. ohne den "Path"-Teil)
        /// </summary>
        public string ServerAddress
        {
            get
            {
                if (_serverAddress == null)
                    _serverAddress = GetServerAddress();
                return _serverAddress;
            }
        }

        /// <summary>
        /// Berechnet die Serveradresse
        /// </summary>
        private string GetServerAddress()
        {
            UriBuilder builder = new UriBuilder(OcAddress);
            builder.Query = "";
            builder.Path = "";
            builder.Fragment = "";

            return builder.ToString();
        }

        public string Combine(string part)
        {
            return OcAddress + "/" + part;
        }

        public string CombineServerAddress(string part)
        {
            return GetServerAddress().Trim("/".ToCharArray()) + "/"
                   + part.Trim("/".ToCharArray());
        }

    }
}
