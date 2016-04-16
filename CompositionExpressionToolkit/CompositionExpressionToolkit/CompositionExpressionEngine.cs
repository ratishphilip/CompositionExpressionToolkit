using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Windows.UI.Composition;

namespace CompositionExpressionToolkit
{
    /// <summary>
    /// Converts an Expression to a string that can be used as an input
    /// for ExpressionAnimation and KeyFrameAnimation
    /// </summary>
    public abstract class CompositionExpressionEngine
    {
        #region Fields

        private static readonly Dictionary<ExpressionType, string> BinaryExpressionStrings;
        private static readonly Dictionary<Type, MethodInfo> VisitMethods;

        private static bool _noQuotesForConstant;
        private static bool _firstBinaryExpression = true;
        private static Dictionary<string, object> _parameters;
         
        #endregion

        #region Construction / Initialization

        /// <summary>
        /// Static constructor
        /// </summary>
        static CompositionExpressionEngine()
        {
            BinaryExpressionStrings = new Dictionary<ExpressionType, string>()
            {
                { ExpressionType.Add, "+" },
                { ExpressionType.AddChecked, "+" },
                { ExpressionType.And, "&" },
                { ExpressionType.AndAlso, "&&" },
                { ExpressionType.Coalesce, "??" },
                { ExpressionType.Divide, "/" },
                { ExpressionType.Equal, "==" },
                { ExpressionType.ExclusiveOr, "^" },
                { ExpressionType.GreaterThan, ">" },
                { ExpressionType.GreaterThanOrEqual, ">=" },
                { ExpressionType.LeftShift, "<<" },
                { ExpressionType.LessThan, "<" },
                { ExpressionType.LessThanOrEqual, "<=" },
                { ExpressionType.Modulo, "%" },
                { ExpressionType.Multiply, "*" },
                { ExpressionType.MultiplyChecked, "*" },
                { ExpressionType.NotEqual, "!=" },
                { ExpressionType.Or, "|" },
                { ExpressionType.OrElse, "||" },
                { ExpressionType.Power, "^" },
                { ExpressionType.RightShift, ">>" },
                { ExpressionType.Subtract, "-" },
                { ExpressionType.SubtractChecked, "-" },
                { ExpressionType.Assign, "=" },
                { ExpressionType.AddAssign, "+=" },
                { ExpressionType.AndAssign, "&=" },
                { ExpressionType.DivideAssign, "/=" },
                { ExpressionType.ExclusiveOrAssign, "^=" },
                { ExpressionType.LeftShiftAssign, "<<=" },
                { ExpressionType.ModuloAssign, "%=" },
                { ExpressionType.MultiplyAssign, "*=" },
                { ExpressionType.OrAssign, "|=" },
                { ExpressionType.PowerAssign, "^=" },
                { ExpressionType.RightShiftAssign, ">>=" },
                { ExpressionType.SubtractAssign, "-=" },
                { ExpressionType.AddAssignChecked, "+=" },
                { ExpressionType.MultiplyAssignChecked, "*=" },
                { ExpressionType.SubtractAssignChecked, "-=" },
            };

            // Get all the types which derive from Expression or MemberBinding
            var expressionTypes = typeof(Expression)
                                        .GetTypeInfo()
                                        .Assembly
                                        .GetTypes()
                                        .Where(t => t.IsSubclassOf(typeof(Expression))
                                               || t.IsSubclassOf(typeof(MemberBinding)));

            // Get all the private static Visit methods defined in the CompositionExpressionEngine
            var visitMethods = typeof(CompositionExpressionEngine)
                                    .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                                    .Where(m => m.Name == "Visit");

            // Get the list of Visit methods whose first parameter matches one of the types in expressionTypes
            VisitMethods = expressionTypes.Join(visitMethods,
                                                  t => t,                                  // Selector for Expression Types
                                                  m => m.GetParameters()[0].ParameterType, // Selector for Expression Engine Methods
                                                  (t, m) => new { Type = t, Method = m })  // Result Selector
                                            .ToDictionary(t => t.Type, t => t.Method);

        }

        #endregion

        #region APIs

        /// <summary>
        /// Converts the given Expression to a string that can be used as an input
        /// for ExpressionAnimation and KeyFrameAnimation
        /// </summary>
        /// <typeparam name="T">Type of the Expression</typeparam>
        /// <param name="expression">Expression</param>
        /// <returns>CompositionExpression</returns>
        public static CompositionExpression CreateCompositionExpression<T>(Expression<Func<CompositionExpressionContext, T>> expression)
        {
            // Reset flags
            _noQuotesForConstant = false;
            _firstBinaryExpression = true;
            _parameters = new Dictionary<string, object>();

            var compositionExpr = new CompositionExpression();
            // Visit the Expression Tree and convert it to string
            var expr = Visit(expression).ToString();
            compositionExpr.Expression = expr;
            // Obtain the parameters involved in the expression
            compositionExpr.Parameters = new Dictionary<string, object>(_parameters);

            return compositionExpr;
        }

        #endregion

        #region Visit methods

        /// <summary>
        /// Visits an Expression
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(Expression expr)
        {
            if (expr == null)
            {
                return new SimpleExpressionToken("null");
            }

            var baseType = expr.GetType();
            while (!baseType.IsPublic())
            {
                baseType = baseType.BaseType();
            }

            // Get the Visit method whose first parameter best matches the type of baseType
            MethodInfo methodInfo;
            if (VisitMethods.TryGetValue(baseType, out methodInfo) ||
                ((baseType.BaseType() != null) && VisitMethods.TryGetValue(baseType.BaseType(), out methodInfo)))
            {
                // Once a matching Visit method is found, Invoke it!
                return (ExpressionToken)methodInfo.Invoke(null, new object[] { expr });
            }

            return null;
        }

        /// <summary>
        /// Visits a BinaryExpression
        /// </summary>
        /// <param name="expr">BinaryExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(BinaryExpression expr)
        {
            var leftToken = Visit(expr.Left);
            var rightToken = Visit(expr.Right);
            if (expr.NodeType == ExpressionType.ArrayIndex)
            {
                var arrToken = new CompositeExpressionToken();
                arrToken.AddToken(leftToken);
                arrToken.AddToken(new CompositeExpressionToken(rightToken, BracketType.Square));
                return arrToken;
            }

            string symbol;
            if (!BinaryExpressionStrings.TryGetValue(expr.NodeType, out symbol))
                return new SimpleExpressionToken("");

            // This check is done to avoid wrapping the final ExpressionToken in Round Brackets
            var bracketType = BracketType.Round;
            if (_firstBinaryExpression)
            {
                bracketType = BracketType.None;
                _firstBinaryExpression = false;
            }

            var token = new CompositeExpressionToken(bracketType);
            token.AddToken(leftToken);
            token.AddToken(" " + symbol + " ");
            token.AddToken(rightToken);

            return token;
        }

        /// <summary>
        /// Visits a ConditionalExpression
        /// </summary>
        /// <param name="expr">ConditionalExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(ConditionalExpression expr)
        {
            var token = new CompositeExpressionToken();
            token.AddToken(Visit(expr.Test));
            token.AddToken(" ? ");
            token.AddToken(Visit(expr.IfTrue));
            token.AddToken(" : ");
            token.AddToken(Visit(expr.IfFalse));
            return token;
        }

        /// <summary>
        /// Visits a ConstantExpression
        /// </summary>
        /// <param name="expr">ConstantExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(ConstantExpression expr)
        {
            if (expr.Value == null)
                return new SimpleExpressionToken("null");

            var str = expr.Value as string;
            if (str != null)
                return new CompositeExpressionToken(str, _noQuotesForConstant ? BracketType.None : BracketType.Quotes);

            return new SimpleExpressionToken(expr.Value.ToString());
        }

        /// <summary>
        /// Visits a InvocationExpression
        /// </summary>
        /// <param name="expr">InvocationExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(InvocationExpression expr)
        {
            var token = new CompositeExpressionToken();
            token.AddToken("Invoke");
            // Visit each of the arguments
            token.AddToken(new CompositeExpressionToken(expr.Arguments.Select(Visit), BracketType.Round, true));
            return token;
        }

        /// <summary>
        /// Visits a LambdaExpression
        /// </summary>
        /// <param name="expr">LambdaExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(LambdaExpression expr)
        {
            var token = new CompositeExpressionToken();

            // No need to print the parameter of type ExpressionContext
            if (expr.Parameters[0].Type != typeof(CompositionExpressionContext))
            {
                // Parameter(s)
                var paramStr = string.Join(", ", expr.Parameters.Select(p => CleanIdentifier(p.Name)).ToArray());
                var bracketType = (expr.Parameters.Count == 1) ? BracketType.None : BracketType.Round;
                token.AddToken(new CompositeExpressionToken(paramStr, bracketType));

                // Arrow
                token.AddToken(" => ");
            }

            // Expression Body
            var bodyToken = Visit(expr.Body);
            if (bodyToken != null)
            {
                token.AddToken(bodyToken);
            }

            return token;
        }

        /// <summary>
        /// Visits a MemberExpression
        /// </summary>
        /// <param name="expr">MemberExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(MemberExpression expr)
        {
            // NOTE: This check is for ScrollViewerManipulationPropertySet. It has a property called
            // Properties which is of type CompositionPropertySet. So while converting to string, 'Properties'
            // need not be printed
            if ((expr.Member is PropertyInfo) && (expr.Type == typeof (CompositionPropertySet) && (expr.Member.Name == "Properties"))
                && (expr.Expression is MemberExpression) && (expr.Expression.Type == typeof (CompositionPropertySet)))
            {
                return Visit(expr.Expression);
            }

            // Check if the parent of this expression has a name which starts with CS$<
            var memberExpr = expr.Expression as MemberExpression;
            if ((memberExpr != null) && (memberExpr.Member.Name.StartsWith("CS$<", StringComparison.Ordinal)))
            {
                // Add to the parameters dictionary
                if (!_parameters.ContainsKey(expr.Member.Name) && expr.Member is FieldInfo &&
                    memberExpr.Expression is ConstantExpression)
                {
                    var localFieldValue =
                        ((FieldInfo) memberExpr.Member).GetValue(((ConstantExpression) memberExpr.Expression).Value);
                    _parameters.Add(expr.Member.Name,
                        ((FieldInfo) expr.Member).GetValue(localFieldValue));
                }

                return new SimpleExpressionToken(expr.Member.Name);
            }

            var token = new CompositeExpressionToken();
            var constExpr = expr.Expression as ConstantExpression;
            if (constExpr?.Value != null && (constExpr.Value.GetType().IsNested && constExpr.Value.GetType().Name.StartsWith("<", StringComparison.Ordinal)))
            {
                // Add to the parameters dictionary
                if (!_parameters.ContainsKey(expr.Member.Name) && expr.Member is FieldInfo)
                    _parameters.Add(expr.Member.Name, ((FieldInfo)expr.Member).GetValue(constExpr.Value));

                return new SimpleExpressionToken(expr.Member.Name);
            }

            if (expr.Expression != null)
            {
                token.AddToken(Visit(expr.Expression));
            }
            else
            {
                token.AddToken(expr.Member.DeclaringType.Name);
            }
            token.AddToken("." + CleanIdentifier(expr.Member.Name));

            return token;
        }

        /// <summary>
        /// Visits a MethodCallExpression
        /// </summary>
        /// <param name="expr">MethodCallExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(MethodCallExpression expr)
        {
            var isExtensionMethod = expr.Method.IsDefined(typeof (ExtensionAttribute));
            var methodName = expr.Method.Name;

            var token = new CompositeExpressionToken();
            // If this is an extension method
            if (isExtensionMethod)
            {
                if (expr.Method.DeclaringType == typeof(CompositionPropertySetExtensions))
                {
                    token.AddToken(Visit(expr.Arguments[0]));
                    token.AddToken(".");
                    _noQuotesForConstant = true;
                    token.AddToken(Visit(expr.Arguments[1]));
                    _noQuotesForConstant = false;
                }
                else
                {
                    token.AddToken(Visit(expr.Arguments[0]));
                    token.AddToken("." + methodName);
                    token.AddToken(new CompositeExpressionToken(expr.Arguments.Skip(1).Select(Visit), BracketType.Round,
                        true));
                }
            }
            else
            {
                var showDot = true;
                if (expr.Object == null)
                {
                    token.AddToken(expr.Method.DeclaringType.FormattedName());
                }
                // No need to print the object name if the object is of type CompositionExpressionContext
                else if (expr.Object.Type == typeof (CompositionExpressionContext))
                {
                    showDot = false;
                }
                else
                {
                    token.AddToken(Visit(expr.Object));
                }

                if (expr.Method.IsSpecialName &&
                    (expr.Method.DeclaringType.GetProperties()
                        .FirstOrDefault(p => p.GetAccessors().Contains(expr.Method)) != null))
                {
                    token.AddToken(new CompositeExpressionToken(expr.Arguments.Select(Visit), BracketType.Square, true));
                }
                else
                {
                    token.AddToken((showDot ? "." : "") + expr.Method.Name);
                    token.AddToken(new CompositeExpressionToken(expr.Arguments.Select(Visit), BracketType.Round, true));
                }
            }

            return token;
        }

        /// <summary>
        /// Visits a ParameterExpression
        /// </summary>
        /// <param name="expr">ParameterExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(ParameterExpression exp)
        {
            var name = exp.Name ?? "<param>";
            return new SimpleExpressionToken(CleanIdentifier(name));
        }

        /// <summary>
        /// Visits a TypeBinaryExpression
        /// </summary>
        /// <param name="expr">TypeBinaryExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(TypeBinaryExpression exp)
        {
            var token = new CompositeExpressionToken(BracketType.Round);
            token.AddToken(Visit(exp.Expression));
            token.AddToken(" is ");
            token.AddToken(exp.TypeOperand.Name);
            return token;
        }

        /// <summary>
        /// Visits a UnaryExpression
        /// </summary>
        /// <param name="expr">UnaryExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(UnaryExpression expr)
        {
            if (expr.NodeType == ExpressionType.Quote)
            {
                return Visit(expr.Operand);
            }

            var token = new CompositeExpressionToken();
            var suffix = string.Empty;

            switch (expr.NodeType)
            {
                case ExpressionType.Convert:
                    if (expr.Operand.Type.IsSubclassOf(expr.Type))
                        return Visit(expr.Operand);
                    token.AddToken(new CompositeExpressionToken(expr.Type.Name, BracketType.Round));
                    break;
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    token.AddToken("-");
                    break;
                case ExpressionType.UnaryPlus:
                    token.AddToken("+");
                    break;
                case ExpressionType.Not:
                    token.AddToken("!");
                    break;
                case ExpressionType.PreIncrementAssign:
                    token.AddToken("++");
                    break;
                case ExpressionType.PreDecrementAssign:
                    token.AddToken("--");
                    break;
                case ExpressionType.TypeAs:
                    token.AddToken(Visit(expr.Operand));
                    token.AddToken(" as ");
                    token.AddToken(expr.Type.Name);
                    return token;
                case ExpressionType.OnesComplement:
                    token.AddToken("~");
                    break;
                case ExpressionType.PostIncrementAssign:
                    suffix = "++";
                    break;
                case ExpressionType.PostDecrementAssign:
                    suffix = "--";
                    break;
                default:
                    token.AddToken(expr.NodeType.ToString());
                    break;
            }

            // Visit the operand
            var operandToken = Visit(expr.Operand);
            var compToken = operandToken as CompositeExpressionToken;
            // If there are more the one tokens in the CompositeExpressionToken
            // then wrap them with Round brackets
            if (compToken?.TokenCount() > 1)
            {
                compToken.SetBrackets(BracketType.Round);
            }

            token.AddToken(operandToken);

            // Is suffix non-empty?
            if (!string.IsNullOrWhiteSpace(suffix))
            {
                token.AddToken(suffix);
            }

            return token;
        }

        /// <summary>
        /// Visits a MemberAssignment
        /// </summary>
        /// <param name="mb">MemberAssignment</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(MemberAssignment mb)
        {
            var token = new CompositeExpressionToken();
            token.AddToken(CleanIdentifier(mb.Member.Name) + " = ");
            token.AddToken(Visit(mb.Expression));
            return token;
        }

        /// <summary>
        /// Visits a ListInitExpression
        /// </summary>
        /// <param name="expr">ListInitExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(ListInitExpression expr)
        {
            // Not supported right now
            return null;
        }

        /// <summary>
        /// Visits a MemberInitExpression
        /// </summary>
        /// <param name="expr">MemberInitExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(MemberInitExpression expr)
        {
            // Not supported right now
            return null;
        }

        /// <summary>
        /// Visits a MemberListBinding
        /// </summary>
        /// <param name="mb">MemberListBinding</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(MemberListBinding mb)
        {
            // Not supported right now
            return null;
        }

        /// <summary>
        /// Visits a MemberMemberBinding
        /// </summary>
        /// <param name="mb">MemberMemberBinding</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(MemberMemberBinding mb)
        {
            // Not supported right now
            return null;
        }

        /// <summary>
        /// Visits a NewArrayExpression
        /// </summary>
        /// <param name="expr">NewArrayExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(NewArrayExpression expr)
        {
            // Not supported right now
            return null;
        }

        /// <summary>
        /// Visits a NewExpression
        /// </summary>
        /// <param name="expr">NewExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(NewExpression expr)
        {
            // Not supported right now
            return null;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Converts identifier name to a more readable format 
        /// </summary>
        /// <param name="name">Identifier Name</param>
        /// <returns>Formatted Name</returns>
        private static string CleanIdentifier(string name)
        {
            if (name == null)
                return null;
            if (name.StartsWith("<>h__TransparentIdentifier", StringComparison.Ordinal))
                return "temp_" + name.Substring(26);
            return name;
        }

        #endregion

    }
}
