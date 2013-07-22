namespace OwnCloud.Data.Calendar.Parsing
{
    /// <summary>
    /// A token is a Key (maybe contains a subkey) and the value eg. MyKey:MyValue
    /// </summary>
    public class Token
    {
        public string NamingKey {
            get
            {
                if (_key.IndexOf(';') == -1)
                    return _key;

                return _key.Split(';')[0];
            }
        }

        private string _key = "";
        /// <summary>
        /// The key of the token
        /// </summary>
        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

        /// <summary>
        /// Content of the subkey
        /// </summary>
        public string SubKey
        {
            get
            {
                return _key.IndexOf(';') == -1 ? null : _key.Split(';')[1];
            }
            set
            {
                if (_key.IndexOf(';') == -1)
                    _key = _key + (string.IsNullOrEmpty(value) ? "" : ";") + value;
                else
                    _key = _key.Split(';')[0] + (string.IsNullOrEmpty(value) ? "" : ";") + value;
            }
        }


        private EncodedTokenValue _value = new EncodedTokenValue();
        /// <summary>
        /// The value of the token
        /// </summary>
        public EncodedTokenValue Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public override string ToString()
        {
            return Key + ":" + Value.DecodedValue;
        }
    }
}
