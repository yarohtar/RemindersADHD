using RemindersADHD.MVVM.ViewModels;

namespace RemindersADHD.MVVM.Views;

public partial class Pomodoro : ContentPage
{
    PomodoroViewModel vm = new();
    public Pomodoro()
    {
        InitializeComponent();
        BindingContext = vm;

    }


    protected override async void OnAppearing()
    {
        vm.Initialize();
    }
}