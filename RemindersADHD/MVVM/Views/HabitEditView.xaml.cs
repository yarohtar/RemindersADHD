using RemindersADHD.MVVM.ViewModels;

namespace RemindersADHD.MVVM.Views;

public partial class HabitEditView : ContentPage
{
    public HabitEditView()
    {
        InitializeComponent();
        BindingContext = new HabitEditViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as HabitEditViewModel)!.Initialize();
    }
}