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
        TextInput.SelectionStart = TextInput.SelectionEnd = TextInput.Value.Length;
        TextInput.Focus();
    }

    void Input(ChangeEventArgs e) {
        Update = true;
        Console.WriteLine(e.Value);
    }

    void Select(EventArgs e) {
        Update = true;
        Console.WriteLine("Selected");
    }

    [JSInvokable]
    public static Task? SelectionChange() => Instance?.SelectionChangeInstance();

    async Task SelectionChangeInstance() {
        Update = true;
        await Task.CompletedTask;
    }

    [JSInvokable]
    public static Task? Render(decimal time, bool resized) => Instance?.RenderInstance(time, resized);

    async Task RenderInstance(decimal time, bool resized) {
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

        DrawBoldText(ctx, time.ToString(), 10, 20, 10);
    }

    async Task RenderIndex(CanvasRenderingContext2D ctx) {
        var userName = L["texts.home.userName"];
        var xStart = (Canvas.Width * 0.5 - ctx.MeasureText(userName).Width) / DevicePixelRatio;
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
        TextInput.Style["z-index"] = "1";
        TextInput.Style["width"] = $"{ctx.MeasureText(userName).Width * 2 / DevicePixelRatio}px";
        TextInput.Style["height"] = $"{34 / DevicePixelRatio}px";
        TextInput.Style["font-size"] = $"{25 / DevicePixelRatio}px";
        TextInput.Style["border"] = "2px solid rgb(136, 153, 85)";
        TextInput.Style["border-radius"] = "5px";
        TextInput.Style["background-color"] = "rgb(238, 255, 170)";
        TextInput.Style["color"] = "rgb(0, 143, 111)";
        TextInput.Style["top"] = $"{(yStart + 30) / DevicePixelRatio}px";
        TextInput.Style["left"] = $"{xStart / DevicePixelRatio}px";
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
}