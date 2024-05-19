using Microsoft.JSInterop;

namespace GodOfGodField.Client;

public class HTMLInputElement(IJSInProcessObjectReference elementRef) : HTMLElement(elementRef) {
    public string Value { get => ElementRef.GetProperty<string>("value"); set => ElementRef.SetProperty("value", value); }
    public int SelectionStart { get => ElementRef.GetProperty<int>("selectionStart"); set => ElementRef.SetProperty("selectionStart", value); }
    public int SelectionEnd { get => ElementRef.GetProperty<int>("selectionEnd"); set => ElementRef.SetProperty("selectionEnd", value); }
}