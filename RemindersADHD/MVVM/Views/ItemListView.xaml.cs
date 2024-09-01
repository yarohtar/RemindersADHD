using RemindersADHD.MVVM.ViewModels;

namespace RemindersADHD.MVVM.Views;

public partial class ItemListView : ContentPage
{
	ListViewModel vm;
	public ItemListView()
	{
		InitializeComponent();
		vm = new ListViewModel();
		BindingContext = vm;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await vm.Initialize();
	}
}