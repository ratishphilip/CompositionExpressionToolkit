using System.Collections.Generic;

namespace CompositionExpressionToolkit
{
    /// <summary>
    /// This class stores the result of the Expression Tree visit
    /// done by the CompositionExpressionEngine
    /// </summary>
    public class CompositionExpression
    {
        public string Expression { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }
}
