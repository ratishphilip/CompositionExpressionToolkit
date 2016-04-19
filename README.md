# CompositionExpressionToolkit
A toolkit for setting the Expression in **CompositionAnimation** via **Lambda Expressions**. It contains a **CompositionExpressionEngine** which converts the Lambda expression to an appropriate string that can be used as an input for setting the **ExpressionAnimation.Expression** property or inserting an ExpressionKeyFrame in a **KeyFrameAnimation**.

# Usage

## ExpressionAnimationExtensions

## ScopedBatchHelper

This class contains a method **CreateScopedBatch** creates a scoped batch and handles the completed event the subscribing and unsubscribing process internally.

Example usage:


```
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
