using RemindersADHD.MVVM.ViewModels;

namespace RemindersADHD.MVVM.Views;

public partial class ShoppingListView : ContentPage
{
	public ShoppingListView()
	{
		InitializeComponent();
		BindingContext = new ShoppingListViewModel();
	}

	override protected async void OnAppearing()
	{
		base.OnAppearing();
		await (BindingContext as ShoppingListViewModel)!.Initialize();
	}
}