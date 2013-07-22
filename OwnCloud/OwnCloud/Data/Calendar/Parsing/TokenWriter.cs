using System.IO;

namespace OwnCloud.Data.Calendar.Parsing
{
    public static class TokenWriter
    {
        /// <summary>
        /// Schreibt ein TokenNode mit allen Tokens und Unterelementen in einen Stream
        /// </summary>
        public static void WriteTokenNode(TokenNode tokenNode, Stream target)
        {
            TextWriter writer = new StreamWriter(target);
            WriteTokenNode(tokenNode, writer);
            writer.Flush();
        }

        /// <summary>
        /// Schreibt ein TokenNode mit allen TOkens und Unterelementen in einen Writer
        /// </summary>
        public static void WriteTokenNode(TokenNode tokenNode, TextWriter writer)
        {
            writer.WriteLine("BEGIN:" + tokenNode.Name);

            foreach (var token in tokenNode.Tokens)
                WriteToken(token, writer);

            foreach (var childNode in tokenNode.Childs)
                WriteTokenNode(childNode, writer);

            writer.WriteLine("END:" + tokenNode.Name);
        }

        /// <summary>
        /// Schreibt ein Token in den Textwriter
        /// </summary>
        public static void WriteToken(Token token, TextWriter writer)
        {
            writer.WriteLine(token.Key + ":" + token.Value.EncodedValue);
        }

    }
}
