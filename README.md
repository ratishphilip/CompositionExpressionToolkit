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

public static KeyFrameAnimation InsertExpressionKeyFrame<T>(this KeyFrameAnimation animation, 
	float normalizedProgressKey, Expression<CompositionLambda<T>> expression);
	
public static KeyFrameAnimation InsertExpressionKeyFrame<T>(this KeyFrameAnimation animation, 
	float normalizedProgressKey, 
	Expression<CompositionLambda<T>> expression, CompositionEasingFunction easingFunction);
			
```

Each of these methods have a parameter of type **Expression&lt;CompositionLambda&lt;T&gt;&gt;** which defines the actual lambda expression. These extension methods parse the lambda expression and convert them to appropriate mathematical expression string and link to the symbols used in the lambda expression by calling the appropriate __Set*xxx*Parameter__ internally.  

**CompositionLambda&lt;T&gt;** is a delegate which is defined like this

```C#

public delegate T CompositionLambda<out T>(CompositionExpressionContext ctx);

```

**CompositionExpressionContext** class defines a set of dummy helper functions (all the <a href="https://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.composition.expressionanimation.aspx">__helper methods__</a> supported by ExpressionAnimation). These methods are primarily used to create the lambda expression.

### Examples

#### Example 1

**Without using CompositionExpressionToolkit**

```C#
Point position = new Point(0,0);
Vector3KeyFrameAnimation offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
CompositionPropertySet scrollProperties = 
	ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);

position.X += scrollViewer.HorizontalOffset;
position.Y += scrollViewer.VerticalOffset;
offsetAnimation.Duration = totalDuration;

// Create expression string
string expression = 
	"Vector3(scrollingProperties.Translation.X, scrollingProperties.Translation.Y, 0) + itemOffset";

// Set the expression
offsetAnimation.InsertExpressionKeyFrame(1f, expression);

// Set the parameters
offsetAnimation.SetReferenceParameter("scrollingProperties", scrollProperties);
offsetAnimation.SetVector3Parameter("itemOffset", new Vector3((float) position.X, (float) position.Y, 0));

```

**Using CompositionExpressionToolkit**

```C#
Point position = new Point(0,0);
Vector3KeyFrameAnimation offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
CompositionPropertySet scrollProperties = 
	ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);

position.X += scrollViewer.HorizontalOffset;
position.Y += scrollViewer.VerticalOffset;
var itemOffset = new Vector3(position.X.Single(), position.Y.Single(), 0);

offsetAnimation.Duration = totalDuration;

// Create the CompositionLambda Expression
Expression<CompositionLambda<Vector3>> expression =
	c => c.Vector3(scrollProperties.Get<TranslateTransform>("Translation").X.Single(),
		 	scrollProperties.Get<TranslateTransform>("Translation").Y.Single(), 0) + itemOffset;
		 
// Set the Expression
offsetAnimation.InsertExpressionKeyFrame(1f, expression);

```




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
## Converting from `double` to `float`
Most of the values which is calculated or derived from the properties of **UIElement** (and its derived classes) are of type **double**. But most of the classes in **Sytem.Numerics** and **Windows.UI.Composition** namespaces require the values to be of type **float**. If you find adding a `(float)` cast before each and every variable of type **double**, you can call the **.Single** extension method on the variable which converts the **double** into **float**. Ensure that the value of the double variable is between **System.Single.MinValue** and **System.Single.MaxValue** otherwise **ArgumentOutOfRangeException** will be thrown.  

**Note**: _Conversion of a value from **double** to **float** will reduce the precision of the value._  

**Example**
```C#
double width = window.Width;
double height = window.Height;
Vector2 size = new Vector2(width.Single(), height.Single());
```

# Installing from NuGet
To install CompositionExpressionToolkit, run the following command in the  **Package Manager Console**  

`Install-Package CompositionExpressionToolkit`

# Credits
The **CompositionExpressionEngine** is based on the <a href="https://github.com/albahari/ExpressionFormatter">ExpressionFormatter</a> project by **Joseph Albahari** (*the legend behind the*  ***LinqPad*** *tool*). 

*Thank you Joseph Albahari for being generous and making the ExpressionFormatter code open source!*

This project is also influenced from the <a href="https://github.com/aL3891/CompositionAnimationToolkit">CompositionAnimationToolkit</a> by Allan Lindqvist.
