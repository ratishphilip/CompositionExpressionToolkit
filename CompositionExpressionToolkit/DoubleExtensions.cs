using System;

namespace CompositionExpressionToolkit
{
    /// <summary>
    /// Extension methods for System.Double
    /// </summary>
    public static class DoubleExtensions
    {
        /// <summary>
        /// Converts double value to float
        /// </summary>
        /// <param name="value">double value</param>
        /// <returns>float</returns>
        public static float Single(this double value)
        {
            // Double to float conversion can overflow.
            try
            {
                return Convert.ToSingle(value);
            }
            catch (OverflowException ex)
            {
                throw new ArgumentOutOfRangeException("Cannot convert the double value to float!", ex);
            }
        }
    }
}
