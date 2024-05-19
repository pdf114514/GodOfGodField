using Microsoft.JSInterop;

namespace GodOfGodField.Client;

public class HTMLElement(IJSInProcessObjectReference elementRef) {
    public IJSInProcessObjectReference ElementRef { get; } = elementRef;

    public CSSStyleDeclaration Style => new(ElementRef.GetProperty<IJSInProcessObjectReference>("style"));
    public void Focus() => ElementRef.InvokeVoid("focus");
    public void Blur() => ElementRef.InvokeVoid("blur");
    public DOMRectReadOnly GetBoundingClientRect() => new(ElementRef.Invoke<IJSInProcessObjectReference>("getBoundingClientRect"));
}

public class CSSStyleDeclaration(IJSInProcessObjectReference styleRef) {
    public IJSInProcessObjectReference StyleRef { get; } = styleRef;

    public string this[string name] { get => StyleRef.Invoke<string>("getProperty", name); set => StyleRef.InvokeVoid("setProperty", name, value); }
}

public class DOMRectReadOnly(IJSInProcessObjectReference rectRef) {
    public IJSInProcessObjectReference RectRef { get; } = rectRef;

    public double X => RectRef.GetProperty<double>("x");
    public double Y => RectRef.GetProperty<double>("y");
    public double Width => RectRef.GetProperty<double>("width");
    public double Height => RectRef.GetProperty<double>("height");
    public double Top => RectRef.GetProperty<double>("top");
    public double Right => RectRef.GetProperty<double>("right");
    public double Bottom => RectRef.GetProperty<double>("bottom");
    public double Left => RectRef.GetProperty<double>("left");
}