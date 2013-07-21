using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace OwnCloud.Net
{
    class TLSHandshake
    {
        Dictionary<string, byte[]> _ciphers;
        Dictionary<string, byte[]> _compressionMethods;
        Dictionary<string, byte[]> _extensions;
        const int TLS_V1_RECORD_HANDSHAKE = 0x16;
        byte[] TLS_V1_VERSION = new byte[] { 0x03, 0x01 };
        const int TLS_HANDSHAKE_TYPE_CLIENT_HELLO = 1;
        const int TLS_RANDOMBYTES_LENGTH = 28;
        const int TLS_SESSIONID_LENGTH = 32;

        public const int TLS_HANDSHAKE_SERVER_HELLO = 0x01;
        public const int TLS_HANDSHAKE_CLIENT_HELLO = 0x02;
        public const int TLS_HANDSHAKE_CERTIFICATE = 0x0B;
        public const int TLS_HANDSHAKE_SERVER_HELLO_DONE = 0x0E;
        public const int TLS_HTTP_PORT = 443;

        public TLSHandshake()
        {
            _ciphers = new Dictionary<string, byte[]>() {
               {"TLS_RSA_WITH_NULL_MD5"                 , new byte[]  { 0x00,0x01 }},
                {"TLS_RSA_WITH_NULL_SHA"                 , new byte[]  { 0x00,0x02 }},
                {"TLS_RSA_WITH_NULL_SHA256"              , new byte[]  { 0x00,0x3B }},
                {"TLS_RSA_WITH_RC4_128_MD5"              , new byte[]  { 0x00,0x04 }},
                {"TLS_RSA_WITH_RC4_128_SHA"              , new byte[]  { 0x00,0x05 }},
                {"TLS_RSA_WITH_3DES_EDE_CBC_SHA"         , new byte[]  { 0x00,0x0A }},
                {"TLS_RSA_WITH_AES_128_CBC_SHA"          , new byte[]  { 0x00,0x2F }},
                {"TLS_RSA_WITH_AES_256_CBC_SHA"          , new byte[]  { 0x00,0x35 }},
                {"TLS_RSA_WITH_AES_128_CBC_SHA256"       , new byte[]  { 0x00,0x3C }},
                {"TLS_RSA_WITH_AES_256_CBC_SHA256"       , new byte[]  { 0x00,0x3D }},
                {"TLS_DH_DSS_WITH_3DES_EDE_CBC_SHA"      , new byte[]  { 0x00,0x0D }},
                {"TLS_DH_RSA_WITH_3DES_EDE_CBC_SHA"      , new byte[]  { 0x00,0x10 }},
                {"TLS_DHE_DSS_WITH_3DES_EDE_CBC_SHA"     , new byte[]  { 0x00,0x13 }},
                {"TLS_DHE_RSA_WITH_3DES_EDE_CBC_SHA"     , new byte[]  { 0x00,0x16 }},
                {"TLS_DH_DSS_WITH_AES_128_CBC_SHA"       , new byte[]  { 0x00,0x30 }},
                {"TLS_DH_RSA_WITH_AES_128_CBC_SHA"       , new byte[]  { 0x00,0x31 }},
                {"TLS_DHE_DSS_WITH_AES_128_CBC_SHA"      , new byte[]  { 0x00,0x32 }},
                {"TLS_DHE_RSA_WITH_AES_128_CBC_SHA"      , new byte[]  { 0x00,0x33 }},
                {"TLS_DH_DSS_WITH_AES_256_CBC_SHA"       , new byte[]  { 0x00,0x36 }},
                {"TLS_DH_RSA_WITH_AES_256_CBC_SHA"       , new byte[]  { 0x00,0x37 }},
                {"TLS_DHE_DSS_WITH_AES_256_CBC_SHA"      , new byte[]  { 0x00,0x38 }},
                {"TLS_DHE_RSA_WITH_AES_256_CBC_SHA"      , new byte[]  { 0x00,0x39 }},
                {"TLS_DH_DSS_WITH_AES_128_CBC_SHA256"    , new byte[]  { 0x00,0x3E }},
                {"TLS_DH_RSA_WITH_AES_128_CBC_SHA256"    , new byte[]  { 0x00,0x3F }},
                {"TLS_DHE_DSS_WITH_AES_128_CBC_SHA256"   , new byte[]  { 0x00,0x40 }},
                {"TLS_DHE_RSA_WITH_AES_128_CBC_SHA256"   , new byte[]  { 0x00,0x67 }},
                {"TLS_DH_DSS_WITH_AES_256_CBC_SHA256"    , new byte[]  { 0x00,0x68 }},
                {"TLS_DH_RSA_WITH_AES_256_CBC_SHA256"    , new byte[]  { 0x00,0x69 }},
                {"TLS_DHE_DSS_WITH_AES_256_CBC_SHA256"   , new byte[]  { 0x00,0x6A }},
                {"TLS_DHE_RSA_WITH_AES_256_CBC_SHA256"   , new byte[]  { 0x00,0x6B }}
            };
            _compressionMethods = new Dictionary<string, byte[]>()
            {
                {"null", new byte[] {0}}
            };

            _extensions = new Dictionary<string, byte[]>()
            {
                {"elliptic_curves", new byte[] { 
                    0x00, 0x0A, // identifier
                    0x00, 0x08, // length
                    0x00, 0x06, // curves length
                    0x00, 0x17, // curve SECP256r1,
                    0x00, 0x18, // curve SECP384r1,
                    0x00, 0x19  // curve SECP521r1
                }},
                {"ec_point_formats", new byte[] {
                    0x00, 0x0B, // identifier
                    0x00, 0x02, // length
                    0x01, // length
                    0x00 // Uncompressed curve format
                }},
                {"SessionTicket TLS", new byte[] {
                    0x00, 0x23, // identifier
                    0x00, 0x00 // length
                }},
                {"next_protocol_negotiation", new byte[] {
                    0x33, 0x74, // identifier
                    0x00, 0x00 // length
                }}
            };
        }

        /// <summary>
        /// Adds an extension.
        /// Note that some standard extensions are already set.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetExtension(string name, byte[] value)
        {
            _extensions.Add(name, value);
        }

        /// <summary>
        /// Sets the server_name Extension.
        /// </summary>
        /// <param name="name"></param>
        public void SetServerNameExtension(string name)
        {
            byte[] totalLength = BitConverter.GetBytes((short)(name.Length + 5));
            if (BitConverter.IsLittleEndian) Array.Reverse(totalLength);

            byte[] listLength = BitConverter.GetBytes((short)(name.Length + 3));
            if (BitConverter.IsLittleEndian) Array.Reverse(listLength);

            byte[] nameLength = BitConverter.GetBytes((short)(name.Length));
            if (BitConverter.IsLittleEndian) Array.Reverse(nameLength);

            byte[] ext_inf = new byte[] {      
                    0x00, 0x00, // identifier
                    totalLength[0], totalLength[1],
                    listLength[0], listLength[1],
                    0x00, // Typ: hostname
                    nameLength[0], nameLength[1]
            };

            byte[] ext = new byte[ext_inf.Length + name.Length];
            Buffer.BlockCopy(ext_inf, 0, ext, 0, ext_inf.Length);
            Buffer.BlockCopy(Encoding.UTF8.GetBytes(name), 0, ext, ext_inf.Length, name.Length);

            if (_extensions.ContainsKey("server_name"))
            {
                _extensions["server_name"] = ext;
            }
            else
            {
                _extensions.Add("server_name", ext);
            }
        }

        /// <summary>
        /// Creates a TLSv1 Handshake Frame
        /// </summary>
        /// <returns></returns>
        public byte[] CreateClientHello()
        {
            MemoryStream stream = new MemoryStream();
            stream.WriteByte(TLS_V1_RECORD_HANDSHAKE);
            stream.Write(TLS_V1_VERSION, 0, 2);
            stream.Write(new byte[] { 0, 0 }, 0, 2); // length 16bit, offset 3
            stream.Write(new byte[] { TLS_HANDSHAKE_TYPE_CLIENT_HELLO }, 0, 1);
            stream.Write(new byte[] { 0, 0, 0 }, 0, 3); // handshake length 24bit, offset 6
            stream.Write(TLS_V1_VERSION, 0, 2);

            // timestamp
            TimeSpan span = (DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
            byte[] time = BitConverter.GetBytes((UInt32)span.TotalSeconds);
            if(BitConverter.IsLittleEndian) Array.Reverse(time);
            stream.Write(time, 0, 4);

            // Random bytes
            var rand = new Random();
            byte[] random_bytes = new byte[TLS_RANDOMBYTES_LENGTH];
            rand.NextBytes(random_bytes);
            stream.Write(random_bytes, 0, TLS_RANDOMBYTES_LENGTH);
            
            // session id
            stream.Write(new byte[] { TLS_SESSIONID_LENGTH }, 0, 1);
            byte[] session_id = new byte[TLS_SESSIONID_LENGTH];
            rand.NextBytes(session_id);
            stream.Write(session_id, 0, TLS_SESSIONID_LENGTH);
            // cipher list, 16bit
            byte[] cipherCount = BitConverter.GetBytes((short)(_ciphers.Count * 2));
            if(BitConverter.IsLittleEndian) Array.Reverse(cipherCount);
            stream.Write(cipherCount, 0, 2);
            foreach (KeyValuePair<string, byte[]> cipher in _ciphers)
            {
                stream.Write(cipher.Value, 0, 2);
            }

            // compression methods, 8bit
            stream.Write(new byte[] { (byte)_compressionMethods.Count }, 0, 1);
            foreach (KeyValuePair<string, byte[]> method in _compressionMethods)
            {
                stream.Write(method.Value, 0, 1);
            }

            // extensions, 16bit
            int ext_length = 0;
            foreach (KeyValuePair<string, byte[]> ext in _extensions)
            {
                ext_length += ext.Value.Length;
            }

            byte[] extension_len = BitConverter.GetBytes((short)ext_length);
            if (BitConverter.IsLittleEndian) Array.Reverse(extension_len);
            stream.Write(extension_len, 0, 2);
            foreach (KeyValuePair<string, byte[]> ext in _extensions)
            {
                stream.Write(ext.Value, 0, ext.Value.Length);
            }

            // update length: streamsize - handshake - version - length (5)
            stream.Seek(3, SeekOrigin.Begin);
            byte[] length = BitConverter.GetBytes((short)(stream.Length - 5));
            if(BitConverter.IsLittleEndian) Array.Reverse(length);
            stream.Write(length, 0, 2);

            // update handshake length: streamsize - handshake - version - length - handshake type - length
            stream.Seek(6, SeekOrigin.Begin);
            byte[] handshake_length = BitConverter.GetBytes((int)(stream.Length - 9));
            if (BitConverter.IsLittleEndian) Array.Reverse(handshake_length);
            stream.Write(handshake_length, 1, 3);

            byte[] buffer = stream.GetBuffer();
            byte[] result = new byte[stream.Length];
            Buffer.BlockCopy(buffer, 0, result, 0, (int)stream.Length);
            return result;
        }



        /// <summary>
        /// Seeks for a typical protocol type and returns
        /// this packet. Input must be a valid TLS-Record-Layer-Packet.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="TLSHandshakeType"></param>
        /// <returns></returns>
        public byte[] FindPacket(byte[] input, int TLSHandshakeType)
        {
            if (input.Length == 0 || input[0] != TLS_V1_RECORD_HANDSHAKE) return new byte[0];
            MemoryStream stream = new MemoryStream(input, 0, input.Length, false);
            while (stream.Position < stream.Length)
            {
                if (stream.ReadByte() == TLS_V1_RECORD_HANDSHAKE)
                {
                    // jump over Version
                    stream.Seek(2, SeekOrigin.Current);
                    // find length in next 2 bytes
                    stream.Seek(2, SeekOrigin.Current);
                    // compare handshake type
                    if (stream.ReadByte() == TLSHandshakeType)
                    {
                        // find length in next 3 bytes
                        byte[] length = new byte[4];
                        stream.Read(length, 1, 3);
                        if (BitConverter.IsLittleEndian) Array.Reverse(length);
                        int packet_length = BitConverter.ToInt32(length, 0);
                        byte[] packet = new byte[packet_length];
                        // copy packet
                        stream.Read(packet, 0, packet_length);
                        // close stream
                        stream.Close();
                        return packet;
                    }
                }
            }
            return new byte[0];
        }

        /// <summary>
        /// Reads a binary X.509 certificate and returns a dictionary
        /// with details. The certicate chain must be given.
        /// 
        /// </summary>
        /// <param name="input">byte[]</param>
        /// <returns>A list containing dictionary with all found X.509 ASN Attributes.</returns>
        public List<Dictionary<string, string>> GetCertificateDetails(byte[] input)
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            MemoryStream stream = new MemoryStream(input, 0, input.Length, false);

            // starts with chain len
            stream.Seek(3, SeekOrigin.Begin);

           while(stream.Position < stream.Length) {
               // starts with len
               byte[] c_len = new byte[4];
               stream.Read(c_len, 1, 3);
               if (BitConverter.IsLittleEndian) Array.Reverse(c_len);
               int len = BitConverter.ToInt32(c_len, 0);
               byte[] cert = new byte[len];
               stream.Read(cert, 0, len);
               list.Add(_CertDetails(cert));
           }

           return list;
        }

        private Dictionary<string, string> _CertDetails(byte[] input)
        {
            var dict = new Dictionary<string, string>();
            try
            {
                var c = new X509Certificate(input);
                string[] attributeToken = c.Subject.Split(',');
                foreach (string token in attributeToken)
                {
                    string[] p = token.Split('=');
                    dict.Add(p[0].Trim(), p[1].Trim());
                }

                dict.Add("ValidAfter", c.GetEffectiveDateString());
                dict.Add("ValidTo", c.GetExpirationDateString());
            }
            catch (Exception)
            {

            }
            return dict;
        }
    }
}
