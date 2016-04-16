using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompositionExpressionToolkit
{
    internal enum BracketType
    {
        None = 0,               //
        Round = 1,              // ( )
        Square = 2,             // [ ]
        Curly = 3,              // { }
        Angle = 4,              // < >
        Apostrophe = 5,         // ' '
        Quotes = 6,             // " "
        AngleXml = 7,           // &lt; &gt;
        ApostropheXml = 8,      // &apos; &apos;
        QuotesXml = 9           // &quot; &quot;
    }

    internal abstract class ExpressionToken
    {
        public abstract void Write(StringBuilder sb);

        public override string ToString()
        {
            var sb = new StringBuilder();
            Write(sb);
            return sb.ToString();
        }
    }

    internal class SimpleExpressionToken : ExpressionToken
    {
        public readonly string Text;

        public SimpleExpressionToken(string text)
        {
            Text = text;
        }

        public override void Write(StringBuilder sb)
        {
            sb.Append(Text);
        }
    }

    internal class CompositeExpressionToken : ExpressionToken
    {
        private string _openBracket;
        private string _closeBracket;
        private BracketType _bracketType;
        private readonly bool _addCommas;
        private readonly List<ExpressionToken> _tokens;
            
        public CompositeExpressionToken(BracketType bracketType = BracketType.None, 
            bool addCommas = false)
        {
            SetBrackets(bracketType);
            _addCommas = addCommas;
            _tokens = new List<ExpressionToken>();
        }

        public CompositeExpressionToken(string tokenStr, BracketType bracketType)
        : this(bracketType)
        {
            AddToken(tokenStr);
        }

        public CompositeExpressionToken(ExpressionToken expressionToken, BracketType bracketType)
            : this(bracketType)
        {
            AddToken(expressionToken);
        }

        public CompositeExpressionToken(IEnumerable<ExpressionToken> tokens, BracketType bracketType, bool addCommas)
            : this(bracketType, addCommas)
        {
            _tokens.AddRange(tokens.Where(t => t != null));
        }

        public void SetBrackets(BracketType bracketType)
        {
            _bracketType = bracketType;
            switch (bracketType)
            {
                case BracketType.Round:
                    _openBracket = "(";
                    _closeBracket = ")";
                    break;
                case BracketType.Square:
                    _openBracket = "[";
                    _closeBracket = "]";
                    break;
                case BracketType.Curly:
                    _openBracket = "{";
                    _closeBracket = "}";
                    break;
                case BracketType.Angle:
                    _openBracket = "<";
                    _closeBracket = ">";
                    break;
                case BracketType.Apostrophe:
                    _openBracket = "\'";
                    _closeBracket = "\'";
                    break;
                case BracketType.Quotes:
                    _openBracket = "\"";
                    _closeBracket = "\"";
                    break;
                case BracketType.AngleXml:
                    _openBracket = "&lt;";
                    _closeBracket = "&gt;";
                    break;
                case BracketType.ApostropheXml:
                    _openBracket = "&apos;";
                    _closeBracket = "&apos";
                    break;
                case BracketType.QuotesXml:
                    _openBracket = "&quot;";
                    _closeBracket = "&quot;";
                    break;
                default:
                    _openBracket = "";
                    _closeBracket = "";
                    break;
            }
        }

        public void AddToken(string tokenStr)
        {
            if (!string.IsNullOrWhiteSpace(tokenStr))
            {
                AddToken(new SimpleExpressionToken(tokenStr));
            }
        }

        public void AddToken(string tokenStr, BracketType bracketType)
        {
            AddToken(new CompositeExpressionToken(tokenStr, bracketType));
        }

        public void AddToken<T>(T token) where T : ExpressionToken
        {
            _tokens.Add(token);
        }

        public int TokenCount()
        {
            return _tokens.Count(t => t != null);
        }

        public override void Write(StringBuilder sb)
        {
            var flag = (_bracketType != BracketType.None);
            if (flag)
            {
                sb.Append(_openBracket);
            }

            var first = true;
            foreach (var token in _tokens.Where(t => t != null))
            {
                if (first)
                    first = false;
                else if (_addCommas)
                    sb.Append(", ");

                token.Write(sb);
            }

            if (flag)
            {
                sb.Append(_closeBracket);
            }
        }
    }
}
