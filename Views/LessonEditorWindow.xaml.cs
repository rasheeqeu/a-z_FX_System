using System.Windows;
using System.Windows.Input;
using ForexTradingWorkspace.ViewModels;

namespace ForexTradingWorkspace.Views;

public partial class LessonEditorWindow : Window
{
    private readonly LessonEditorViewModel _vm;

    public LessonEditorWindow(LessonEditorViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = vm;
        Loaded += async (_, _) => await vm.InitializeAsync();
        KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
        {
            _vm.SaveCurrentCommand.Execute(null);
            e.Handled = true;
        }
    }
}
