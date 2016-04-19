using System;
using Windows.Foundation;
using Windows.UI.Composition;

namespace CompositionExpressionToolkit
{
    /// <summary>
    /// Helper class to create scoped batch
    /// </summary>
    public static class ScopedBatchHelper
    {
        /// <summary>
        /// This class creates a scoped batch and handles the completed event
        /// the subscribing and unsubscribing process internally.
        /// 
        /// Example usage:
        /// ScopedBatchHelper.CreateScopedBatch(_compositor, CompositionBatchTypes.Animation,
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
        public static void CreateScopedBatch(Compositor compositor, CompositionBatchTypes batchType, Action action, Action postAction = null)
        {
            if (action == null)
                throw new ArgumentException("Cannot create a scoped batch on an action with null value!", nameof(action));

            // Create ScopedBatch
            var batch = compositor.CreateScopedBatch(batchType);

            // Invoke the action
            action();

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

            // End Batch
            batch.End();
        }
    }
}
