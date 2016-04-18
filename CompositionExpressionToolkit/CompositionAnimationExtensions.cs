using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using Windows.Foundation;
using Windows.UI.Composition;

namespace CompositionExpressionToolkit
{
    /// <summary>
    /// Extension methods for Animations deriving from CompositionAnimation
    /// </summary>
    public static class CompositionAnimationExtensions
    {
        #region Fields

        private static readonly Dictionary<Type, MethodInfo> SetMethods;
        private static readonly Type[] Floatables;

        #endregion

        #region Static Constructor

        static CompositionAnimationExtensions()
        {
            SetMethods = typeof(CompositionAnimation)
                               .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                               .Where(m => m.Name.StartsWith("Set") && m.Name.EndsWith("Parameter"))
                               .ToDictionary(m => m.GetParameters()[1].ParameterType,
                                             m => m);

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
                                  typeof(decimal)
                              };
        }

        #endregion

        #region Extensions

        /// <summary>
        /// Sets the Expression property of ExpressionAnimation by converting
        /// the given Expression to appropriate string
        /// </summary>
        /// <typeparam name="T">Type of the Expression</typeparam>
        /// <param name="animation">ExpressionAnimation</param>
        /// <param name="expression">Expression</param>
        /// <returns>Dictionary of parameter names and the parameters</returns>
        public static Dictionary<string, object> SetExpression<T>(this ExpressionAnimation animation,
            Expression<Func<CompositionExpressionContext, T>> expression)
        {
            var ce = CompositionExpressionEngine.CreateCompositionExpression(expression);
            animation.Expression = ce.Expression;
            animation.SetParameters(ce.Parameters);

            return ce.Parameters;
        }

        /// <summary>
        /// Inserts a KeyFrame in the KeyFrameAnimation by converting
        /// the given Expression to appropriate string
        /// </summary>
        /// <typeparam name="T">Type of the Expression</typeparam>
        /// <param name="animation">KeyFrameAnimation</param>
        /// <param name="normalizedProgressKey"></param>
        /// <param name="expression">Expression</param>
        /// <returns>KeyFrameAnimation</returns>
        public static KeyFrameAnimation InsertExpressionKeyFrame<T>(this KeyFrameAnimation animation, float normalizedProgressKey,
            Expression<Func<CompositionExpressionContext, T>> expression)
        {
            var ce = CompositionExpressionEngine.CreateCompositionExpression(expression);
            animation.InsertExpressionKeyFrame(normalizedProgressKey, ce.Expression);
            animation.SetParameters(ce.Parameters);

            return animation;
        }

        /// <summary>
        /// Inserts a KeyFrame in the KeyFrameAnimation by converting
        /// the given Expression to appropriate string
        /// </summary>
        /// <typeparam name="T">Type of the Expression</typeparam>
        /// <param name="animation">KeyFrameAnimation</param>
        /// <param name="normalizedProgressKey"></param>
        /// <param name="expression">Expression</param>
        /// <param name="easingFunction">Easing Function</param>
        /// <returns>KeyFrameAnimation</returns>
        public static KeyFrameAnimation InsertExpressionKeyFrame<T>(this KeyFrameAnimation animation, float normalizedProgressKey,
            Expression<Func<CompositionExpressionContext, T>> expression, CompositionEasingFunction easingFunction)
        {
            var ce = CompositionExpressionEngine.CreateCompositionExpression(expression);
            animation.InsertExpressionKeyFrame(normalizedProgressKey, ce.Expression, easingFunction);
            animation.SetParameters(ce.Parameters);

            return animation;
        }

        /// <summary>
        /// Sets the parameters obtained from parsing the Expression to the CompositionAnimation
        /// </summary>
        /// <typeparam name="T">Type of Animation</typeparam>
        /// <param name="animation">Animation object into which the parameters must be set</param>
        /// <param name="parameters">Parameters to set</param>
        /// <returns>Animation</returns>
        public static T SetParameters<T>(this T animation, Dictionary<string, object> parameters) where T : CompositionAnimation
        {
            Dictionary<string, object> newParameters = new Dictionary<string, object>();

            foreach (var key in parameters.Keys)
            {
                var parameter = parameters[key];
                var type = parameter.GetType();

                // Can the type be converted to float?
                if (Floatables.Contains(type))
                {
                    type = typeof(float);
                    parameter = Convert.ToSingle(parameter);
                }

                if (type == typeof(Point))
                {
                    var point = (Point)parameter;
                    parameter = new Vector3((float)point.X, (float)point.Y, 0);
                    type = typeof(Vector3);
                }

                while (!type.IsPublic())
                {
                    type = type.BaseType();
                }

                MethodInfo methodInfo;
                // Find matching Setxxx method for the given type
                if (SetMethods.TryGetValue(type, out methodInfo) ||
                    ((type.BaseType() != null) && SetMethods.TryGetValue(type.BaseType(), out methodInfo)))
                {
                    // Once a matching SetxxxParameter method is found, Invoke it!
                    methodInfo.Invoke(animation, new[] { key, parameter });
                }
                else
                {
                    // If no matching method is found, then convert the parameter into a CompositionPropertySet
                    // Since we cannot modify the parameters dictionary while we are inside the loop, add the key and the
                    // CompositionPropertySet to the newParameters dictionary, so that the parameters dictionary can be updated 
                    // once the loop completes
                    newParameters[key] = CompositionPropertySetExtensions.ToPropertySet(parameters[key], animation.Compositor);
                }
            }

            // If any key value pairs exist in the newParameters dictionary, then update the parameters dictionary
            if (newParameters.Any())
            {
                foreach (var item in newParameters)
                {
                    parameters[item.Key] = item.Value;
                    // Set item.Value as the Reference Parameter for the animation
                    animation.SetReferenceParameter(item.Key, (CompositionObject)parameters[item.Key]);
                }
                // Clean up newParameters dictionary
                newParameters.Clear();
            }

            return animation;
        }

        #endregion
    }
}
