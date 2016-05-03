﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Windows.UI.Composition;

namespace CompositionExpressionToolkit
{
    #region Delegates

    public delegate T CompositionLambda<T>(CompositionExpressionContext<T> ctx);

    #endregion

    /// <summary>
    /// Converts an Expression to a string that can be used as an input
    /// for ExpressionAnimation and KeyFrameAnimation
    /// </summary>
    public abstract class CompositionExpressionEngine
    {
        #region Fields

        private static readonly Dictionary<ExpressionType, string> BinaryExpressionStrings;
        private static readonly Dictionary<Type, MethodInfo> VisitMethods;
        private static readonly Dictionary<Type, MethodInfo> ParseVisitMethods;
        private static readonly Type[] Floatables;

        private static bool _noQuotesForConstant;
        private static bool _firstBinaryExpression;
        private static Dictionary<string, object> _parameters;
        private static bool _firstParseBinaryExpression;
        private static bool _noQuotesForParseConstant;

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

            Floatables = new[]{
                                  typeof(short),
                                  typeof(ushort),
                                  typeof(int),
                                  typeof(uint),
                                  typeof(long),
                                  typeof(ulong),
                                  typeof(char),
                                  typeof(double),
                                  typeof(bool),
                                  typeof(float),
                                  typeof(decimal)
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


            // Get all the private static ParseVisit methods defined in the CompositionExpressionEngine
            var parseVisitMethods = typeof(CompositionExpressionEngine)
                                    .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                                    .Where(m => m.Name == "ParseVisit");

            // Get the list of ParseVisit methods whose first parameter matches one of the types in expressionTypes
            ParseVisitMethods = expressionTypes.Join(parseVisitMethods,
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
        /// <returns>CompositionExpressionResult</returns>
        public static CompositionExpressionResult CreateCompositionExpression<T>(Expression<CompositionLambda<T>> expression)
        {
            // Reset flags
            _noQuotesForConstant = false;
            _firstBinaryExpression = false;
            _parameters = new Dictionary<string, object>();

            var compositionExpr = new CompositionExpressionResult();
            // Visit the Expression Tree and convert it to string
            var expr = Visit(expression).ToString();
            compositionExpr.Expression = expr;
            // Obtain the parameters involved in the expression
            compositionExpr.Parameters = new Dictionary<string, object>(_parameters);

            return compositionExpr;
        }

        public static string ParseExpression(Expression<Func<object>> expression)
        {
            // Reset flags
            _noQuotesForParseConstant = false;
            _firstParseBinaryExpression = false;

            return ParseVisit(expression).ToString();
        }

        #endregion

        #region Visit methods

        /// <summary>
        /// Visits an Expression
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(Expression expression)
        {
            if (expression == null)
            {
                return new SimpleExpressionToken("null");
            }

            var baseType = expression.GetType();
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
                return (ExpressionToken)methodInfo.Invoke(null, new object[] { expression });
            }

            return null;
        }

        /// <summary>
        /// Visits a BinaryExpression
        /// </summary>
        /// <param name="expression">BinaryExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(BinaryExpression expression)
        {
            // Check if it is the outermost BinaryExpression
            // If yes, then no need to add round brackets to 
            // the whole visited expression
            var noBrackets = _firstBinaryExpression;
            if (_firstBinaryExpression)
            {
                // Set it to false so that the internal BinaryExpression(s)
                // will have round brackets
                _firstBinaryExpression = false;
            }

            var leftToken = Visit(expression.Left);
            var rightToken = Visit(expression.Right);
            if (expression.NodeType == ExpressionType.ArrayIndex)
            {
                var arrToken = new CompositeExpressionToken();
                arrToken.AddToken(leftToken);
                arrToken.AddToken(new CompositeExpressionToken(rightToken, BracketType.Square));
                return arrToken;
            }

            string symbol;
            if (!BinaryExpressionStrings.TryGetValue(expression.NodeType, out symbol))
                return new SimpleExpressionToken("");

            // This check is done to avoid wrapping the final ExpressionToken in Round Brackets
            var bracketType = BracketType.Round;
            if (noBrackets)
            {
                bracketType = BracketType.None;
            }

            var token = new CompositeExpressionToken(bracketType);
            token.AddToken($"{leftToken} {symbol} {rightToken}");

            return token;
        }

        /// <summary>
        /// Visits a ConditionalExpression
        /// </summary>
        /// <param name="expression">ConditionalExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(ConditionalExpression expression)
        {
            var token = new CompositeExpressionToken();
            token.AddToken($"{Visit(expression.Test)} ? {Visit(expression.IfTrue)} : {Visit(expression.IfFalse)}");
            return token;
        }

        /// <summary>
        /// Visits a ConstantExpression
        /// </summary>
        /// <param name="expression">ConstantExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(ConstantExpression expression)
        {
            if (expression.Value == null)
                return new SimpleExpressionToken("null");

            var str = expression.Value as string;
            if (str != null)
                return new CompositeExpressionToken(str, _noQuotesForConstant ? BracketType.None : BracketType.Quotes);

            return new SimpleExpressionToken(expression.Value.ToString());
        }

        /// <summary>
        /// Visits a InvocationExpression
        /// </summary>
        /// <param name="expression">InvocationExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(InvocationExpression expression)
        {
            var token = new CompositeExpressionToken();
            token.AddToken("Invoke");
            // Visit each of the arguments
            token.AddToken(new CompositeExpressionToken(expression.Arguments.Select(Visit), BracketType.Round, true));
            return token;
        }

        /// <summary>
        /// Visits a LambdaExpression
        /// </summary>
        /// <param name="expression">LambdaExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(LambdaExpression expression)
        {
            var token = new CompositeExpressionToken();

            // ### Customized for Windows.UI.Composition ###
            // No need to print the parameter of type CompositionExpressionContext<T>
            if (!IsGenericCompositionExpressionContextType(expression.Parameters[0].Type))
            {
                // Parameter(s)
                var paramStr = string.Join(", ", expression.Parameters.Select(p => CleanIdentifier(p.Name)).ToArray());
                var bracketType = (expression.Parameters.Count == 1) ? BracketType.None : BracketType.Round;
                token.AddToken(new CompositeExpressionToken(paramStr, bracketType));

                // Arrow
                token.AddToken(" => ");
            }
            // If the parameter is of type CompositionExpressionContext<T> then it means 
            // that this is a CompositionLambda expression (i.e. First specific Visit). 
            // If the outermost Expression in the body of the CompositionLambda expression
            // is a BinaryExpression, then no need to add round brackets
            else if ((expression.Body as BinaryExpression) != null)
            {
                _firstBinaryExpression = true;
            }

            // Expression Body
            var bodyToken = Visit(expression.Body);
            if (bodyToken != null)
            {
                token.AddToken(bodyToken);
            }

            return token;
        }

        /// <summary>
        /// Visits a MemberExpression
        /// </summary>
        /// <param name="expression">MemberExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(MemberExpression expression)
        {
            // ### Customized for Windows.UI.Composition ###
            // Check if this expression is accessing the StartingValue or FinalValue
            // Property of CompositionExpressionContext<T>
            if (((expression.Member as PropertyInfo) != null) &&
                (expression.Expression != null) &&
                IsGenericCompositionExpressionContextType(expression.Expression.Type))
            {
                return new SimpleExpressionToken($"this.{expression.Member.Name}");
            }

            // This check is for CompositionPropertySet. It has a property called 
            // Properties which is of type CompositionPropertySet. So while converting to string, 'Properties' 
            // need not be printed 
            if (((expression.Member as PropertyInfo) != null) &&
                (expression.Type == typeof(CompositionPropertySet) && (expression.Member.Name == "Properties"))
                && (expression.Expression is MemberExpression) && (expression.Expression.Type == typeof(CompositionPropertySet)))
            {
                return Visit(expression.Expression);
            }

            // If the expression is of type CompositionPropertySet, then no need to 
            // visit this expression tree further. Just add this CompositionPropertySet
            // to the _parameters dictionary (if it doesn't already exist) and return
            // the name of the expression member
            if (expression.Type == typeof(CompositionPropertySet))
            {
                if (!_parameters.ContainsKey(expression.Member.Name) &&
                    expression.Expression is ConstantExpression)
                {
                    if ((expression.Member as FieldInfo) != null)
                    {
                        _parameters.Add(expression.Member.Name, ((FieldInfo)expression.Member).GetValue(((ConstantExpression)expression.Expression).Value));
                    }
                    else if ((expression.Member as PropertyInfo) != null)
                    {
                        _parameters.Add(expression.Member.Name, ((PropertyInfo)expression.Member).GetValue(((ConstantExpression)expression.Expression).Value));
                    }
                }

                return new SimpleExpressionToken(expression.Member.Name);
            }

            // Check if the parent of this expression has a name which starts with CS$<
            var parentMemberExpr = expression.Expression as MemberExpression;
            if ((parentMemberExpr != null) &&
                parentMemberExpr.Member.Name.StartsWith("CS$<", StringComparison.Ordinal))
            {
                // ### Customized for Windows.UI.Composition ###
                // Add to the parameters dictionary
                if (!_parameters.ContainsKey(expression.Member.Name)
                    && (parentMemberExpr.Expression as ConstantExpression) != null)
                {
                    var constantExpr = (ConstantExpression)parentMemberExpr.Expression;

                    if ((parentMemberExpr.Member as FieldInfo) != null)
                    {
                        var localFieldValue = ((FieldInfo)parentMemberExpr.Member).GetValue(constantExpr.Value);
                        if ((expression.Member as FieldInfo) != null)
                        {
                            _parameters.Add(expression.Member.Name, ((FieldInfo)expression.Member).GetValue(localFieldValue));
                        }
                        else if ((expression.Member as PropertyInfo) != null)
                        {
                            _parameters.Add(expression.Member.Name, ((PropertyInfo)expression.Member).GetValue(localFieldValue));
                        }
                    }
                    else if ((parentMemberExpr.Member as PropertyInfo) != null)
                    {
                        var localFieldValue = ((PropertyInfo)parentMemberExpr.Member).GetValue(constantExpr.Value);
                        if ((expression.Member as FieldInfo) != null)
                        {
                            _parameters.Add(expression.Member.Name, ((FieldInfo)expression.Member).GetValue(localFieldValue));
                        }
                        else if ((expression.Member as PropertyInfo) != null)
                        {
                            _parameters.Add(expression.Member.Name, ((PropertyInfo)expression.Member).GetValue(localFieldValue));
                        }
                    }
                }

                return new SimpleExpressionToken(expression.Member.Name);
            }

            var token = new CompositeExpressionToken();
            var constExpr = expression.Expression as ConstantExpression;
            if ((constExpr?.Value != null)
                && constExpr.Value.GetType().IsNested
                && constExpr.Value.GetType().Name.StartsWith("<", StringComparison.Ordinal))
            {
                // ### Customized for Windows.UI.Composition ###
                // Add to the parameters dictionary
                if (!_parameters.ContainsKey(expression.Member.Name))
                {
                    if ((expression.Member as FieldInfo) != null)
                    {
                        _parameters.Add(expression.Member.Name, ((FieldInfo)expression.Member).GetValue(constExpr.Value));
                    }
                    else if ((expression.Member as PropertyInfo) != null)
                    {
                        _parameters.Add(expression.Member.Name, ((PropertyInfo)expression.Member).GetValue(constExpr.Value));
                    }
                }

                return new SimpleExpressionToken(expression.Member.Name);
            }

            if (expression.Expression != null)
            {
                token.AddToken(Visit(expression.Expression));
            }
            else
            {
                token.AddToken(expression.Member.DeclaringType.Name);
            }

            token.AddToken($".{CleanIdentifier(expression.Member.Name)}");

            return token;
        }

        /// <summary>
        /// Visits a MethodCallExpression
        /// </summary>
        /// <param name="expression">MethodCallExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(MethodCallExpression expression)
        {
            var isExtensionMethod = expression.Method.IsDefined(typeof(ExtensionAttribute));
            var methodName = expression.Method.Name;

            var token = new CompositeExpressionToken();
            // If this is an extension method
            if (isExtensionMethod)
            {
                // ### Customized for Windows.UI.Composition ###
                // If the .Single() extension method is being called on a System.Double
                // value, no need to print it.
                if (expression.Method.DeclaringType == typeof(DoubleExtensions))
                {
                    token.AddToken(Visit(expression.Arguments[0]));
                }
                // If the extension method being called belongs to CompositionPropertySetExtensions
                // then no need to add the method name
                else if (expression.Method.DeclaringType == typeof(CompositionPropertySetExtensions))
                {
                    token.AddToken(Visit(expression.Arguments[0]));
                    token.AddToken(".");
                    _noQuotesForConstant = true;
                    token.AddToken(Visit(expression.Arguments[1]));
                    _noQuotesForConstant = false;
                }
                else
                {
                    token.AddToken(Visit(expression.Arguments[0]));
                    token.AddToken($".{methodName}");
                    token.AddToken(new CompositeExpressionToken(expression.Arguments.Skip(1).Select(Visit), BracketType.Round,
                        true));
                }
            }
            else
            {
                var showDot = true;
                if (expression.Object == null)
                {
                    token.AddToken(expression.Method.DeclaringType.FormattedName());
                }
                // ### Customized for Windows.UI.Composition ###
                // No need to print the object name if the object is of type CompositionExpressionContext<T>
                else if (IsGenericCompositionExpressionContextType(expression.Object.Type))
                {
                    showDot = false;
                }
                else
                {
                    token.AddToken(Visit(expression.Object));
                }

                if (expression.Method.IsSpecialName &&
                    (expression.Method.DeclaringType.GetProperties()
                        .FirstOrDefault(p => p.GetAccessors().Contains(expression.Method)) != null))
                {
                    token.AddToken(new CompositeExpressionToken(expression.Arguments.Select(Visit), BracketType.Square, true));
                }
                else
                {
                    token.AddToken((showDot ? "." : "") + expression.Method.Name);
                    token.AddToken(new CompositeExpressionToken(expression.Arguments.Select(Visit), BracketType.Round, true));
                }
            }

            return token;
        }

        /// <summary>
        /// Visits a ParameterExpression
        /// </summary>
        /// <param name="expression">ParameterExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(ParameterExpression expression)
        {
            var name = expression.Name ?? "<param>";
            return new SimpleExpressionToken(CleanIdentifier(name));
        }

        /// <summary>
        /// Visits a TypeBinaryExpression
        /// </summary>
        /// <param name="expression">TypeBinaryExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(TypeBinaryExpression expression)
        {
            var token = new CompositeExpressionToken(BracketType.Round);
            token.AddToken($"{Visit(expression.Expression)} is {expression.TypeOperand.Name}");
            return token;
        }

        /// <summary>
        /// Visits a UnaryExpression
        /// </summary>
        /// <param name="expression">UnaryExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(UnaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.Quote)
            {
                return Visit(expression.Operand);
            }

            var token = new CompositeExpressionToken();
            var suffix = string.Empty;
            var bracketsRequired = true;

            switch (expression.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    if (expression.Operand.Type.IsSubclassOf(expression.Type))
                        return Visit(expression.Operand);
                    // ### Customized for Windows.UI.Composition ###
                    // Don't add a cast for any of the types in Floatables
                    if (Floatables.Contains(expression.Type))
                    {
                        bracketsRequired = false;
                    }
                    else
                    {
                        token.AddToken(new CompositeExpressionToken(expression.Type.Name, BracketType.Round));
                    }
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
                    token.AddToken(Visit(expression.Operand));
                    token.AddToken(" as ");
                    token.AddToken(expression.Type.Name);
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
                    token.AddToken(expression.NodeType.ToString());
                    break;
            }

            // Visit the operand
            var operandToken = Visit(expression.Operand);
            var compToken = operandToken as CompositeExpressionToken;
            // If there are more the one tokens in the CompositeExpressionToken
            // then wrap them with Round brackets
            if (bracketsRequired && (compToken?.TokenCount() > 1))
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
            token.AddToken($"{CleanIdentifier(mb.Member.Name)} = {Visit(mb.Expression)}");
            return token;
        }

        /// <summary>
        /// Visits a ListInitExpression
        /// </summary>
        /// <param name="expression">ListInitExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(ListInitExpression expression)
        {
            // Not supported right now
            return null;
        }

        /// <summary>
        /// Visits a MemberInitExpression
        /// </summary>
        /// <param name="expression">MemberInitExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(MemberInitExpression expression)
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
        /// <param name="expression">NewArrayExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(NewArrayExpression expression)
        {
            // Not supported right now
            return null;
        }

        /// <summary>
        /// Visits a NewExpression
        /// </summary>
        /// <param name="expression">NewExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken Visit(NewExpression expression)
        {
            // Not supported right now
            return null;
        }

        #endregion

        #region ParseVisit Methods

        /// <summary>
        /// Visits an Expression
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken ParseVisit(Expression expression)
        {
            if (expression == null)
            {
                return new SimpleExpressionToken("null");
            }

            var baseType = expression.GetType();
            while (!baseType.IsPublic())
            {
                baseType = baseType.BaseType();
            }

            // Get the Visit method whose first parameter best matches the type of baseType
            MethodInfo methodInfo;
            if (ParseVisitMethods.TryGetValue(baseType, out methodInfo) ||
                ((baseType.BaseType() != null) && ParseVisitMethods.TryGetValue(baseType.BaseType(), out methodInfo)))
            {
                // Once a matching Visit method is found, Invoke it!
                return (ExpressionToken)methodInfo.Invoke(null, new object[] { expression });
            }

            return null;
        }

        /// <summary>
        /// Visits a BinaryExpression
        /// </summary>
        /// <param name="expression">BinaryExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken ParseVisit(BinaryExpression expression)
        {
            // Check if it is the outermost BinaryExpression
            // If yes, then no need to add round brackets to 
            // the whole visited expression
            var noBrackets = _firstParseBinaryExpression;
            if (_firstParseBinaryExpression)
            {
                // Set it to false so that the internal BinaryExpression(s)
                // will have round brackets
                _firstParseBinaryExpression = false;
            }

            var leftToken = ParseVisit(expression.Left);
            var rightToken = ParseVisit(expression.Right);
            if (expression.NodeType == ExpressionType.ArrayIndex)
            {
                var arrToken = new CompositeExpressionToken();
                arrToken.AddToken(leftToken);
                arrToken.AddToken(new CompositeExpressionToken(rightToken, BracketType.Square));
                return arrToken;
            }

            string symbol;
            if (!BinaryExpressionStrings.TryGetValue(expression.NodeType, out symbol))
                return new SimpleExpressionToken("");

            // This check is done to avoid wrapping the final ExpressionToken in Round Brackets
            var bracketType = noBrackets ? BracketType.None : BracketType.Round;

            var token = new CompositeExpressionToken(bracketType);
            token.AddToken($"{leftToken} {symbol} {rightToken}");

            return token;
        }

        /// <summary>
        /// Visits a ConstantExpression
        /// </summary>
        /// <param name="expression">ConstantExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken ParseVisit(ConstantExpression expression)
        {
            if (expression.Value == null)
                return new SimpleExpressionToken("null");

            var str = expression.Value as string;
            if (str != null)
                return new CompositeExpressionToken(str, _noQuotesForParseConstant ? BracketType.None : BracketType.Quotes);

            return new SimpleExpressionToken(expression.Value.ToString());
        }

        /// <summary>
        /// Visits a LambdaExpression
        /// </summary>
        /// <param name="expression">LambdaExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken ParseVisit(LambdaExpression expression)
        {
            var token = new CompositeExpressionToken();

            if ((expression.Body as BinaryExpression) != null)
            {
                _firstParseBinaryExpression = true;
            }

            // Expression Body
            var bodyToken = ParseVisit(expression.Body);
            if (bodyToken != null)
            {
                token.AddToken(bodyToken);
            }

            return token;
        }

        /// <summary>
        /// Visits a MemberExpression
        /// </summary>
        /// <param name="expression">MemberExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken ParseVisit(MemberExpression expression)
        {
            // ### Customized for Windows.UI.Composition ###
            // Check if this expression is accessing the StartingValue or FinalValue
            // Property of CompositionExpressionContext<T>
            if (((expression.Member as PropertyInfo) != null) &&
                (expression.Expression != null) &&
                IsGenericCompositionExpressionContextType(expression.Expression.Type))
            {
                return new SimpleExpressionToken($"this.{expression.Member.Name}");
            }

            // This check is for CompositionPropertySet. It has a property called 
            // Properties which is of type CompositionPropertySet. So while converting to string, 'Properties' 
            // need not be printed 
            if (((expression.Member as PropertyInfo) != null) &&
                (expression.Type == typeof(CompositionPropertySet) && (expression.Member.Name == "Properties"))
                && (expression.Expression is MemberExpression) && (expression.Expression.Type == typeof(CompositionPropertySet)))
            {
                return ParseVisit(expression.Expression);
            }

            // If the expression type is a subclass of CompositionObject, no need to proceed further
            // because the ParseExpression is called on an object which is a subclass of CompositionObject
            // and we just need the property access string.
            if (expression.Type.IsSubclassOf(typeof(CompositionObject)))
                return null;

            var token = new CompositeExpressionToken();
            ExpressionToken parent = null;
            parent = expression.Expression != null ?
                        ParseVisit(expression.Expression) :
                        new SimpleExpressionToken(expression.Member.DeclaringType.Name);

            if (parent != null)
            {
                token.AddToken(parent);
                token.AddToken($".{CleanIdentifier(expression.Member.Name)}");
            }
            else
            {
                token.AddToken(CleanIdentifier(expression.Member.Name));
            }

            return token;
        }

        /// <summary>
        /// Visits a MethodCallExpression
        /// </summary>
        /// <param name="expression">MethodCallExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken ParseVisit(MethodCallExpression expression)
        {
            var isExtensionMethod = expression.Method.IsDefined(typeof(ExtensionAttribute));
            var methodName = expression.Method.Name;

            var token = new CompositeExpressionToken();
            // If this is an extension method
            if (isExtensionMethod)
            {
                // ### Customized for Windows.UI.Composition ###
                // If the .Single() extension method is being called on a System.Double
                // value, no need to print it.
                if (expression.Method.DeclaringType == typeof(DoubleExtensions))
                {
                    token.AddToken(ParseVisit(expression.Arguments[0]));
                }
                // If the extension method being called belongs to CompositionPropertySetExtensions
                // then no need to add the method name
                else if (expression.Method.DeclaringType == typeof(CompositionPropertySetExtensions))
                {
                    var parent = ParseVisit(expression.Arguments[0]);
                    if (parent != null)
                    {
                        token.AddToken(parent);
                        token.AddToken(".");
                        _noQuotesForParseConstant = true;
                        token.AddToken(ParseVisit(expression.Arguments[1]));
                        _noQuotesForParseConstant = false;
                    }
                }
                else
                {
                    var parent = ParseVisit(expression.Arguments[0]);
                    if (parent != null)
                    {
                        token.AddToken(parent);
                        token.AddToken($".{methodName}");
                    }
                    else
                    {
                        // ### Customized for Windows.UI.Composition ###
                        // Special Case: If the extension method name is ScaleXY,
                        // then 'Scale.XY' must be the string returned.
                        if (methodName == "ScaleXY")
                        {
                            methodName = "Scale.XY";
                        }

                        token.AddToken(methodName);
                    }
                }
            }
            else
            {
                var showDot = true;
                if (expression.Object == null)
                {
                    token.AddToken(expression.Method.DeclaringType.FormattedName());
                }
                // ### Customized for Windows.UI.Composition ###
                // No need to print the object name if the object derives from CompositionObject
                else if (expression.Type.IsSubclassOf(typeof(CompositionObject)))
                {
                    showDot = false;
                }
                else
                {
                    token.AddToken(ParseVisit(expression.Object));
                }

                // Is it an array index based access?
                if (expression.Method.IsSpecialName &&
                    (expression.Method.DeclaringType.GetProperties()
                        .FirstOrDefault(p => p.GetAccessors().Contains(expression.Method)) != null))
                {
                    token.AddToken(new CompositeExpressionToken(expression.Arguments.Select(ParseVisit), BracketType.Square, true));
                }
                else
                {
                    token.AddToken(showDot ? $".{expression.Method.Name}" : expression.Method.Name);
                }
            }

            return token;
        }

        /// <summary>
        /// Visits a ParameterExpression
        /// </summary>
        /// <param name="expression">ParameterExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken ParseVisit(ParameterExpression expression)
        {
            var name = expression.Name ?? "<param>";
            return new SimpleExpressionToken(CleanIdentifier(name));
        }

        /// <summary>
        /// Visits a UnaryExpression
        /// </summary>
        /// <param name="expression">UnaryExpression</param>
        /// <returns>ExpressionToken</returns>
        private static ExpressionToken ParseVisit(UnaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.Quote)
            {
                return ParseVisit(expression.Operand);
            }

            var token = new CompositeExpressionToken();

            switch (expression.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.TypeAs:
                    token.AddToken(ParseVisit(expression.Operand));
                    break;
            }

            return token;
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

        /// <summary>
        /// Checks if the given type of of type CompositionExpressionContext&lt;T&gt;
        /// </summary>
        /// <param name="inputType">Type to check</param>
        /// <returns>True of type matches otherwise false</returns>
        private static bool IsGenericCompositionExpressionContextType(Type inputType)
        {
            if ((inputType == null) ||
                (!inputType.IsGenericType()) ||
                (!inputType.GenericTypeArguments.Any()))
                return false;

            var paramType = inputType.GenericTypeArguments[0];
            return (inputType == typeof(CompositionExpressionContext<>).MakeGenericType(paramType));
        }

        #endregion
    }
}
