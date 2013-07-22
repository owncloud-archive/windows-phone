using System.Collections.Generic;

namespace OwnCloud.Data.Calendar.Parsing
{
    public class TokenNode
    {
        public string Name { get; set; }

        private List<Token> _tokens = new List<Token>();
        /// <summary>
        /// A list of all token in this node
        /// </summary>
        public List<Token> Tokens
        {
            get { return _tokens; }
            set { _tokens = value; }
        }

        private List<TokenNode> _childs = new List<TokenNode>();
        /// <summary>
        /// A list of all child nodes
        /// </summary>
        public List<TokenNode> Childs
        {
            get { return _childs; }
            set { _childs = value; }
        }


        public override string ToString()
        {
            return Name;
        }

    }
}
