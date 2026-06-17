using System.Windows;
using System.Windows.Controls;

namespace ForexTradingWorkspace.Views;

public sealed class LabeledContentControl : ContentControl
{
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(LabeledContentControl), new PropertyMetadata(""));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }
}
