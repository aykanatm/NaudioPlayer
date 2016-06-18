using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using NaudioPlayer.Annotations;
using NaudioPlayer.Models;
using NaudioWrapper;

namespace NaudioPlayer.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private enum PlaybackState
        {
            Playing, Stopped, Paused
        }

        private PlaybackState _playbackState;

        private readonly AudioPlayer _audioPlayer;

        private string _title;
        private Track _currentTrack;
        private float _currentVolume;
        private ObservableCollection<Track> _playlist;

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

        public float CurrentVolume
        {
            get { return _audioPlayer.GetVolume(); }
            set
            {
                if (value == _currentVolume) return;
                _currentVolume = value;
                if (_audioPlayer != null)
                {
                    _audioPlayer.SetVolume(value);
                }
                OnPropertyChanged(nameof(CurrentVolume));
            }
        }
        
        public Track CurrentTrack
        {
            get { return _currentTrack; }
            set
            {
                if (Equals(value, _currentTrack)) return;
                _currentTrack = value;
                OnPropertyChanged(nameof(CurrentTrack));
            }
        }

        public ObservableCollection<Track> Playlist
        {
            get { return _playlist; }
            set
            {
                if (Equals(value, _playlist)) return;
                _playlist = value;
                OnPropertyChanged(nameof(Playlist));
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
            Application.Current.MainWindow.Closing += MainWindow_Closing;

            Title = "NaudioPlayer";

            LoadCommands();

            _audioPlayer = new AudioPlayer();
            _audioPlayer.PlaybackPaused += _audioPlayer_PlaybackPaused;
            _audioPlayer.PlaybackResumed += _audioPlayer_PlaybackResumed;
            _audioPlayer.PlaybackStopped += _audioPlayer_PlaybackStopped;

            _playbackState = PlaybackState.Stopped;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            _audioPlayer.Dispose();
        }

        private void _audioPlayer_PlaybackStopped()
        {
            _playbackState = PlaybackState.Stopped;
        }

        private void _audioPlayer_PlaybackResumed()
        {
            _playbackState = PlaybackState.Playing;
        }

        private void _audioPlayer_PlaybackPaused()
        {
            _playbackState = PlaybackState.Paused;
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
            _audioPlayer.Dispose();
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
            if (_playbackState == PlaybackState.Stopped)
            {
                return true;
            }
            return false;
        }

        private void AddFolderToPlaylist(object p)
        {

        }
        private bool CanAddFolderToPlaylist(object p)
        {
            if (_playbackState == PlaybackState.Stopped)
            {
                return true;
            }
            return false;
        }

        private void RewindToStart(object p)
        {
            _audioPlayer.SetPosition(0);
        }
        private bool CanRewindToStart(object p)
        {
            if (_playbackState == PlaybackState.Playing)
            {
                return true;
            }
            return false;
        }

        private void StartPlayback(object p)
        {
            CurrentTrack = new Track(_audioPlayer,"01 The Trail.mp3");
            if (_playbackState == PlaybackState.Stopped)
            {
                _audioPlayer.LoadFile(CurrentTrack.Filepath);
            }
            _audioPlayer.TogglePlayPause(CurrentVolume);
        }
        private bool CanStartPlayback(object p)
        {
            return true;
        }

        private void StopPlayback(object p)
        {
            _audioPlayer.Stop();
        }
        private bool CanStopPlayback(object p)
        {
            if (_playbackState == PlaybackState.Playing || _playbackState == PlaybackState.Paused)
            {
                return true;
            }
            return false;
        }

        private void ForwardToEnd(object p)
        {
            _audioPlayer.SetPosition(_audioPlayer.GetLenghtInSeconds());
        }
        private bool CanForwardToEnd(object p)
        {
            if (_playbackState == PlaybackState.Playing)
            {
                return true;
            }
            return false;
        }

        private void Shuffle(object p)
        {
            
        }
        private bool CanShuffle(object p)
        {
            if (_playbackState == PlaybackState.Stopped)
            {
                return true;
            }
            return false;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
