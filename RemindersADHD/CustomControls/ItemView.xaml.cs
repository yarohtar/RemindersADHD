using RemindersADHD.MVVM.Models;
using System.ComponentModel;
using System.Windows.Input;

namespace RemindersADHD.CustomControls;

public partial class ItemView : ContentView
{
    public ItemView()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        //(BindingContext as HabitOnDay).PropertyChanged += OnCheckedChanged;
    }
    private void OnCheckedChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not HabitOnDay h) return;
        if (e.PropertyName == nameof(h.Done))
        {
            Checkbox.IsEnabled = h.Done;
        }
    }

    #region BindableProperties
    public static readonly BindableProperty ItemNameProperty =
        BindableProperty.Create(nameof(ItemName), typeof(string), typeof(ItemView), null);
    public string ItemName
    {
        get => (string)GetValue(ItemNameProperty);
        set => SetValue(ItemNameProperty, value);
    }

    public static readonly BindableProperty HasCheckboxProperty =
        BindableProperty.Create(nameof(HasCheckbox), typeof(bool), typeof(ItemView), true);
    public bool HasCheckbox
    {
        get => (bool)GetValue(HasCheckboxProperty);
        set => SetValue(HasCheckboxProperty, value);
    }

    public static readonly BindableProperty IsCheckedProperty =
        BindableProperty.Create(
            nameof(IsChecked),
            typeof(bool),
            typeof(ItemView),
            defaultBindingMode: BindingMode.TwoWay);
    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public static readonly BindableProperty CheckedCommandProperty =
        BindableProperty.Create(nameof(CheckedCommand), typeof(ICommand), typeof(ItemView), null);
    public ICommand CheckedCommand
    {
        get => (ICommand)GetValue(CheckedCommandProperty);
        set => SetValue(CheckedCommandProperty, value);
    }
    public static readonly BindableProperty CheckedCommandParameterProperty =
        BindableProperty.Create(nameof(CheckedCommandParameter), typeof(object), typeof(ItemView), null);
    public object CheckedCommandParameter
    {
        get => GetValue(CheckedCommandParameterProperty);
        set => SetValue(CheckedCommandParameterProperty, value);
    }

    public static readonly BindableProperty TappedCommandProperty =
        BindableProperty.Create(nameof(TappedCommand), typeof(ICommand), typeof(ItemView), null);
    public ICommand TappedCommand
    {
        get => (ICommand)GetValue(TappedCommandProperty);
        set => SetValue(TappedCommandProperty, value);
    }

    public static readonly BindableProperty TappedCommandParameterProperty =
        BindableProperty.Create(nameof(TappedCommandParameter), typeof(object), typeof(ItemView), null);
    public object TappedCommandParameter
    {
        get => GetValue(TappedCommandParameterProperty);
        set => SetValue(TappedCommandParameterProperty, value);
    }
    #endregion
}