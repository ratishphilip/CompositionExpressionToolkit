using System.Linq.Expressions;
using Windows.UI.Composition;

namespace CompositionExpressionToolkit
{
    /// <summary>
    /// Extension methods for Windows.UI.Composition.Compositor
    /// </summary>
    public static class CompositorExtensions
    {
        /// <summary>
        /// Creates an ExpressionAnimation based on the given CompositionLambda Expression
        /// </summary>
        /// <typeparam name="T">Type of the CompositionLambda Expression</typeparam>
        /// <param name="compositor">Compositor</param>
        /// <param name="expression">CompositionLambda Expression</param>
        /// <returns>ExpressionAnimation</returns>
        public static ExpressionAnimation CreateExpressionAnimation<T>(this Compositor compositor,
            Expression<CompositionLambda<T>> expression)
        {
            var result = CompositionExpressionEngine.CreateCompositionExpression(expression);
            var animation = compositor.CreateExpressionAnimation();
            animation.Expression = result.Expression;
            animation.SetParameters(result.Parameters);

            return animation;
        }
    }
}
