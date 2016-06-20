using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using MyLibrary.CustomCollections.ExtensionMethods;
using MyLibrary.StringOperations.ExtensionMethods;
using NaudioPlayer.Annotations;
using NaudioPlayer.Models;
using NaudioPlayer.Services;
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
        private double _currentTrackLenght;
        private double _currentTrackPosition;
        private string _playPauseImageSource;
        private float _currentVolume;

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

        public string PlayPauseImageSource
        {
            get { return _playPauseImageSource; }
            set
            {
                if (value == _playPauseImageSource) return;
                _playPauseImageSource = value;
                OnPropertyChanged(nameof(PlayPauseImageSource));
            }
        }

        public float CurrentVolume
        {
            get { return _currentVolume; }
            set
            {

                if (value == _currentVolume) return;
                _currentVolume = value;
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
        public ICommand SavePlaylistCommand { get; set; }
        public ICommand LoadPlaylistCommand { get; set; }

        public ICommand RewindToStartCommand { get; set; }
        public ICommand StartPlaybackCommand { get; set; }
        public ICommand StopPlaybackCommand { get; set; }
        public ICommand ForwardToEndCommand { get; set; }
        public ICommand ShuffleCommand { get; set; }

        public ICommand TrackControlMouseDownCommand { get; set; }
        public ICommand TrackControlMouseUpCommand { get; set; }
        public ICommand VolumeControlValueChangedCommand { get; set; }

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

            PlayPauseImageSource = "../Images/play.png";
            CurrentVolume = 1;

            var timer = new System.Timers.Timer();
            timer.Interval = 300;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void UpdateSeekBar()
        {
            if (_playbackState == PlaybackState.Playing)
            {
                CurrentTrackPosition = _audioPlayer.GetPositionInSeconds();
            }
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            UpdateSeekBar();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            _audioPlayer.Dispose();
        }

        private void _audioPlayer_PlaybackStopped()
        {
            _playbackState = PlaybackState.Stopped;
            PlayPauseImageSource = "../Images/play.png";
            CommandManager.InvalidateRequerySuggested();
        }

        private void _audioPlayer_PlaybackResumed()
        {
            _playbackState = PlaybackState.Playing;
            PlayPauseImageSource = "../Images/pause.png";
        }

        private void _audioPlayer_PlaybackPaused()
        {
            _playbackState = PlaybackState.Paused;
            PlayPauseImageSource = "../Images/play.png";
        }

        private void LoadCommands()
        {
            // Menu commands
            ExitApplicationCommand = new RelayCommand(ExitApplication,CanExitApplication);
            AddFileToPlaylistCommand = new RelayCommand(AddFileToPlaylist, CanAddFileToPlaylist);
            AddFolderToPlaylistCommand = new RelayCommand(AddFolderToPlaylist, CanAddFolderToPlaylist);
            SavePlaylistCommand = new RelayCommand(SavePlaylist, CanSavePlaylist);
            LoadPlaylistCommand = new RelayCommand(LoadPlaylist, CanLoadPlaylist);

            // Player commands
            RewindToStartCommand = new RelayCommand(RewindToStart, CanRewindToStart);
            StartPlaybackCommand = new RelayCommand(StartPlayback, CanStartPlayback);
            StopPlaybackCommand = new RelayCommand(StopPlayback, CanStopPlayback);
            ForwardToEndCommand = new RelayCommand(ForwardToEnd, CanForwardToEnd);
            ShuffleCommand = new RelayCommand(Shuffle, CanShuffle);

            // Event commands
            TrackControlMouseDownCommand = new RelayCommand(TrackControlMouseDown, CanTrackControlMouseDown);
            TrackControlMouseUpCommand = new RelayCommand(TrackControlMouseUp, CanTrackControlMouseUp);
            VolumeControlValueChangedCommand = new RelayCommand(VolumeControlValueChanged, CanVolumeControlValueChanged);
        }

        // Menu commands
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
            ofd.Filter = "Audio files (*.wav, *.mp3, *.wma, *.ogg, *.flac) | *.wav; *.mp3; *.wma; *.ogg; *.flac";
            var result = ofd.ShowDialog();
            if (result == true)
            {
                var friendlyName = ofd.SafeFileName.Remove(ofd.SafeFileName.Length - 4);
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
            var cofd = new CommonOpenFileDialog();
            cofd.IsFolderPicker = true;
            var result = cofd.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                var folderName = cofd.FileName;
                var audioFiles = Directory.EnumerateFiles(folderName, "*.*", SearchOption.AllDirectories)
                                          .Where(f=>f.EndsWith(".wav") || f.EndsWith(".wav") || f.EndsWith(".wma") || f.EndsWith(".ogg") || f.EndsWith(".flac"));
                foreach (var audioFile in audioFiles)
                {
                    var removePath = audioFile.RemovePath();
                    var friendlyName = removePath.Remove(removePath.Length - 4);
                    var track = new Track(audioFile, friendlyName);
                    Playlist.Add(track);
                }
                Playlist = new ObservableCollection<Track>(Playlist.OrderBy(z => z.FriendlyName).ToList());
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

        private void SavePlaylist(object p)
        {
            var sfd = new SaveFileDialog();
            sfd.CreatePrompt = false;
            sfd.OverwritePrompt = true;
            sfd.Filter = "PLAYLIST files (*.playlist) | *.playlist";
            if (sfd.ShowDialog() == true)
            {
                var ps = new PlaylistSaver();
                ps.Save(Playlist, sfd.FileName);
            }
        }

        private bool CanSavePlaylist(object p)
        {
            return true;
        }

        private void LoadPlaylist(object p)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "PLAYLIST files (*.playlist) | *.playlist";
            if (ofd.ShowDialog() == true)
            {
                Playlist = new PlaylistLoader().Load(ofd.FileName).ToObservableCollection();
            }
        }

        private bool CanLoadPlaylist(object p)
        {
            return true;
        }

        // Player commands
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
                    _audioPlayer.LoadFile(CurrentTrack.Filepath, CurrentVolume);
                    CurrentTrackLenght = _audioPlayer.GetLenghtInSeconds();
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

        // Events
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

        private void VolumeControlValueChanged(object p)
        {
            _audioPlayer.SetVolume(CurrentVolume);
        }

        private bool CanVolumeControlValueChanged(object p)
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
