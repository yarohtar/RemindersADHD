using RemindersADHD.MVVM.Models;
using System.Windows.Input;

namespace RemindersADHD.CustomControls;

public partial class TestControlsView : ContentPage
{
    public TestControlsView()
    {
        InitializeComponent();
        Item.SubItems.Add(new ItemWithDate(new ItemKind(new Tracker()) { Title = "subitem" }));
    }
    private bool _isRunning = false;
    public bool IsRunning { get => _isRunning; set { _isRunning = value; OnPropertyChanged(); } }

    public ICommand Click => new Command(async () =>
    {
        long m = TimerControl.Milliseconds;
        await TimerControl.NumberTo(m, m + 10000, l => TimerControl.Milliseconds = l, length: 400);
    });

    public ICardBindable Item { get; set; } = new ItemKind(new Tracker()) { Title = "item" }.GetItemNoDate();


}