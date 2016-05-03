using System;
using System.Linq.Expressions;
using Windows.UI.Composition;

namespace CompositionExpressionToolkit
{
    /// <summary>
    /// Extension methods for the CompositionObject class
    /// </summary>
    public static class CompositionObjectExtensions
    {
        /// <summary>
        /// Starts the given animation on the property specified by the given expression.
        /// The expression is converted to the appropriate property string by the
        /// CompositionExpressionEngine 
        /// </summary>
        /// <param name="compositionObject">CompositionObject</param>
        /// <param name="expression">Expression defining the property on which to start the animation</param>
        /// <param name="animation">The animation to execute on the specified property</param>
        public static void StartAnimation(this CompositionObject compositionObject,
            Expression<Func<object>> expression, CompositionAnimation animation)
        {
            compositionObject.StartAnimation(CompositionExpressionEngine.ParseExpression(expression), animation);
        }

        /// <summary>
        /// Stops the given animation on the property specified by the given expression.
        /// The expression is converted to the appropriate property string by the
        /// CompositionExpressionEngine 
        /// </summary>
        /// <param name="compositionObject">CompositionObject</param>
        /// <param name="expression">Expression defining the property on which to stop the animation</param>
        public static void StopAnimation(this CompositionObject compositionObject,
            Expression<Func<object>> expression)
        {
            compositionObject.StopAnimation(CompositionExpressionEngine.ParseExpression(expression));
        }

        /// <summary>
        /// This extension method is a dummy method added to specify to start/stop animation on the 
        /// 'Scale.XY' property of the CompositionObject. Though no such property exists in CompositionObject,
        /// it merely indicates that the animation has to be executed (or stopped) on both the 'Scale.X' and 'Scale.Y'
        /// properties of the CompositionObject.
        /// </summary>
        /// <param name="compositionObject">CompositionObject</param>
        /// <returns></returns>
        public static string ScaleXY(this CompositionObject compositionObject)
        {
            return "Scale.XY";
        }
    }
}
