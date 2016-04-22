namespace CompositionExpressionToolkit
{
    public static partial class CompositionAnimationExtensions
    {
        public static Windows.UI.Composition.KeyFrameAnimation InsertExpressionKeyFrame<T>(this Windows.UI.Composition.KeyFrameAnimation animation, float normalizedProgressKey, System.Linq.Expressions.Expression<CompositionExpressionToolkit.CompositionLambda<T>> expression) { return default(Windows.UI.Composition.KeyFrameAnimation); }
        public static Windows.UI.Composition.KeyFrameAnimation InsertExpressionKeyFrame<T>(this Windows.UI.Composition.KeyFrameAnimation animation, float normalizedProgressKey, System.Linq.Expressions.Expression<CompositionExpressionToolkit.CompositionLambda<T>> expression, Windows.UI.Composition.CompositionEasingFunction easingFunction) { return default(Windows.UI.Composition.KeyFrameAnimation); }
        public static System.Collections.Generic.Dictionary<string, object> SetExpression<T>(this Windows.UI.Composition.ExpressionAnimation animation, System.Linq.Expressions.Expression<CompositionExpressionToolkit.CompositionLambda<T>> expression) { return default(System.Collections.Generic.Dictionary<string, object>); }
        public static T SetParameters<T>(this T animation, System.Collections.Generic.Dictionary<string, object> parameters) where T : Windows.UI.Composition.CompositionAnimation { return default(T); }
    }
    public partial class CompositionExpressionContext
    {
        internal CompositionExpressionContext() { }
        public System.Numerics.Vector2 Abs(System.Numerics.Vector2 value) { return default(System.Numerics.Vector2); }
        public System.Numerics.Vector3 Abs(System.Numerics.Vector3 value) { return default(System.Numerics.Vector3); }
        public System.Numerics.Vector4 Abs(System.Numerics.Vector4 value) { return default(System.Numerics.Vector4); }
        public float Abs(float value) { return default(float); }
        public float Acos(float value) { return default(float); }
        public float Asin(float value) { return default(float); }
        public float Atan(float value) { return default(float); }
        public float Ceiling(float value) { return default(float); }
        public float Clamp(float value, float min, float max) { return default(float); }
        public Windows.UI.Color ColorLerp(Windows.UI.Color ColorTo, Windows.UI.Color ColorFrom, float Progression) { return default(Windows.UI.Color); }
        public Windows.UI.Color ColorLerpHSL(Windows.UI.Color ColorTo, Windows.UI.Color ColorFrom, float Progression) { return default(Windows.UI.Color); }
        public Windows.UI.Color ColorLerpRGB(Windows.UI.Color ColorTo, Windows.UI.Color ColorFrom, float Progression) { return default(Windows.UI.Color); }
        public System.Numerics.Quaternion Concatenate(System.Numerics.Quaternion value, System.Numerics.Quaternion value2) { return default(System.Numerics.Quaternion); }
        public float Cos(float value) { return default(float); }
        public System.Numerics.Vector2 Distance(System.Numerics.Vector2 value1, System.Numerics.Vector2 value2) { return default(System.Numerics.Vector2); }
        public System.Numerics.Vector3 Distance(System.Numerics.Vector3 value1, System.Numerics.Vector3 value2) { return default(System.Numerics.Vector3); }
        public System.Numerics.Vector4 Distance(System.Numerics.Vector4 value1, System.Numerics.Vector4 value2) { return default(System.Numerics.Vector4); }
        public float DistanceSquared(System.Numerics.Vector2 value1, System.Numerics.Vector2 value2) { return default(float); }
        public float DistanceSquared(System.Numerics.Vector3 value1, System.Numerics.Vector3 value2) { return default(float); }
        public float DistanceSquared(System.Numerics.Vector4 value1, System.Numerics.Vector4 value2) { return default(float); }
        public float Floor(float value) { return default(float); }
        public System.Numerics.Vector2 Inverse(System.Numerics.Vector2 value) { return default(System.Numerics.Vector2); }
        public System.Numerics.Vector3 Inverse(System.Numerics.Vector3 value) { return default(System.Numerics.Vector3); }
        public System.Numerics.Vector4 Inverse(System.Numerics.Vector4 value) { return default(System.Numerics.Vector4); }
        public System.Numerics.Vector2 Length(System.Numerics.Vector2 value) { return default(System.Numerics.Vector2); }
        public System.Numerics.Vector3 Length(System.Numerics.Vector3 value) { return default(System.Numerics.Vector3); }
        public System.Numerics.Vector4 Length(System.Numerics.Vector4 value) { return default(System.Numerics.Vector4); }
        public System.Numerics.Vector2 LengthSquared(System.Numerics.Vector2 value) { return default(System.Numerics.Vector2); }
        public System.Numerics.Vector3 LengthSquared(System.Numerics.Vector3 value) { return default(System.Numerics.Vector3); }
        public System.Numerics.Vector4 LengthSquared(System.Numerics.Vector4 value) { return default(System.Numerics.Vector4); }
        public System.Numerics.Matrix3x2 Lerp(System.Numerics.Matrix3x2 value1, System.Numerics.Matrix3x2 value2, float progress) { return default(System.Numerics.Matrix3x2); }
        public System.Numerics.Matrix4x4 Lerp(System.Numerics.Matrix4x4 value1, System.Numerics.Matrix4x4 value2, float progress) { return default(System.Numerics.Matrix4x4); }
        public System.Numerics.Vector2 Lerp(System.Numerics.Vector2 value1, System.Numerics.Vector2 value2, float progress) { return default(System.Numerics.Vector2); }
        public System.Numerics.Vector3 Lerp(System.Numerics.Vector3 value1, System.Numerics.Vector3 value2, float progress) { return default(System.Numerics.Vector3); }
        public System.Numerics.Vector4 Lerp(System.Numerics.Vector4 value1, System.Numerics.Vector4 value2, float progress) { return default(System.Numerics.Vector4); }
        public float Ln(float value) { return default(float); }
        public float Log10(float value) { return default(float); }
        public System.Numerics.Matrix3x2 Matrix3x2(float M11, float M12, float M21, float M22, float M31, float M32) { return default(System.Numerics.Matrix3x2); }
        public System.Numerics.Matrix3x2 Matrix3x2CreateFromScale(System.Numerics.Vector2 scale) { return default(System.Numerics.Matrix3x2); }
        public System.Numerics.Matrix3x2 Matrix3x2CreateFromTranslation(System.Numerics.Vector2 translation) { return default(System.Numerics.Matrix3x2); }
        public System.Numerics.Matrix4x4 Matrix4x4(float M11, float M12, float M13, float M14, float M21, float M22, float M23, float M24, float M31, float M32, float M33, float M34, float M41, float M42, float M43, float M44) { return default(System.Numerics.Matrix4x4); }
        public System.Numerics.Matrix4x4 Matrix4x4CreateFromAxisAngle(System.Numerics.Vector3 axis, float angle) { return default(System.Numerics.Matrix4x4); }
        public System.Numerics.Matrix4x4 Matrix4x4CreateFromScale(System.Numerics.Vector3 scale) { return default(System.Numerics.Matrix4x4); }
        public System.Numerics.Matrix4x4 Matrix4x4CreateFromTranslation(System.Numerics.Vector3 translation) { return default(System.Numerics.Matrix4x4); }
        public float Max(float value1, float value2) { return default(float); }
        public float Min(float value1, float value2) { return default(float); }
        public float Mod(float dividend, float divisor) { return default(float); }
        public float Normalize() { return default(float); }
        public System.Numerics.Vector2 Normalize(System.Numerics.Vector2 value) { return default(System.Numerics.Vector2); }
        public System.Numerics.Vector3 Normalize(System.Numerics.Vector3 value) { return default(System.Numerics.Vector3); }
        public System.Numerics.Vector4 Normalize(System.Numerics.Vector4 value) { return default(System.Numerics.Vector4); }
        public float Pow(float value, int power) { return default(float); }
        public System.Numerics.Quaternion QuaternionCreateFromAxisAngle(System.Numerics.Vector3 axis, float angle) { return default(System.Numerics.Quaternion); }
        public float Round(float value) { return default(float); }
        public System.Numerics.Matrix3x2 Scale(System.Numerics.Matrix3x2 value, float factor) { return default(System.Numerics.Matrix3x2); }
        public System.Numerics.Matrix4x4 Scale(System.Numerics.Matrix4x4 value, float factor) { return default(System.Numerics.Matrix4x4); }
        public System.Numerics.Vector2 Scale(System.Numerics.Vector2 value, float factor) { return default(System.Numerics.Vector2); }
        public System.Numerics.Vector3 Scale(System.Numerics.Vector3 value, float factor) { return default(System.Numerics.Vector3); }
        public System.Numerics.Vector4 Scale(System.Numerics.Vector4 value, float factor) { return default(System.Numerics.Vector4); }
        public float Sin(float value) { return default(float); }
        public System.Numerics.Quaternion Slerp(System.Numerics.Quaternion value1, System.Numerics.Quaternion value2, float progress) { return default(System.Numerics.Quaternion); }
        public float Sqrt(float value) { return default(float); }
        public float Square(float value) { return default(float); }
        public float Tan(float value) { return default(float); }
        public float ToDegrees(float radians) { return default(float); }
        public float ToRadians(float degrees) { return default(float); }
        public System.Numerics.Vector2 Transform(System.Numerics.Vector2 value, System.Numerics.Matrix3x2 matrix) { return default(System.Numerics.Vector2); }
        public System.Numerics.Vector4 Transform(System.Numerics.Vector4 value, System.Numerics.Matrix4x4 matrix) { return default(System.Numerics.Vector4); }
        public System.Numerics.Vector2 Vector2(float x, float y) { return default(System.Numerics.Vector2); }
        public System.Numerics.Vector3 Vector3(float x, float y, float z) { return default(System.Numerics.Vector3); }
        public System.Numerics.Vector4 Vector4(float x, float y, float z, float w) { return default(System.Numerics.Vector4); }
    }
    public abstract partial class CompositionExpressionEngine
    {
        protected CompositionExpressionEngine() { }
        public static CompositionExpressionToolkit.CompositionExpressionResult CreateCompositionExpression<T>(System.Linq.Expressions.Expression<CompositionExpressionToolkit.CompositionLambda<T>> expression) { return default(CompositionExpressionToolkit.CompositionExpressionResult); }
    }
    public partial class CompositionExpressionResult
    {
        public CompositionExpressionResult() { }
        public string Expression { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(string); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
        public System.Collections.Generic.Dictionary<string, object> Parameters { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.Collections.Generic.Dictionary<string, object>); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
    }
    public delegate T CompositionLambda<out T>(CompositionExpressionToolkit.CompositionExpressionContext ctx);
    public static partial class CompositionPropertySetExtensions
    {
        public static T Get<T>(this Windows.UI.Composition.CompositionPropertySet propertySet, string key) { return default(T); }
        public static void Insert<T>(this Windows.UI.Composition.CompositionPropertySet propertySet, string key, object input) { }
        public static Windows.UI.Composition.CompositionPropertySet ToPropertySet(object input, Windows.UI.Composition.Compositor compositor) { return default(Windows.UI.Composition.CompositionPropertySet); }
    }
    public static partial class ScopedBatchHelper
    {
        public static void CreateScopedBatch(Windows.UI.Composition.Compositor compositor, Windows.UI.Composition.CompositionBatchTypes batchType, System.Action action, System.Action postAction=null) { }
    }
    public static partial class TypeExtensions
    {
        public static System.Type BaseType(this System.Type type) { return default(System.Type); }
        public static System.Type[] GetGenericArguments(this System.Type type) { return default(System.Type[]); }
        public static System.Reflection.MethodInfo GetMethod(this System.Type type, string methodName) { return default(System.Reflection.MethodInfo); }
        public static System.Reflection.PropertyInfo GetProperty(this System.Type type, string propertyName) { return default(System.Reflection.PropertyInfo); }
        public static object GetPropertyValue(this object instance, string propertyValue) { return default(object); }
        public static System.Reflection.TypeInfo GetTypeInfo(this System.Type type) { return default(System.Reflection.TypeInfo); }
        public static bool IsAssignableFrom(this System.Type type, System.Type parentType) { return default(bool); }
        public static bool IsEnum(this System.Type type) { return default(bool); }
        public static bool IsGenericType(this System.Type type) { return default(bool); }
        public static bool IsPrimitive(this System.Type type) { return default(bool); }
        public static bool IsPublic(this System.Type type) { return default(bool); }
        public static bool IsSubclassOf(this System.Type type, System.Type parentType) { return default(bool); }
    }
}
