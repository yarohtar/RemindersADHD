using RemindersADHD.MVVM.ViewModels;

namespace RemindersADHD.MVVM.Views;

public partial class HabitsView : ContentPage
{
    HabitsViewModel? ViewModel => BindingContext as HabitsViewModel;
    public HabitsView()
    {
        InitializeComponent();
        var b = new HabitsViewModel();
        BindingContext = b;
        b.DateChanging = new Func<ChangingToDay, Task>(DateChangingHandler);
        b.ListUpdated = new Func<Task>(ResetControls);
    }
    override protected async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel!.Initialize();
    }

    #region Animate Changing Date
    private async Task DateChangingHandler(ChangingToDay changingToDay)
    {
        switch (changingToDay)
        {
            case ChangingToDay.Next:
                await AnimateNext();
                break;
            case ChangingToDay.Previous:
                await AnimatePrevious();
                break;
            case ChangingToDay.Same:
                await AnimateBack();
                break;
            case ChangingToDay.Future:
                await AnimateBack();
                break;
            default:
                break;
        }
    }
    private async Task AnimateBack()
    {
        double width = OriginalList.Width;
        Task t1, t2;
        if (FakeList.TranslationX > 0)
        {
            t1 = FakeList.TranslateTo(width, 0, 400, Easing.CubicOut);
        }
        else
            t1 = FakeList.TranslateTo(-width, 0, 400, Easing.CubicOut);
        t2 = OriginalList.TranslateTo(0, 0, 400, Easing.CubicOut);
        await Task.WhenAll(t1, t2);
    }
    private async Task AnimateNext()
    {
        ViewModel?.SetHelperNext();
        double width = OriginalList.Width;
        FakeList.TranslationX = OriginalList.TranslationX + width;
        //FakeList.SetBinding(ItemsView.ItemsSourceProperty, "HelperHabits");
        var t1 = FakeList.TranslateTo(0, 0, 400, Easing.CubicInOut);
        var t2 = OriginalList.TranslateTo(-width, 0, 400, Easing.CubicInOut);
        await Task.WhenAll(t1, t2);
    }
    private async Task AnimatePrevious()
    {
        ViewModel?.SetHelperPrev();
        double width = OriginalList.Width;
        FakeList.TranslationX = OriginalList.TranslationX - width;
        var t1 = FakeList.TranslateTo(0, 0, 400, Easing.CubicInOut);
        var t2 = OriginalList.TranslateTo(width, 0, 400, Easing.CubicInOut);
        await Task.WhenAll(t1, t2);
    }
    private async Task ResetControls()
    {
        OriginalList.TranslationX = 0;
        FakeList.TranslationX = 0;
    }
    private bool isPanning = false;
    private int panTo = 0;
    private int previousXSign = 0;
    public async void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        double width = OriginalList.Width;
        switch (e.StatusType)
        {
            default:
            case GestureStatus.Running:
                if (Math.Abs(e.TotalX) > Math.Abs(e.TotalY) && Math.Abs(e.TotalX) > width / 200)
                    isPanning = true;
                if (!isPanning)
                    return;
                if (e.TotalX > 0)
                {
                    if (e.TotalX * previousXSign <= 0)
                    {
                        ViewModel?.SetHelperPrev();
                        previousXSign = 1;
                    }
                    OriginalList.TranslationX = e.TotalX;
                    FakeList.TranslationX = -width + e.TotalX;
                }
                else if (e.TotalX < 0)
                {
                    if (e.TotalX * previousXSign <= 0)
                    {
                        ViewModel?.SetHelperNext();
                        previousXSign = -1;
                    }
                    OriginalList.TranslationX = e.TotalX;
                    FakeList.TranslationX = width + e.TotalX;
                }
                else
                    previousXSign = 0;
                if (e.TotalX > width / 3)
                    panTo = 1;
                else if (e.TotalX < -width / 3)
                    panTo = -1;
                else
                    panTo = 0;
                break;
            case GestureStatus.Completed:
                previousXSign = 0;
                if (!isPanning)
                    return;
                isPanning = false;
                if (BindingContext is not HabitsViewModel vm)
                    return;
                if (panTo == 1)
                    vm.PrevDateCommand.Execute(null);
                else if (panTo == -1)
                    vm.NextDateCommand.Execute(null);
                else
                    await AnimateBack();
                break;
        }
    }
    #endregion
}