using System;
using System.Linq;
using System.Linq.Expressions;
using Windows.Graphics.Effects;
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

        /// <summary>
        /// Creates an instance of CompositionEffectFactory.
        /// </summary>
        /// <param name="compositor">Compositor</param>
        /// <param name="graphicsEffect">The type of effect to create.</param>
        /// <param name="animatablePropertyExpressions">List of Expression each specifying 
        /// an animatable property</param>
        /// <returns>The created CompositionEffectFactory object.</returns>
        public static CompositionEffectFactory CreateEffectFactory(this Compositor compositor,
            IGraphicsEffect graphicsEffect, params Expression<Func<object>>[] animatablePropertyExpressions)
        {
            var animatableProperties = animatablePropertyExpressions.Select(CompositionExpressionEngine.ParseExpression).ToArray();

            return compositor.CreateEffectFactory(graphicsEffect, animatableProperties);
        }

        /// <summary>
        /// Creates an instance of KeyFrameAnimation&lt;T&gt;
        /// </summary>
        /// <typeparam name="T">Type of the encapsulated KeyFrameAnimation</typeparam>
        /// <param name="compositor">Compositor</param>
        /// <returns>KeyFrameAnimation&lt;T&gt;</returns>
        public static KeyFrameAnimation<T> CreateKeyFrameAnimation<T>(this Compositor compositor)
        {
            return new KeyFrameAnimation<T>(KeyFrameAnimationHelper.CreateAnimation<T>(compositor));
        }

        /// <summary>
        /// Creates a CompositionLambda expression for 'c =&gt; c.StartingValue' for the given type
        /// </summary>
        /// <typeparam name="T">Type of the CompositionLambda Expression</typeparam>
        /// <param name="compositor">Compositor</param>
        /// <returns>Expression&lt;CompositionLambda&lt;T&gt;&gt;</returns>
        public static Expression<CompositionLambda<T>> CreateStartingValueExpression<T>(this Compositor compositor)
        {
            Expression<CompositionLambda<T>> expression = c => c.StartingValue;
            return expression;
        }

        /// <summary>
        /// Creates a CompositionLambda expression for 'c =&gt; c.FinalValue' for the given type
        /// </summary>
        /// <typeparam name="T">Type of the CompositionLambda Expression</typeparam>
        /// <param name="compositor">Compositor</param>
        /// <returns>Expression&lt;CompositionLambda&lt;T&gt;&gt;</returns>
        public static Expression<CompositionLambda<T>> CreateFinalValueExpression<T>(this Compositor compositor)
        {
            Expression<CompositionLambda<T>> expression = c => c.FinalValue;
            return expression;
        }

    }
}
