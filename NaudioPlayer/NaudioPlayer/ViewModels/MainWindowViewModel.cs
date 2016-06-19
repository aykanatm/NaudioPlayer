using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using NaudioPlayer.Annotations;
using NaudioPlayer.Models;
using NaudioWrapper;

namespace NaudioPlayer.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private const float MaxVolume = 100;

        private enum PlaybackState
        {
            Playing, Stopped, Paused
        }

        private PlaybackState _playbackState;

        private readonly AudioPlayer _audioPlayer;

        private string _title;
        private double _currentTrackLenght;
        private double _currentTrackPosition;

        private Track _currentTrack;
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
            get
            {
                return _audioPlayer.GetVolume();
            }
            set
            {
                if (_audioPlayer != null)
                {
                    _audioPlayer.SetVolume(value);
                }
                
                OnPropertyChanged(nameof(CurrentVolume));
            }
        }

        public double CurrentTrackLenght
        {
            get { return _currentTrackLenght; }
            set
            {
                if (value.Equals(_currentTrackLenght)) return;
                _currentTrackLenght = value;
                OnPropertyChanged(nameof(CurrentTrackLenght));
            }
        }

        public double CurrentTrackPosition
        {
            get { return _currentTrackPosition; }
            set
            {
                if (value.Equals(_currentTrackPosition)) return;
                _currentTrackPosition = value;
                OnPropertyChanged(nameof(CurrentTrackPosition));
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

        public ICommand TrackControlMouseDownCommand { get; set; }
        public ICommand TrackControlMouseUpCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel()
        {
            Application.Current.MainWindow.Closing += MainWindow_Closing;

            Title = "NaudioPlayer";

            LoadCommands();

            Playlist = new ObservableCollection<Track>();

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

            TrackControlMouseDownCommand = new RelayCommand(TrackControlMouseDown, CanTrackControlMouseDown);
            TrackControlMouseUpCommand = new RelayCommand(TrackControlMouseUp, CanTrackControlMouseUp);
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
            var ofd = new OpenFileDialog();
            ofd.Filter = "Audio files (*.wav, *.mp3) | *.wav; *.mp3";
            var result = ofd.ShowDialog();
            if (result == true)
            {
                var removeExtension = ofd.SafeFileName.TakeWhile(c => c != '.');
                var enumerable = removeExtension as char[] ?? removeExtension.ToArray();
                string friendlyName = string.Empty;
                for (int i = 0; i < enumerable.Length; i++)
                {
                    friendlyName += enumerable[i];
                }
                var track = new Track(ofd.FileName, friendlyName);
                Playlist.Add(track);
            }
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
            var ofd = new OpenFileDialog();
            ofd.ValidateNames = false;
            ofd.CheckFileExists = false;
            ofd.FileName = "Folder Selection";
            var result = ofd.ShowDialog();
            if (result == true)
            {
                var folderName = ofd.FileName.Replace("Folder Selection", "");
                var audioFiles = Directory.GetFiles(folderName, "*.wav", SearchOption.AllDirectories);
                foreach (var audioFile in audioFiles)
                {
                    var track = new Track(audioFile,audioFile);
                    Playlist.Add(track);
                }
            }
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
            if (_playbackState == PlaybackState.Stopped)
            {
                if (CurrentTrack != null)
                {
                    _audioPlayer.LoadFile(CurrentTrack.Filepath, MaxVolume);
                    CurrentTrackLenght = _audioPlayer.GetLenghtInSeconds();
                    CurrentVolume = MaxVolume;
                }
            }
            _audioPlayer.TogglePlayPause(CurrentVolume);
        }

        private bool CanStartPlayback(object p)
        {
            if (CurrentTrack != null)
            {
                return true;
            }
            return false;
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

        private void TrackControlMouseDown(object p)
        {
            _audioPlayer.Pause();
        }

        private void TrackControlMouseUp(object p)
        {
            _audioPlayer.SetPosition(CurrentTrackPosition);
            _audioPlayer.Play(NAudio.Wave.PlaybackState.Paused, CurrentVolume);
        }

        private bool CanTrackControlMouseDown(object p)
        {
            if (_playbackState == PlaybackState.Playing)
            {
                return true;
            }
            return false;
        }

        private bool CanTrackControlMouseUp(object p)
        {
            if (_playbackState == PlaybackState.Paused)
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
