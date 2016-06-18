using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NaudioPlayer.Annotations;

namespace NaudioPlayer.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _title;

        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title) return;
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public ICommand ExitApplicationCommand { get; set; }
        public ICommand AddFileToPlaylistCommand { get; set; }
        public ICommand AddFolderToPlaylistCommand { get; set; }

        public ICommand RewindToStartCommand { get; set; }
        public ICommand StartPlaybackCommand { get; set; }
        public ICommand StopPlaybackCommand { get; set; }
        public ICommand ForwardToEndCommand { get; set; }
        public ICommand ShuffleCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel()
        {
            Title = "NaudioPlayer";
            LoadCommands();
        }

        private void LoadCommands()
        {
            ExitApplicationCommand = new RelayCommand(ExitApplication,CanExitApplication);
            AddFileToPlaylistCommand = new RelayCommand(AddFileToPlaylist, CanAddFileToPlaylist);
            AddFolderToPlaylistCommand = new RelayCommand(AddFolderToPlaylist, CanAddFolderToPlaylist);

            RewindToStartCommand = new RelayCommand(RewindToStart, CanRewindToStart);
            StartPlaybackCommand = new RelayCommand(StartPlayback, CanStartPlayback);
            StopPlaybackCommand = new RelayCommand(StopPlayback, CanStopPlayback);
            ForwardToEndCommand = new RelayCommand(ForwardToEnd, CanForwardToEnd);
            ShuffleCommand = new RelayCommand(Shuffle, CanShuffle);
        }

        private void ExitApplication(object p)
        {
            Application.Current.Shutdown();
        }
        private bool CanExitApplication(object p)
        {
            return true;
        }

        private void AddFileToPlaylist(object p)
        {
            
        }
        private bool CanAddFileToPlaylist(object p)
        {
            return true;
        }

        private void AddFolderToPlaylist(object p)
        {

        }
        private bool CanAddFolderToPlaylist(object p)
        {
            return true;
        }

        private void RewindToStart(object p)
        {
            
        }
        private bool CanRewindToStart(object p)
        {
            return true;
        }

        private void StartPlayback(object p)
        {
            
        }
        private bool CanStartPlayback(object p)
        {
            return true;
        }

        private void StopPlayback(object p)
        {
            
        }
        private bool CanStopPlayback(object p)
        {
            return true;
        }

        private void ForwardToEnd(object p)
        {
            
        }
        private bool CanForwardToEnd(object p)
        {
            return true;
        }

        private void Shuffle(object p)
        {
            
        }
        private bool CanShuffle(object p)
        {
            return true;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
