using Microsoft.JSInterop;

namespace GodOfGodField.Client;

public class HTMLCanvasElement(IJSInProcessObjectReference elementRef) : HTMLElement(elementRef) {
    public int Height { get => ElementRef.GetProperty<int>("height"); set => ElementRef.SetProperty("height", value);}
    public int Width { get => ElementRef.GetProperty<int>("width"); set => ElementRef.SetProperty("width", value);}

    public CanvasRenderingContext2D GetContext2D() => new(ElementRef.Invoke<IJSInProcessObjectReference>("getContext", "2d"));
    public CanvasRenderingContext2D GetContext2D(object options) => new(ElementRef.Invoke<IJSInProcessObjectReference>("getContext", "2d", options));
}

public class CanvasRenderingContext2D(IJSInProcessObjectReference contextRef) {
    public IJSInProcessObjectReference ContextRef { get; } = contextRef;
    public HTMLCanvasElement Canvas { get => new(ContextRef.GetProperty<IJSInProcessObjectReference>("canvas")); }
    public string Direction { get => ContextRef.GetProperty<string>("direction"); set => ContextRef.SetProperty("direction", value); } // "ltr" | "rtl" | "inherit"
    public string FillStyle { get => ContextRef.GetProperty<string>("fillStyle"); set => ContextRef.SetProperty("fillStyle", value); } // string | CanvasGradient | CanvasPattern
    public string Filter { get => ContextRef.GetProperty<string>("filter"); set => ContextRef.SetProperty("filter", value); } // string
    public string Font { get => ContextRef.GetProperty<string>("font"); set => ContextRef.SetProperty("font", value); } // string
    public double GlobalAlpha { get => ContextRef.GetProperty<double>("globalAlpha"); set => ContextRef.SetProperty("globalAlpha", value); } // double
    public string GlobalCompositeOperation { get => ContextRef.GetProperty<string>("globalCompositeOperation"); set => ContextRef.SetProperty("globalCompositeOperation", value); } // string
    public bool ImageSmoothingEnabled { get => ContextRef.GetProperty<bool>("imageSmoothingEnabled"); set => ContextRef.SetProperty("imageSmoothingEnabled", value); } // bool
    public string ImageSmoothingQuality { get => ContextRef.GetProperty<string>("imageSmoothingQuality"); set => ContextRef.SetProperty("imageSmoothingQuality", value); } // "low" | "medium" | "high"
    public string LineCap { get => ContextRef.GetProperty<string>("lineCap"); set => ContextRef.SetProperty("lineCap", value); } // "butt" | "round" | "square"
    public double LineDashOffset { get => ContextRef.GetProperty<double>("lineDashOffset"); set => ContextRef.SetProperty("lineDashOffset", value); } // double
    public string LineJoin { get => ContextRef.GetProperty<string>("lineJoin"); set => ContextRef.SetProperty("lineJoin", value); } // "bevel" | "round" | "miter"
    public double LineWidth { get => ContextRef.GetProperty<double>("lineWidth"); set => ContextRef.SetProperty("lineWidth", value); } // double
    public double MiterLimit { get => ContextRef.GetProperty<double>("miterLimit"); set => ContextRef.SetProperty("miterLimit", value); } // double
    public double ShadowBlur { get => ContextRef.GetProperty<double>("shadowBlur"); set => ContextRef.SetProperty("shadowBlur", value); } // double
    public string ShadowColor { get => ContextRef.GetProperty<string>("shadowColor"); set => ContextRef.SetProperty("shadowColor", value); } // string
    public double ShadowOffsetX { get => ContextRef.GetProperty<double>("shadowOffsetX"); set => ContextRef.SetProperty("shadowOffsetX", value); } // double
    public double ShadowOffsetY { get => ContextRef.GetProperty<double>("shadowOffsetY"); set => ContextRef.SetProperty("shadowOffsetY", value); } // double
    public string StrokeStyle { get => ContextRef.GetProperty<string>("strokeStyle"); set => ContextRef.SetProperty("strokeStyle", value); } // string | CanvasGradient | CanvasPattern
    public string TextAlign { get => ContextRef.GetProperty<string>("textAlign"); set => ContextRef.SetProperty("textAlign", value); } // "start" | "end" | "left" | "right" | "center"
    public string TextBaseline { get => ContextRef.GetProperty<string>("textBaseline"); set => ContextRef.SetProperty("textBaseline", value); } // "top" | "hanging" | "middle" | "alphabetic" | "ideographic" | "bottom"
    public void Arc(double x, double y, double radius, double startAngle, double endAngle, bool anticlockwise = false) => ContextRef.InvokeVoid("arc", x, y, radius, startAngle, endAngle, anticlockwise);
    public void ArcTo(double x1, double y1, double x2, double y2, double radius) => ContextRef.InvokeVoid("arcTo", x1, y1, x2, y2, radius);
    public void BeginPath() => ContextRef.InvokeVoid("beginPath");
    public void BezierCurveTo(double cp1x, double cp1y, double cp2x, double cp2y, double x, double y) => ContextRef.InvokeVoid("bezierCurveTo", cp1x, cp1y, cp2x, cp2y, x, y);
    public void ClearRect(int x, int y, int width, int height) => ContextRef.InvokeVoid("clearRect", x, y, width, height);
    public void Clip() => ContextRef.InvokeVoid("clip");
    public void ClosePath() => ContextRef.InvokeVoid("closePath");
    public ImageData CreateImageData(int width, int height) => new(ContextRef.Invoke<IJSInProcessObjectReference>("createImageData", width, height));
    public ImageData CreateImageData(ImageData imageData) => new(ContextRef.Invoke<IJSInProcessObjectReference>("createImageData", imageData.ImageDataRef));
    public CanvasGradient CreateLinearGradient(double x0, double y0, double x1, double y1) => new(ContextRef.Invoke<IJSInProcessObjectReference>("createLinearGradient", x0, y0, x1, y1));
    // public CanvasGradient CreatePattern(HTMLImageElement image, string repetition) => new(ContextRef.Invoke<IJSInProcessObjectReference>("createPattern", image.ElementRef, repetition));
    public CanvasGradient CreateRadialGradient(double x0, double y0, double r0, double x1, double y1, double r1) => new(ContextRef.Invoke<IJSInProcessObjectReference>("createRadialGradient", x0, y0, r0, x1, y1, r1));
    // public void DrawFocusIfNeeded(Element element) => ContextRef.InvokeVoid("drawFocusIfNeeded", element.ElementRef);
    // public void DrawFocusIfNeeded(Path2D path, Element element) => ContextRef.InvokeVoid("drawFocusIfNeeded", path.Path2DRef, element.ElementRef);
    public void DrawImage(IJSInProcessObjectReference image, double dx, double dy) => ContextRef.InvokeVoid("drawImage", image, dx, dy);
    public void DrawImage(IJSInProcessObjectReference image, double dx, double dy, double dWidth, double dHeight) => ContextRef.InvokeVoid("drawImage", image, dx, dy, dWidth, dHeight);
    public void DrawImage(IJSInProcessObjectReference image, double sx, double sy, double sWidth, double sHeight, double dx, double dy, double dWidth, double dHeight) => ContextRef.InvokeVoid("drawImage", image, sx, sy, sWidth, sHeight, dx, dy, dWidth, dHeight);
    public void Ellipse(double x, double y, double radiusX, double radiusY, double rotation, double startAngle, double endAngle, bool anticlockwise = false) => ContextRef.InvokeVoid("ellipse", x, y, radiusX, radiusY, rotation, startAngle, endAngle, anticlockwise);
    public void Fill() => ContextRef.InvokeVoid("fill");
    public void Fill(string fillRule) => ContextRef.InvokeVoid("fill", fillRule);
    // public void Fill(Path2D path, string? fillRule = null) => ContextRef.InvokeVoid("fill", path.Path2DRef, fillRule);
    public void FillRect(double x, double y, double width, double height) => ContextRef.InvokeVoid("fillRect", x, y, width, height);
    public void FillText(string text, double x, double y) => ContextRef.InvokeVoid("fillText", text, x, y);
    public void FillText(string text, double x, double y, double maxWidth) => ContextRef.InvokeVoid("fillText", text, x, y, maxWidth);
    public ImageData GetImageData(int sx, int sy, int sw, int sh) => new(ContextRef.Invoke<IJSInProcessObjectReference>("getImageData", sx, sy, sw, sh));
    public double[] GetLineDash() => ContextRef.Invoke<double[]>("getLineDash");
    // public DOMMatrix GetTransform() => new(ContextRef.Invoke<IJSInProcessObjectReference>("getTransform"));
    public bool IsPointInPath(double x, double y) => ContextRef.Invoke<bool>("isPointInPath", x, y);
    public bool IsPointInPath(double x, double y, string fillRule) => ContextRef.Invoke<bool>("isPointInPath", x, y, fillRule);
    // public bool IsPointInPath(Path2D path, double x, double y, string? fillRule = null) => ContextRef.Invoke<bool>("isPointInPath", path.Path2DRef, x, y, fillRule);
    public bool IsPointInStroke(double x, double y) => ContextRef.Invoke<bool>("isPointInStroke", x, y);
    // public bool IsPointInStroke(Path2D path, double x, double y) => ContextRef.Invoke<bool>("isPointInStroke", path.Path2DRef, x, y);
    public void LineTo(double x, double y) => ContextRef.InvokeVoid("lineTo", x, y);
    public TextMetrics MeasureText(string text) => new(ContextRef.Invoke<IJSInProcessObjectReference>("measureText", text));
    public void MoveTo(double x, double y) => ContextRef.InvokeVoid("moveTo", x, y);
    public void PutImageData(ImageData imageData, double dx, double dy) => ContextRef.InvokeVoid("putImageData", imageData.ImageDataRef, dx, dy);
    public void PutImageData(ImageData imageData, double dx, double dy, double dirtyX, double dirtyY, double dirtyWidth, double dirtyHeight) => ContextRef.InvokeVoid("putImageData", imageData.ImageDataRef, dx, dy, dirtyX, dirtyY, dirtyWidth, dirtyHeight);
    public void QuadraticCurveTo(double cpx, double cpy, double x, double y) => ContextRef.InvokeVoid("quadraticCurveTo", cpx, cpy, x, y);
    public void Rect(double x, double y, double width, double height) => ContextRef.InvokeVoid("rect", x, y, width, height);
    public void ResetTransform() => ContextRef.InvokeVoid("resetTransform");
    public void Restore() => ContextRef.InvokeVoid("restore");
    public void Rotate(double angle) => ContextRef.InvokeVoid("rotate", angle);
    public void RoundRect(double x, double y, double width, double height, double radii) => ContextRef.InvokeVoid("roundRect", x, y, width, height, radii);
    public void RoundRect(double x, double y, double width, double height, double[] radii) => ContextRef.InvokeVoid("roundRect", x, y, width, height, radii);
    public void Save() => ContextRef.InvokeVoid("save");
    public void Scale(double x, double y) => ContextRef.InvokeVoid("scale", x, y);
    public void ScrollPathIntoView() => ContextRef.InvokeVoid("scrollPathIntoView");
    // public void ScrollPathIntoView(Path2D path) => ContextRef.InvokeVoid("scrollPathIntoView", path.Path2DRef);
    public void SetLineDash(double[] segments) => ContextRef.InvokeVoid("setLineDash", segments);
    public void SetTransform(double a, double b, double c, double d, double e, double f) => ContextRef.InvokeVoid("setTransform", a, b, c, d, e, f);
    // public void SetTransform(DOMMatrixInit? matrix = null) => ContextRef.InvokeVoid("setTransform", matrix);
    public void Stroke() => ContextRef.InvokeVoid("stroke");
    // public void Stroke(Path2D path) => ContextRef.InvokeVoid("stroke", path.Path2DRef);
    public void StrokeRect(double x, double y, double width, double height) => ContextRef.InvokeVoid("strokeRect", x, y, width, height);
    public void StrokeText(string text, double x, double y) => ContextRef.InvokeVoid("strokeText", text, x, y);
    public void StrokeText(string text, double x, double y, double maxWidth) => ContextRef.InvokeVoid("strokeText", text, x, y, maxWidth);
    public void Transform(double a, double b, double c, double d, double e, double f) => ContextRef.InvokeVoid("transform", a, b, c, d, e, f);
    public void Translate(double x, double y) => ContextRef.InvokeVoid("translate", x, y);
}

public class TextMetrics(IJSInProcessObjectReference textMetricsRef) {
    public IJSInProcessObjectReference TextMetricsRef { get; } = textMetricsRef;
    public double Width { get => TextMetricsRef.GetProperty<double>("width"); }
    public double ActualBoundingBoxLeft { get => TextMetricsRef.GetProperty<double>("actualBoundingBoxLeft"); }
    public double ActualBoundingBoxRight { get => TextMetricsRef.GetProperty<double>("actualBoundingBoxRight"); }
    public double FontBoundingBoxAscent { get => TextMetricsRef.GetProperty<double>("fontBoundingBoxAscent"); }
    public double FontBoundingBoxDescent { get => TextMetricsRef.GetProperty<double>("fontBoundingBoxDescent"); }
    public double ActualBoundingBoxAscent { get => TextMetricsRef.GetProperty<double>("actualBoundingBoxAscent"); }
    public double ActualBoundingBoxDescent { get => TextMetricsRef.GetProperty<double>("actualBoundingBoxDescent"); }
    public double EmHeightAscent { get => TextMetricsRef.GetProperty<double>("emHeightAscent"); }
    public double EmHeightDescent { get => TextMetricsRef.GetProperty<double>("emHeightDescent"); }
    public double HangingBaseline { get => TextMetricsRef.GetProperty<double>("hangingBaseline"); }
    public double AlphabeticBaseline { get => TextMetricsRef.GetProperty<double>("alphabeticBaseline"); }
    public double IdeographicBaseline { get => TextMetricsRef.GetProperty<double>("ideographicBaseline"); }
}

public class ImageData(IJSInProcessObjectReference imageDataRef) {
    public IJSInProcessObjectReference ImageDataRef { get; } = imageDataRef;
    public int Width { get => ImageDataRef.GetProperty<int>("width"); }
    public int Height { get => ImageDataRef.GetProperty<int>("height"); }
    public Uint8ClampedArray Data { get => new(ImageDataRef.GetProperty<IJSInProcessObjectReference>("data")); }
}

public class Uint8ClampedArray(IJSInProcessObjectReference dataRef) {
    public IJSInProcessObjectReference DataRef { get; } = dataRef;
    public int Length { get => DataRef.GetProperty<int>("length"); }
    public byte this[int index] { get => DataRef.Invoke<byte>("get", index); set => DataRef.InvokeVoid("set", index, value); }
}

public class CanvasGradient(IJSInProcessObjectReference gradientRef) {
    public IJSInProcessObjectReference GradientRef { get; } = gradientRef;
    public void AddColorStop(double offset, string color) => GradientRef.InvokeVoid("addColorStop", offset, color);
}