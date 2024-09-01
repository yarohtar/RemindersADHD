using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RemindersADHD.MVVM.ViewModels
{
    public class Session : INotifyPropertyChanged
    {
        public enum SessionMode { Study, Break, LongBreak }
        public static Dictionary<SessionMode, long> Lengths = 
            new() { 
            { SessionMode.Study, 1500000 },
            { SessionMode.Break, 300000 }, 
            {SessionMode.LongBreak, 1800000 } };
        public enum SessionState { Finished, Current, Future }
        public SessionMode Mode { get; set; }
        private SessionState _state;
        public SessionState State
        {
            get => _state;
            set
            {
                _state = value;
                OnPropertyChanged();
            }
        }
        public long LengthMilliseconds => Lengths[Mode];
        public string Display => (LengthMilliseconds / 1000 / 60).ToString() + "'" + ((LengthMilliseconds/1000)%60==0 ? "" : " " + ((LengthMilliseconds / 1000) % 60).ToString()+"''");

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class PomodoroViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Session> Sessions { get; set; } = [];
        public Session CurrentSession => Sessions[CurrentSessionIndex];
        public int NumberOfSessions => Sessions.Count;
        private int _currentSessionIndex = -1;
        public int CurrentSessionIndex
        {
            get => _currentSessionIndex;
            set
            {
                int s = value % NumberOfSessions;
                if (s < 0) s += NumberOfSessions;
                int old = _currentSessionIndex;
                if (s == old)
                    return;
                _currentSessionIndex = s;
                TimeLeft = CurrentSession.LengthMilliseconds;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TimeLeft));
                OnPropertyChanged(nameof(CurrentSession));
                if (old == -1)
                    old = 0;
                int min = Math.Min(old, s);
                int max= Math.Max(old, s);

                for (int i = min; i <= max; i++)
                {
                    if (i < CurrentSessionIndex) Sessions[i].State = Session.SessionState.Finished;
                    else if (i == CurrentSessionIndex) Sessions[i].State = Session.SessionState.Current;
                    else Sessions[i].State = Session.SessionState.Future;
                }
            }
        }
        public long TimeLeft { get; set; } = 0;
        public async void Initialize()
        {
            Sessions.Clear();
            Sessions.Add(new Session { Mode = Session.SessionMode.Study });
            Sessions.Add(new Session { Mode = Session.SessionMode.Break });
            Sessions.Add(new Session { Mode = Session.SessionMode.Study });
            Sessions.Add(new Session { Mode = Session.SessionMode.Break });
            Sessions.Add(new Session { Mode = Session.SessionMode.Study });
            Sessions.Add(new Session { Mode = Session.SessionMode.Break });
            Sessions.Add(new Session { Mode = Session.SessionMode.Study });
            Sessions.Add(new Session { Mode = Session.SessionMode.LongBreak });
            if(NumberOfSessions > 0) {
                CurrentSessionIndex = 0;
                for (int i = 1; i < NumberOfSessions; i++) Sessions[i].State = Session.SessionState.Future;
            }
        }

        private bool _isRunning = false;
        public bool IsRunning { get => _isRunning; set { _isRunning = value; OnPropertyChanged(); } }
        public ICommand StartPauseCommand => new Command(StartStop);
        public ICommand ChangeColourCommand => new Command(ChangeColour);
        public ICommand NextSessionCommand => new Command(() => CurrentSessionIndex++);
        public ICommand ResetCommand => new Command(() => { IsRunning = false; TimeLeft = CurrentSession.LengthMilliseconds;
            OnPropertyChanged(nameof(TimeLeft));
        });
        private string _backgroundColour="Red";
        public string BackgroundColour { get => _backgroundColour; set { _backgroundColour = value; OnPropertyChanged(); } }
        private void StartStop()
        {
            if(IsRunning) IsRunning = false;
            else IsRunning = true;
        }
        private void ChangeColour()
        {
            if (BackgroundColour == "Red") BackgroundColour = "Green";
            else BackgroundColour = "Red";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string name="")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
