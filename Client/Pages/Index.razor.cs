using GodOfGodField.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace GodOfGodField.Client.Pages;

public partial class Index : ComponentBase {
    static Index? Instance;
    HTMLCanvasElement Canvas = default!;
    HTMLInputElement TextInput = default!;
    double DevicePixelRatio = 1;

    Dictionary<string, IJSInProcessObjectReference> ResourceImages = new();
    bool Update = false;
    string BackgroundImage = "images.screens.home.png";
    double MouseX = 0;
    double MouseY = 0;

    public Index() => Instance = this;

    protected override async Task OnAfterRenderAsync(bool firstRender) {
        if (firstRender) {
            foreach (var name in typeof(Resources).Assembly.GetManifestResourceNames()) {
                if (!name.EndsWith(".png")) continue;
                var url = await JS.CreateObjectURL(new(Resources.GetResource(name)!));
                ResourceImages[name.Replace($"{typeof(Resources).Assembly.GetName().Name}.Resources.", "")] = await JS.CreateImageFromURL(url);
                await JS.RevokeObjectURL(url);
            }
            await ((IJSInProcessRuntime)JS).InvokeVoidAsync("main");
            Canvas = new(((IJSInProcessRuntime)JS).GetElementById("Game"));
            TextInput = new(((IJSInProcessRuntime)JS).GetElementById("TextInput"));
            DevicePixelRatio = ((IJSInProcessRuntime)JS).Invoke<double>("window.getProperty", "devicePixelRatio");
        }
    }

    void Click(MouseEventArgs e) {
        Update = true;
        Console.WriteLine($"X: {e.ClientX}, Y: {e.ClientY}");
        // TextInput.SelectionStart = TextInput.SelectionEnd = TextInput.Value.Length;
        // TextInput.Focus();
    }

    void Input(ChangeEventArgs e) {
        Update = true;
        Console.WriteLine(e.Value);
    }

    void Select(EventArgs e) {
        Update = true;
    }

    void MouseMove(MouseEventArgs e) {
        Update = true;
        MouseX = e.ClientX * DevicePixelRatio;
        MouseY = e.ClientY * DevicePixelRatio;
    }

    [JSInvokable]
    public static Task? SelectionChange() => Instance?.SelectionChangeInstance();

    async Task SelectionChangeInstance() {
        Update = true;
        await Task.CompletedTask;
    }

    [JSInvokable]
    public static Task? Render(decimal time, bool resized, decimal renderTime, decimal fps) => Instance?.RenderInstance(time, resized, renderTime, fps);

    async Task RenderInstance(decimal time, bool resized, decimal renderTime, decimal fps) {
        if (!Update && !resized) return;
        Update = false;
        var ctx = Canvas.GetContext2D();
        ctx.ClearRect(0, 0, Canvas.Width, Canvas.Height);
        ctx.FillStyle = "rgb(0, 143, 111)";
        ctx.FillRect(0, 0, Canvas.Width, 30);
        ctx.FillRect(0, Canvas.Height - 30, Canvas.Width, Canvas.Height);

        var backgroundImage = ResourceImages[BackgroundImage];
        var b = backgroundImage.GetProperty<double>("width") <= Canvas.Width;
        ctx.DrawImage(backgroundImage, 0, 30, Canvas.Width, Canvas.Height - 60);

        // context.FillStyle = "red";
        // context.Font = "24px Arial";
        // context.FillText($"Hello, World!", (Canvas.Width - context.MeasureText($"Hello, World!").Width) / 2, Canvas.Height / 2);

        await RenderIndex(ctx);

        // draw center vertical and horizontal lines
        ctx.StrokeStyle = "red";
        ctx.LineWidth = 1;
        ctx.BeginPath();
        ctx.MoveTo(Canvas.Width / 2, 0);
        ctx.LineTo(Canvas.Width / 2, Canvas.Height);
        ctx.Stroke();
        ctx.BeginPath();
        ctx.MoveTo(0, Canvas.Height / 2);
        ctx.LineTo(Canvas.Width, Canvas.Height / 2);
        ctx.Stroke();

        // draw vertical and horizontal lines for mouse position
        ctx.StrokeStyle = "blue";
        ctx.LineWidth = 1;
        ctx.BeginPath();
        ctx.MoveTo(MouseX, 0);
        ctx.LineTo(MouseX, Canvas.Height);
        ctx.Stroke();
        ctx.BeginPath();
        ctx.MoveTo(0, MouseY);
        ctx.LineTo(Canvas.Width, MouseY);
        ctx.Stroke();

        DrawBoldText(ctx, $"FPS: {fps}, Render: {renderTime}ms", 10, 20, 10);
    }

    async Task RenderIndex(CanvasRenderingContext2D ctx) {
        var userName = L["texts.home.userName"];
        ctx.Font = "bold 30px sans-serif";
        var xStart = Canvas.Width * 0.5 - ctx.MeasureText(userName).Width;
        var yStart = (Canvas.Height - 30 * 2) / 2;
        DrawBoldText(ctx, userName, xStart, yStart, 30);

        // DrawRoundedBox(ctx, xStart, yStart + 30, 200, 34, "rgb(238, 255, 170)", "rgb(136, 153, 85)", 2, 5);
        // if (TextInput.SelectionStart != TextInput.SelectionEnd) {
        //     // TODO draw a blue box around the selected text
        //     var selectionStart = xStart + 5 + ctx.MeasureText(TextInput.Value.Substring(0, TextInput.SelectionStart)).Width;
        //     var selectionEnd = xStart + 5 + ctx.MeasureText(TextInput.Value.Substring(0, TextInput.SelectionEnd)).Width;
        //     DrawBox(ctx, selectionStart, yStart + 30, selectionEnd - selectionStart, 34, "rgba(0, 0, 255, 0.3)");
        // }
        // DrawText(ctx, TextInput.Value, xStart + 5, yStart + 55, 30);
        ResetCSS(TextInput);
        TextInput.Style["z-index"] = "1";
        TextInput.Style["width"] = $"{ctx.MeasureText(userName).Width * 2 / DevicePixelRatio}px";
        TextInput.Style["height"] = $"{34 / DevicePixelRatio}px";
        TextInput.Style["font-size"] = $"{25 / DevicePixelRatio}px";
        TextInput.Style["border"] = "2px solid rgb(136, 153, 85)";
        TextInput.Style["border-radius"] = "5px";
        TextInput.Style["background-color"] = "rgb(238, 255, 170)";
        TextInput.Style["color"] = "rgb(0, 143, 111)";
        TextInput.Style["outline"] = "none";
        TextInput.Style["position"] = "absolute";
        TextInput.Style["top"] = $"{(yStart += 30) / DevicePixelRatio}px";
        TextInput.Style["left"] = $"{xStart / DevicePixelRatio}px";

        var bMouseOverBox = MouseX >= xStart && MouseX <= xStart + ctx.MeasureText(userName).Width && MouseY >= yStart && MouseY <= yStart + 30;
        // Console.WriteLine($"MouseX: {MouseX}, MouseY: {MouseY}, xStart: {xStart}, yStart: {yStart}, bMouseOverBox: {bMouseOverBox}");
        DrawRoundedBox2(ctx, L["texts.home.setUserName"], Canvas.Width * 0.5 - Canvas.Width * 0.2, yStart += 70, Canvas.Width * 0.4, 90, MixWithWhite("rgb(0, 143, 111)", bMouseOverBox ? 0.2 : 0), "rgb(238, 255, 238)", 2, 25);
        ctx.Font = "bold 60px sans-serif";
        DrawBoldText2(ctx, L["texts.home.setUserName"], Canvas.Width * 0.5 - ctx.MeasureText(L["texts.home.setUserName"]).Width / 2, yStart + 65, 60);
    }

    void ResetCSS(HTMLElement element) => element.Style.StyleRef.SetProperty("cssText", "");

    string MixWithWhite(string color, double weight) {
        var colorParts = color.Split("(", 2)[1].Split(")")[0].Split(",");
        var red = int.Parse(colorParts[0]);
        var green = int.Parse(colorParts[1]);
        var blue = int.Parse(colorParts[2]);
        var white = 255;
        var red2 = (int)((white - red) * weight + red);
        var green2 = (int)((white - green) * weight + green);
        var blue2 = (int)((white - blue) * weight + blue);
        return $"rgb({red2}, {green2}, {blue2})";
    }

    void DrawText(CanvasRenderingContext2D ctx, string text, double x, double y, double size, string color = "rgb(0, 143, 111)") {
        ctx.Font = $"{size}px sans-serif";
        ctx.FillStyle = color;
        ctx.FillText(text, x, y);
    }

    void DrawBoldText(CanvasRenderingContext2D ctx, string text, double x, double y, double size, string color = "rgb(0, 143, 111)", string strokeColor = "rgb(255, 255, 221)", double lineWidth = 4) {
        ctx.Font = $"bold {size}px sans-serif";
        ctx.LineWidth = lineWidth;
        ctx.LineJoin = "miter";
        ctx.MiterLimit = 4;
        ctx.StrokeStyle = strokeColor;
        ctx.StrokeText(text, x, y);
        ctx.FillStyle = color;
        ctx.FillText(text, x, y);
    }

    void DrawBoldText2(CanvasRenderingContext2D ctx, string text, double x, double y, double size, string color = "rgb(238, 255, 238)", double lineWidth = 4) {
        ctx.Font = $"bold {size}px sans-serif";
        ctx.LineWidth = lineWidth;
        ctx.LineJoin = "miter";
        ctx.MiterLimit = 4;
        ctx.FillStyle = color;
        ctx.FillText(text, x, y);
    }


    void DrawBox(CanvasRenderingContext2D ctx, double x, double y, double width, double height, string backgroundColor) {
        ctx.FillStyle = backgroundColor;
        ctx.FillRect(x, y, width, height);
    }

    void DrawRoundedBox(CanvasRenderingContext2D ctx, double x, double y, double width, double height, string backgroundColor, string borderColor, double borderWidth, double borderRadius) {
        ctx.FillStyle = backgroundColor;
        ctx.StrokeStyle = borderColor;
        ctx.LineWidth = borderWidth;
        ctx.RoundRect(x, y, width, height, borderRadius);
        ctx.Fill();
        ctx.Stroke();
    }

    void DrawRoundedBox2(CanvasRenderingContext2D ctx, string text, double x, double y, double width, double height, string backgroundColor, string borderColor, double borderWidth, double borderRadius) {
        ctx.FillStyle = backgroundColor;
        ctx.StrokeStyle = borderColor;
        ctx.LineWidth = borderWidth;
        ctx.RoundRect(x, y, width, height, borderRadius);
        ctx.Stroke();
        ctx.Fill();
        ctx.RoundRect(x + borderWidth * 2, y + borderWidth * 2, width - borderWidth * 4, height - borderWidth * 4, borderRadius - borderWidth * 2);
        ctx.Stroke();
    }
}