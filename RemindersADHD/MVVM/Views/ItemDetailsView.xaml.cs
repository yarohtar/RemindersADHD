using RemindersADHD.MVVM.ViewModels;

namespace RemindersADHD.MVVM.Views;

public partial class ItemDetailsView : ContentPage
{
	public ItemDetailsView()
	{
		InitializeComponent();
		BindingContext = new ItemDetailsViewModel();
	}
}