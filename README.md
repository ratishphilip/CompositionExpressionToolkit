<p align="center">
<img align="center" src="https://cloud.githubusercontent.com/assets/7021835/14901044/4da2cbb6-0d48-11e6-9a5b-872324451063.png" alt="CompositionExpressionToolkit logo">
</p>

__CompositionExpressionToolkit__ is a collection of Extension methods and Helper classes which make it easier to use <a href="https://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.composition.aspx">Windows.UI.Composition</a> features. They include methods for creating statically typed **CompositionAnimation** expressions, **CompositionPropertySet** extension methods, helper methods for creating **ScopedBatchSets** etc.

# CompositionExpressionToolkit Internals

## 1. CompositionPropertySet extensions
The <a href="https://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.composition.compositionpropertyset.aspx">__CompositionPropertySet__</a> class is like a dictionary which stores key-value pairs. As of now, the values can be of type __float__, __Color__, __Matrix3x2__, __Matrix4x4__, __Quaternion__, __Scalar__, __Vector2__, __Vector3__ and __Vector4__. To store and retrieve, __CompositionPropertySet__ has separate __Insert*xxx*__ and __TryGet*xxx*__ methods for each type.  
__CompositionExpressionToolkit__ provides generic extension methods __Insert<T>__ and __Get<T>__ which makes things simpler.

```C#
public static void Insert<T>(this CompositionPropertySet propertySet, string key, object input);
public static T Get<T>(this CompositionPropertySet propertySet, string key);
```

## 2. Creating statically typed CompositionAnimation Expressions
According to MSDN, <a href="https://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.composition.expressionanimation.aspx">__ExpressionAnimation__</a> and <a href="">__KeyFrameAnimation__</a> use a _mathematical expression_ to specify how the animated value should be calculated each frame. The expressions can reference properties from composition objects. _Currently, the mathematical expression is provided in the form of a **string**_. Expression animations work by parsing the mathematical expression string and internally converting it to a list of operations to produce an output value.  
Well, using a __string__ for creating an expression increases the chance of introducing errors (spelling, type-mismatch to name a few...). These errors will not be picked up during compile time and can be difficult to debug during runtime too.  
To mitigate this issue, we can use lambda expressions which are statically typed and allow the common errors to be caught during compile time.

**CompositionExpressionToolkit** provides the following extension methods which allow the user to provide lambda expressions

```C#

public static ExpressionAnimation CreateExpressionAnimation<T>(this Compositor compositor,
            Expression<CompositionLambda<T>> expression);
            
public static Dictionary<string, object> SetExpression<T>(this ExpressionAnimation animation,
			Expression<CompositionLambda<T>> expression);

public static KeyFrameAnimation InsertExpressionKeyFrame<T>(this KeyFrameAnimation animation, 
	float normalizedProgressKey, Expression<CompositionLambda<T>> expression);
	
public static KeyFrameAnimation InsertExpressionKeyFrame<T>(this KeyFrameAnimation animation, 
	float normalizedProgressKey, 
	Expression<CompositionLambda<T>> expression, CompositionEasingFunction easingFunction);
			
```

Each of these methods have a parameter of type **Expression&lt;CompositionLambda&lt;T&gt;&gt;** which defines the actual lambda expression. These extension methods parse the lambda expression and convert them to appropriate mathematical expression string and link to the symbols used in the lambda expression by calling the appropriate __Set*xxx*Parameter__ internally.  

**CompositionLambda&lt;T&gt;** is a delegate which is defined as

```C#

public delegate T CompositionLambda<T>(CompositionExpressionContext<T> ctx);

```

**CompositionExpressionContext&lt;T&gt;** is a generic class which defines a set of dummy helper functions (all the <a href="https://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.composition.expressionanimation.aspx">__helper methods__</a> supported by ExpressionAnimation). These methods are primarily used to create the lambda expression. This class also defines the **StartingValue** and **FinalValue** properties for use within **CompositionLambda** expressions.

**CompositionExpressionToolkit** also provides the following extension methods which allow the user to provide a key-value pair (or a set of key-value pairs) as parameter(s) for the **CompositionAnimation**

```C#
public static bool SetParameter<T>(this T animation, string key, object input) 
    where T : CompositionAnimation 
{
}

public static T SetParameters<T>(this T animation, Dictionary<string, object> parameters) 
    where T : CompositionAnimation 
{
}
        
```

### Examples
The following examples show how expressions are currently provided in string format to ExpressionAnimation. These examples also show how, using **CompositionExpressionToolkit**, **Expression&lt;CompositionLambda&lt;T&gt;&gt;** can be created (for the same scenario) and provided to the ExpressionAnimation.

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

#### Example 2

**Without using CompositionExpressionToolkit**

```C#
ScrollViewer myScrollViewer = ThumbnailList.GetFirstDescendantOfType<ScrollViewer>();
var scrollProperties = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(myScrollViewer);
			
ExpressAnimation parallaxExpression = compositor.CreateExpressionAnimation();
parallaxExpression.SetScalarParameter("StartOffset", 0.0f);
parallaxExpression.SetScalarParameter("ParallaxValue", 0.5f);
parallaxExpression.SetScalarParameter("ItemHeight", 0.0f);
parallaxExpression.SetReferenceParameter("ScrollManipulation", scrollProperties);
parallaxExpression.Expression = "(ScrollManipulation.Translation.Y + StartOffset - (0.5 * ItemHeight)) * 
	ParallaxValue - (ScrollManipulation.Translation.Y + StartOffset - (0.5 * ItemHeight))";
	
```

**Using CompositionExpressionToolkit**

```C#
ScrollViewer myScrollViewer = ThumbnailList.GetFirstDescendantOfType<ScrollViewer>();
var scrollProperties = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(myScrollViewer);
			
ExpressAnimation parallaxExpression = compositor.CreateExpressionAnimation();

var StartOffset = 0.0f;
var ParallaxValue = 0.5f;
var ItemHeight = 0.0f;
// Create the Expression
Expression<CompositionLambda<float>> expr = c =>
		((scrollProperties.Get<TranslateTransform>("Translation").Y + StartOffset - (0.5 * ItemHeight)) *
		 ParallaxValue - (scrollProperties.Get<TranslateTransform>("Translation").Y + StartOffset - 
		 (0.5 * ItemHeight))).Single();
// Set the Expression
parallaxExpression.SetExpression(expr);

```

#### Example 3 

This example shows how to provide _this.StartingValue_ and _this.FinalValue_ in a **CompositionLambda** Expression  

**Without using CompositionExpressionToolkit**

```C#
var scaleKeyFrameAnimation = _compositor.CreateVector3KeyFrameAnimation();
scaleKeyFrameAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
scaleKeyFrameAnimation.Duration = TimeSpan.FromSeconds(3);

var rotationAnimation = _compositor.CreateScalarKeyFrameAnimation();
rotationAnimation.InsertExpressionKeyFrame(1.0f, "this.StartingValue + 45.0f");
rotationAnimation.Duration = TimeSpan.FromSeconds(3);

```

**Using CompositionExpressionToolkit**

```C#
Expression<CompositionLambda<float>> expr1 = c => c.FinalValue;

var scaleKeyFrameAnimation = _compositor.CreateVector3KeyFrameAnimation();
scaleKeyFrameAnimation.InsertExpressionKeyFrame(1.0f, expr1);
scaleKeyFrameAnimation.Duration = TimeSpan.FromSeconds(3);

Expression<CompositionLambda<float>> expr2 = c => c.StartingValue + 45.0f;

var rotationAnimation = compositor.CreateScalarKeyFrameAnimation();
rotationAnimation.InsertExpressionKeyFrame(1.0f, expr2);
rotationAnimation.Duration = TimeSpan.FromSeconds(3);

```

#### Example 4

The following table shows few examples of **Expression&lt;CompositionLambda&lt;T&gt;&gt;** expressed as **String**

| T | Expression&lt;CompositionLambda&lt;T&gt;&gt; | String |
|-----|-----|-----|
|`float` | `c => c.StartingValue`| `"this.StartingValue"` |
|`float` | `c => c.FinalValue`| `"this.FinalValue"` |
|`float` | `c => c.StartingValue + (45.0).Single()`| `"this.StartingValue + 45"` |
|`Vector3` | `c => c.Vector3(propSet.Get<TranslateTransform>("Translation").X, propSet.Get<TranslateTransform>("Translation").Y, 0)` <br /> _[**propSet** is of type **CompositionPropertySet**]_ | `"Vector3(propSet.Translation.X, propSet.Translation.Y, 0)` |
|`Vector3` | `c => c.Vector3(propSet.Properties.Get<TranslateTransform>("Translation").X, propSet.Properties.Get<TranslateTransform>("Translation").Y, 0)`<br /> _[**propSet** is of type **CompositionPropertySet**]_ | `"Vector3(propSet.Translation.X, propSet.Translation.Y, 0)` |

## 3. ScopedBatchHelper

This class contains a static method **CreateScopedBatch** creates a scoped batch and handles the subscribing and unsubscribing process of the **Completed** event internally.

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
## 4. Converting from `double` to `float`
Most of the values which is calculated or derived from the properties of **UIElement** (and its derived classes) are of type **double**. But most of the classes in **Sytem.Numerics** and **Windows.UI.Composition** namespaces require the values to be of type **float**. If you find it tedious adding a `(float)` cast before each and every variable of type **double**, you can call the **.Single** extension method for **System.Double** instead, which converts the **double** into **float**. Ensure that the value of the double variable is between **System.Single.MinValue** and **System.Single.MaxValue** otherwise **ArgumentOutOfRangeException** will be thrown.  

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
