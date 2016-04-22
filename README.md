# CompositionExpressionToolkit
__CompositionExpressionToolkit__ is a collection of Extension methods and Helper classes which make it easier to use <a href="https://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.composition.aspx">Windows.UI.Composition</a> features. They include methods for creating statically typed **CompositionAnimation** expressions, **CompositionPropertySet** extension methods, helper methods for creating **ScopedBatchSets** etc.

## CompositionPropertySet extensions
The <a href="https://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.composition.compositionpropertyset.aspx">__CompositionPropertySet__</a> class is like a dictionary which stores key-value pairs. As of now, the values can be of type __float__, __Color__, __Matrix3x2__, __Matrix4x4__, __Quaternion__, __Scalar__, __Vector2__, __Vector3__ and __Vector4__. To store and retrieve, __CompositionPropertySet__ has separate __Insert*xxx*__ and __TryGet*xxx*__ methods for each type.  
__CompositionExpressionToolkit__ provides generic extension methods __Insert<T>__ and __Get<T>__ which makes things simpler.

```C#
public static void Insert<T>(this CompositionPropertySet propertySet, string key, object input);
public static T Get<T>(this CompositionPropertySet propertySet, string key);
```

## Creating statically typed CompositionAnimation Expressions
According to MSDN, <a href="https://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.composition.expressionanimation.aspx">__ExpressionAnimation__</a> and <a href="">__KeyFrameAnimation__</a> use a _mathematical expression_ to specify how the animated value should be calculated each frame. The expressions can reference properties from composition objects. Currently, the _mathematical expression_ is provided in the form of a __string__. Expression animations work by parsing the mathematical expression string and internally converting it to a list of operations to produce an output value.  
Well, using a __string__ for creating an expression increases the chance of introducing errors (spelling, type-mismatch to name a few...). These errors will not be picked up during compile time and can be difficult to debug during runtime too.  
To mitigate this issue, we can use lambda expressions which are statically typed and allow the common errors to be caught during compile time.

__CompositionExpressionToolkit__ provides the following extension methods which allow the user to provide lambda expressions

```C#

public static Dictionary<string, object> SetExpression<T>(this ExpressionAnimation animation,
			Expression<CompositionLambda<T>> expression);

public static KeyFrameAnimation InsertExpressionKeyFrame<T>(this KeyFrameAnimation animation, float normalizedProgressKey,
			Expression<CompositionLambda<T>> expression);
	
public static KeyFrameAnimation InsertExpressionKeyFrame<T>(this KeyFrameAnimation animation, float normalizedProgressKey,
            Expression<CompositionLambda<T>> expression, CompositionEasingFunction easingFunction);
			
```

Each of these methods have a parameter of type `Expression<CompositionLambda<T>>` which defines the actual lambda expression. These extension methods parse the lambda expression and convert them to appropriate mathematical expression string and link to the symbols used in the lambda expression by calling the appropriate __Set*xxx*Parameter__ internally.  

**CompositionLambda&lt;T&gt;** is a delegate which is defined like this

```C#

public delegate T CompositionLambda<out T>(CompositionExpressionContext ctx);

```

**CompositionExpressionContext** class defines a set of dummy helper functions (all the <a href="https://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.composition.expressionanimation.aspx">__helper methods__</a> supported by ExpressionAnimation). These methods are primarily used to create the lambda expression.

### Examples

...




## ScopedBatchHelper

This class contains a static method **CreateScopedBatch** creates a scoped batch and handles the completed event the subscribing and unsubscribing process internally.

__API__:

```C#
public static void CreateScopedBatch(Compositor compositor, 
                                     CompositionBatchTypes batchType, 
                                     Action action, 
                                     Action postAction = null)
```
__Example usage__:

```C#
ScopedBatchHelper.CreateScopedBatch(_compositor, CompositionBatchTypes.Animation,
       () => // Action
       {
           transitionVisual.StartAnimation("Scale.XY", _scaleUpAnimation);
       },
       () => // Post Action
       {
           BackBtn.IsEnabled = true;
       });
```

# Installing from NuGet
To install CompositionExpressionToolkit, run the following command in the  **Package Manager Console**  

`Install-Package CompositionExpressionToolkit`

# Credits
The **CompositionExpressionEngine** is based on the <a href="https://github.com/albahari/ExpressionFormatter">ExpressionFormatter</a> project by **Joseph Albahari** (*the legend behind the*  ***LinqPad*** *tool*). 

*Thank you Joseph Albahari for being generous and making the ExpressionFormatter code open source!*

This project is also influenced from the <a href="https://github.com/aL3891/CompositionAnimationToolkit">CompositionAnimationToolkit</a> by Allan Lindqvist.
