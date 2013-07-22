using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using OwnCloud.Data.Exceptions;

namespace OwnCloud.Data.Calendar.Parsing
{
    class ParserNodeToken : IParser<TokenNode, Stream>
    {
        private const string TokenBegin = "BEGIN";
        private const string TokenEnd = "END";

        public TokenNode Parse(Stream value)
        {
            var rootNode = new TokenNode();
            var tokenStack = new Stack<TokenNode>();

            using (var reader = new StreamReader(value))
            {
                string line = "";
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith(" "))
                    {
                        tokenStack.Peek().Tokens.Last().Value.EncodedValue += line.Substring(1, line.Length - 1);
                        continue;
                    }

                    var currentToken = new Token();
                    ParseSingleToken(ref currentToken, line);

                    switch (currentToken.NamingKey)
                    {
                        case TokenBegin:
                            tokenStack.Push(new TokenNode{ Name = currentToken.Value.EncodedValue});
                            break;
                        case TokenEnd:
                            var endElement = tokenStack.Pop();
                            if (currentToken.Value.EncodedValue != endElement.Name)
                                throw new ParsingICalException();
                            if (tokenStack.Count == 0)
                                rootNode.Childs.Add(endElement);
                            else
                                tokenStack.Peek().Childs.Add(endElement);
                            break;
                        default:
                            tokenStack.Peek().Tokens.Add(currentToken);
                            break;
                    }
                }
            }

            return rootNode;
        }

        private void ParseSingleToken(ref Token currentToken, string line)
        {
            var expression = new Regex(@"(.+):(.*)");
            var cMatch = expression.Match(line);

            if (cMatch.Success)
            {
                ComputeSimpleToken(cMatch,ref currentToken);
            }

        }

        private static void ComputeSimpleToken(Match cMatch, ref Token currentToken)
        {
            currentToken.Key = cMatch.Groups[1].Value;
            currentToken.Value.EncodedValue = cMatch.Groups[2].Value;
        }


    }
}
