using RemindersADHD.MVVM.ViewModels;

namespace RemindersADHD.MVVM.Views;

public partial class ShoppingItemEditView : ContentPage
{
	public ShoppingItemEditView()
	{
		InitializeComponent();
		BindingContext = new ShoppingItemEditViewModel();
	}

	protected override void OnAppearing()
	{
		(BindingContext as ShoppingItemEditViewModel)!.Initialize();
	}
}