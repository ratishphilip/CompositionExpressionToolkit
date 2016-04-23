@REM This script must be executed inside a Developer Command Prompt for Visual Studio
@REM This script must be present in the Solution Folder of CompositionExpressionToolkit project

@pushd %~dp0

@echo.
@echo ===== Building CompositionExpressionToolkit.dll (x86) =====
@echo.
@msbuild CompositionExpressionToolkit\CompositionExpressionToolkit.csproj /p:Configuration="Release" /p:Platform="x86"

@echo.
@echo ===== Building CompositionExpressionToolkit.dll (x64) =====
@echo.
@msbuild CompositionExpressionToolkit\CompositionExpressionToolkit.csproj /p:Configuration="Release" /p:Platform="x64"

@echo.
@echo ===== Building CompositionExpressionToolkit.dll (ARM) =====
@echo.
@msbuild CompositionExpressionToolkit\CompositionExpressionToolkit.csproj /p:Configuration="Release" /p:Platform="ARM"

@echo.
@echo ===== Updating Reference file =====
@copy /Y CompositionExpressionToolkit\bin\x86\Release\CompositionExpressionToolkit.cs CompositionExpressionToolkit.Ref

@echo.
@echo ===== Building CompositionExpressionToolkit.dll (reference) =====
@echo.
@msbuild CompositionExpressionToolkit.Ref\CompositionExpressionToolkit.Ref.csproj /p:Configuration="Release" /p:Platform="AnyCPU"

@echo.
@echo ===== Creating NuGet package =====
@echo.
@NuGet\nuget.exe pack NuGet\CompositionExpressionToolkit.nuspec

@popd
