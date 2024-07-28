using System.Windows.Input;

namespace RemindersADHD.CustomControls;

public partial class TestControlsView : ContentPage
{
	public TestControlsView()
	{
		InitializeComponent();
    }
    private bool _isRunning = false;
    public bool IsRunning { get => _isRunning; set { _isRunning = value; OnPropertyChanged(); } }

    public ICommand Click => new Command(async () => { long m = TimerControl.Milliseconds;  await TimerControl.NumberTo(m, m+10000, l => TimerControl.Milliseconds = l, length: 400); });
}