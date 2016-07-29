using System;
using System.Linq;
using System.Linq.Expressions;
using Windows.Foundation;
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

        /// <summary>
        /// This extension method creates a scoped batch and handles the completed event
        /// the subscribing and unsubscribing process internally.
        /// 
        /// Example usage:
        /// _compositor.CreateScopedBatch(CompositionBatchTypes.Animation,
        ///        () => // Action
        ///        {
        ///            transitionVisual.StartAnimation("Scale.XY", _scaleUpAnimation);
        ///        },
        ///        () => // Post Action
        ///        {
        ///            BackBtn.IsEnabled = true;
        ///        });
        /// 
        /// </summary>
        /// <param name="compositor">Compositor</param>
        /// <param name="batchType">Composition Batch Type</param>
        /// <param name="action">Action to perform within the scoped batch</param>
        /// <param name="postAction">Action to perform once the batch completes</param>
        public static void CreateScopedBatch(this Compositor compositor, CompositionBatchTypes batchType, Action action, Action postAction = null)
        {
            if (action == null)
                throw new ArgumentException("Cannot create a scoped batch on an action with null value!", nameof(action));

            // Create ScopedBatch
            var batch = compositor.CreateScopedBatch(batchType);

            // Is there any action to be executed when the batch completes?
            if (postAction != null)
            {
                // Handler for the Completed Event
                TypedEventHandler<object, CompositionBatchCompletedEventArgs> handler = null;
                handler = (s, ea) =>
                {
                    // Unsubscribe the handler from the Completed Event
                    batch.Completed -= handler;
                    // Invoke the post action
                    postAction();
                };

                // Subscribe to the Completed event
                batch.Completed += handler;
            }

            // Invoke the action
            action();

            // End Batch
            batch.End();
        }
    }
}
