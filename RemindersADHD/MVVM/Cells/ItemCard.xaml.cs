using Microsoft.Maui.Layouts;
using RemindersADHD.MVVM.Models;
using System.ComponentModel;
using System.Windows.Input;

namespace RemindersADHD.MVVM.Cells;

public partial class ItemCard : ContentView
{
	public ItemCard()
	{
		InitializeComponent();
    }


    //public ICommand NavigateToEditItemCommand => new Command(async () => await Navigate());
	public ICommand SwitchMoreCommand => new Command(async () => await SwitchMore());

    private static bool _isBusy = false;
	private async Task Navigate()
	{
        if (_isBusy) { return;  }
        _isBusy = true;
        await Shell.Current.GoToAsync("itemdetails", new Dictionary<string, object> { {"item", Item.Kind } });
        await Task.Delay(500);
        _isBusy = false;
	}
	private async Task SwitchMore()
	{
		Item.Kind.SubItemsVisible ^= true;
    }


    private async void AnimateButton(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ItemKind item) return;

        if (e.PropertyName == nameof(item.SubItemsVisible))
        {
            if (item.SubItemsVisible)
                await MoreButton.RotateTo(180, 150);
            else
            {
                if (MoreButton.Rotation < 180)
                    await MoreButton.RotateTo(0, 200);
                else
                {
                    await MoreButton.RotateTo(360, 200);
                    MoreButton.Rotation = 0;
                }
            }
        }
    }

    #region OnPropertyChanged
    private static async void OnItemPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
    {
        if (bindable is not ItemCard itemCard) throw new Exception();
        if (newvalue is null) return;

        if(newvalue is not ICardBindable item) throw new Exception();

        if (item.Kind.SubItemsVisible)
            itemCard.MoreButton.Rotation = 180;
        else
            itemCard.MoreButton.Rotation = 0;

        if (oldvalue is ICardBindable olditem) olditem.Kind.PropertyChanged -= itemCard.AnimateButton;
        item.Kind.PropertyChanged += itemCard.AnimateButton;
    }
    #endregion

    #region Bindable Properties
    public static readonly BindableProperty ItemProperty =
        BindableProperty.Create(nameof(Item), typeof(ICardBindable), typeof(ItemCard), propertyChanged: OnItemPropertyChanged);

    public ICardBindable Item
    {
        get => (ICardBindable)GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    public static readonly BindableProperty CheckChangedCommandProperty =
        BindableProperty.Create(nameof(CheckChangedCommand), typeof(ICommand), typeof(ItemCard));
    public ICommand CheckChangedCommand
    {
        get=> (ICommand)GetValue(CheckChangedCommandProperty);
        set=> SetValue(CheckChangedCommandProperty, value);
    }

    public static readonly BindableProperty CheckChangedCommandParameterProperty =
        BindableProperty.Create(nameof(CheckChangedCommandParameter), typeof(object), typeof(ItemCard));
    public object CheckChangedCommandParameter
    {
        get=> GetValue(CheckChangedCommandParameterProperty);
        set=> SetValue(CheckChangedCommandParameterProperty, value);
    }

    public static readonly BindableProperty TapCommandProperty = 
        BindableProperty.Create(nameof(TapCommand), typeof(ICommand), typeof(ItemCard));
    public ICommand TapCommand
    {
        get => (ICommand)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    public static readonly BindableProperty TapCommandParameterProperty =
        BindableProperty.Create(nameof(TapCommandParameter), typeof(object), typeof(ItemCard));
    public object TapCommandParameter
    {
        get => GetValue(TapCommandParameterProperty);
        set => SetValue(TapCommandParameterProperty, value);
    }
    #endregion
}