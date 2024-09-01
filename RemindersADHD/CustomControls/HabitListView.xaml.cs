using RemindersADHD.MVVM.Models;
using System.Windows.Input;

namespace RemindersADHD.CustomControls;

public partial class HabitListView : ContentView
{
    public HabitListView()
    {
        InitializeComponent();
    }

    #region Bindable Properties

    public static readonly BindableProperty HabitListProperty =
        BindableProperty.Create(nameof(HabitList), typeof(IEnumerable<HabitOnDay>), typeof(HabitListView), null);
    public IEnumerable<HabitOnDay> HabitList
    {
        get => (IEnumerable<HabitOnDay>)GetValue(HabitListProperty);
        set => SetValue(HabitListProperty, value);
    }

    public static readonly BindableProperty HasCheckboxProperty =
        BindableProperty.Create(nameof(HasCheckbox), typeof(bool), typeof(HabitListView), true);
    public bool HasCheckbox
    {
        get => (bool)GetValue(HasCheckboxProperty);
        set => SetValue(HasCheckboxProperty, value);
    }

    public static readonly BindableProperty CheckedCommandProperty =
        BindableProperty.Create(nameof(CheckedCommand), typeof(ICommand), typeof(HabitListView), null);
    public ICommand CheckedCommand
    {
        get => (ICommand)GetValue(CheckedCommandProperty);
        set => SetValue(CheckedCommandProperty, value);
    }

    public static readonly BindableProperty TappedCommandProperty =
        BindableProperty.Create(nameof(TappedCommand), typeof(ICommand), typeof(HabitListView), null);
    public ICommand TappedCommand
    {
        get => (ICommand)GetValue(TappedCommandProperty);
        set => SetValue(TappedCommandProperty, value);
    }

    #endregion
}