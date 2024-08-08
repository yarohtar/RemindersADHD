using System.Linq.Expressions;
using System.Windows.Input;

namespace RemindersADHD.CustomControls;

public partial class Timer : ContentView
{
    public enum TimerMode { 
        /// <summary>
        /// Timer runs forwards
        /// </summary>
        Basic,
        /// <summary>
        /// Timer runs backwards
        /// </summary>
        BasicBackward,
        /// <summary>
        /// Timer runs forwards and raises <see cref="TimeHit"/> event when clock goes over <see cref="MillisecondsToHit"/>
        /// </summary>
        ForwardTo,
        /// <summary>
        /// Timer runs backwards and raises <see cref="TimeHit"/> event when clock goes under <see cref="MillisecondsToHit"/>
        /// </summary>
        BackwardTo,
        /// <summary>
        /// Timer runs forwards and raises <see cref="TimeHit"/> event when clock goes over <see cref="MillisecondsToHit"/>, then pauses
        /// </summary>
        ForwardUntil,
        /// <summary>
        /// Timer runs backwards and raises <see cref="TimeHit"/> event when clock goes under <see cref="MillisecondsToHit"/>, then pauses
        /// </summary>
        BackwardUntil
    }

	public Timer()
	{
		InitializeComponent();
	}
    
    private DateTime _startTime = DateTime.Now;
    private long _ticksStarted = 0;
    private bool _internalMillisecondsChange = false;
    /// <summary>
    /// stops the event from being raised multiple times
    /// </summary>
    private bool _eventRaised = false;
    private Task _refresher = Task.CompletedTask;

    private long TicksPassed => DateTime.Now.Ticks - _startTime.Ticks;
    private long MillisecondsOnClock =>
        !IsRunning ?
        _ticksStarted / 10000 :
        IsForwards ?
        (_ticksStarted + TicksPassed) / 10000 :
        (_ticksStarted - TicksPassed) / 10000;

    private bool IsForwards => Mode == TimerMode.Basic || Mode == TimerMode.ForwardTo || Mode == TimerMode.ForwardUntil;
    private bool IsBackwards => Mode == TimerMode.BasicBackward || Mode == TimerMode.BackwardTo || Mode == TimerMode.BackwardUntil;
    private bool HasEvent => !(Mode == TimerMode.Basic || Mode == TimerMode.BasicBackward);

    public bool HasMinus => MillisecondsOnClock + AdjustForRounding < 0;
    private long AdjustForRounding => IsForwards ? 0 : 999;
    public string Minutes => Math.Abs((int)Math.Floor((MillisecondsOnClock + AdjustForRounding) / 1000.0) / 60).ToString("00");
    public string Seconds => Math.Abs((int)Math.Floor((MillisecondsOnClock + AdjustForRounding) / 1000.0) % 60).ToString("00");

    public event TimeHitEventHandler? TimeHit;

    public void StartPause()
    {
        if (IsRunning) { IsRunning = false; return; }
        IsRunning = true;
    }

    private async Task Start()
    {
        _startTime = DateTime.Now;
        await _refresher;
        _refresher = Refresher();
        await _refresher;
    }

    private async void Pause()
    {
        if (IsForwards)
            _ticksStarted += TicksPassed;
        else
            _ticksStarted -= TicksPassed;
        await _refresher;
        _refresher.Dispose();
        Refresh();
    }

    private async Task Refresher()
    {
        while (IsRunning)
        {
            await Task.Delay(12);
            Refresh();
        }
    }
    private void Refresh()
    {
        CheckTimerHit();
        _internalMillisecondsChange = true;
        Milliseconds = MillisecondsOnClock;
        _internalMillisecondsChange = false;
        RefreshUI();
    }
    private void RefreshUI()
    {
        OnPropertyChanged(nameof(Minutes));
        OnPropertyChanged(nameof(Seconds));
        OnPropertyChanged(nameof(HasMinus));
    }
    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        //StartPause();
    }

    private void CallTimeHit()
    {
        TimeHit?.Invoke(this, new TimeHitEventArgs { Mode = this.Mode, MillisecondsOnClock = this.MillisecondsOnClock });
        _eventRaised = true;
        TimeHitCommand.Execute(TimeHitCommandParameter);
    }
    /// <remarks>
    /// This function will not Refresh!!
    /// </remarks>
    private void CheckTimerHit()
    {
        if (_eventRaised)
            return;
        switch(Mode)
        {
            case TimerMode.Basic:
            case TimerMode.BasicBackward:
                return;
            case TimerMode.ForwardTo:
                if(MillisecondsOnClock >= MillisecondsToHit)
                {
                    CallTimeHit();
                }
                break;
            case TimerMode.ForwardUntil:
                if(MillisecondsOnClock >= MillisecondsToHit)
                {
                    //pause it
                    IsRunning = false;
                    _ticksStarted = MillisecondsToHit;
                    _startTime = DateTime.Now;

                    CallTimeHit();
                }
                break;
            case TimerMode.BackwardTo:
                if (MillisecondsOnClock <= MillisecondsToHit)
                {
                    CallTimeHit();
                }
                break;
            case TimerMode.BackwardUntil:
                if (MillisecondsOnClock <= MillisecondsToHit)
                {
                    //pause it
                    IsRunning = false;
                    _ticksStarted = MillisecondsToHit;
                    _startTime = DateTime.Now;

                    CallTimeHit();
                }
                break;

            default:
                return;
        }
    }

    #region On Property Events
    private static async void OnIsRunningChanged(BindableObject bindable, object oldvalue, object newvalue)
    {
        if (bindable is not Timer timer) return;
        if (oldvalue is not bool old) return;
        if (newvalue is not bool n) return;
        if (old == n) return;
        if (n) await timer.Start();
        else timer.Pause();
    }
    private static void OnMillisecondChanged(BindableObject bindable, object oldvalue, object newvalue)
    {
        if (bindable is not Timer timer) return;
        if (timer._internalMillisecondsChange) return;
        if (oldvalue is not long old) return;
        if (newvalue is not long n) return;
        if (old == n) return;
        timer._ticksStarted = n * 10000;
        timer._startTime = DateTime.Now;
        timer.RefreshUI();
        timer._eventRaised = false;
    }
    private static void OnModeChanged(BindableObject bindable, object oldvalue, object newvalue)
    {
        if(bindable is not Timer timer) return;
        timer._eventRaised = false;
    }
    private static void OnMillisecondsToHitChanged(BindableObject bindable, object oldvalue, object newvalue)
    {
        if (bindable is not Timer timer) return;
        timer._eventRaised = false;
    }

    #endregion

    #region Bindable Properties

    public static readonly BindableProperty IsRunningProperty =
        BindableProperty.Create(nameof(IsRunning), typeof(bool), typeof(Timer), false,
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: OnIsRunningChanged);
    public bool IsRunning
    {
        get => (bool)GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, value);
    }
    public static readonly BindableProperty MillisecondsProperty =
        BindableProperty.Create(nameof(Milliseconds), typeof(long), typeof(Timer), (long)0,
            defaultBindingMode: BindingMode.OneWayToSource,
            propertyChanged: OnMillisecondChanged);
    public long Milliseconds
    {
        get => (long)GetValue(MillisecondsProperty);
        set => SetValue(MillisecondsProperty, value);
    }

    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(nameof(FontSize), typeof(short), typeof(Timer), (short)70, 
            BindingMode.OneWay);
    public short FontSize
    {
        get=> (short)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public static readonly BindableProperty MillisecondsToHitProperty =
        BindableProperty.Create(nameof(MillisecondsToHit), typeof(long), typeof(Timer),
            defaultValue: (long)0,
            propertyChanged: OnMillisecondsToHitChanged);
    public long MillisecondsToHit
    {
        get => (long)GetValue(MillisecondsToHitProperty);
        set => SetValue(MillisecondsToHitProperty, value);
    }

    public static readonly BindableProperty ModeProperty =
        BindableProperty.Create(nameof(Mode), typeof(TimerMode), typeof(Timer),
            defaultValue: TimerMode.Basic,
            propertyChanged: OnModeChanged);
    public TimerMode Mode { 
        get => (TimerMode)GetValue(ModeProperty);
        set => SetValue(ModeProperty, value); 
    }

    public static readonly BindableProperty TimeHitCommandProperty =
        BindableProperty.Create(nameof(TimeHitCommand), typeof(ICommand), typeof(Timer),
            defaultValue: new Command((object? e) => { }));
    public ICommand TimeHitCommand
    {
        get => (ICommand)GetValue(TimeHitCommandProperty);
        set => SetValue(TimeHitCommandProperty, value);
    }

    public static readonly BindableProperty TimeHitCommandParameterProperty =
        BindableProperty.Create(nameof(TimeHitCommandParameter), typeof(object), typeof(Timer),
            defaultValue: null);
    public object TimeHitCommandParameter
    {
        get=> GetValue(TimeHitCommandParameterProperty);
        set => SetValue(TimeHitCommandParameterProperty, value);
    }
    #endregion
}

public delegate void TimeHitEventHandler(object sender, TimeHitEventArgs e);
public class TimeHitEventArgs : EventArgs
{
    public Timer.TimerMode Mode { get; set; }
    public long MillisecondsOnClock { get; set; }
}