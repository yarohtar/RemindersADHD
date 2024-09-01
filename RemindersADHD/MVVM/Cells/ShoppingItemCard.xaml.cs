using RemindersADHD.MVVM.Models;
using RemindersADHD.MVVM.ViewModels;
using System.ComponentModel;

namespace RemindersADHD.MVVM.Cells;

public partial class ShoppingItemCard : ContentView
{
    public ShoppingItemCard()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty ParentViewModelProperty =
        BindableProperty.Create(nameof(ParentViewModel), typeof(ShoppingListViewModel), typeof(ShoppingItemCard), null);

    public ShoppingListViewModel ParentViewModel
    {
        get => (ShoppingListViewModel)GetValue(ParentViewModelProperty);
        set
        {
            SetValue(ParentViewModelProperty, value);
        }
    }
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (BindingContext is null) return;
        (BindingContext as ShoppingItem)!.PropertyChanged += AnimateButton;
    }
    private async void AnimateButton(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ShoppingItem item) return;

        if (e.PropertyName == nameof(item.HasNote))
        {
            if (item.HasNote)
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
}