using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NaudioPlayer.Annotations;
using NaudioWrapper;

namespace NaudioPlayer.Models
{
    public class Track : INotifyPropertyChanged
    {
        private readonly AudioPlayer _audioPlayer;
        private string _friendlyName;
        private string _filepath;
        private double _lenght;
        private double _currentPosition;

        public string FriendlyName
        {
            get { return _friendlyName; }
            set
            {
                if (value == _friendlyName) return;
                _friendlyName = value;
                OnPropertyChanged(nameof(FriendlyName));
            }
        }

        public string Filepath
        {
            get { return _filepath; }
            set
            {
                if (value == _filepath) return;
                _filepath = value;
                OnPropertyChanged(nameof(Filepath));
            }
        }

        public double Lenght
        {
            get { return _audioPlayer.GetLenghtInSeconds(); }
            set
            {
                if (value == _lenght) return;
                _lenght = value;
                OnPropertyChanged(nameof(Lenght));
            }
        }

        public double CurrentPosition
        {
            get { return _audioPlayer.GetPositionInSeconds(); }
            set
            {
                if (value == _currentPosition) return;
                _audioPlayer.SetPosition(value);
                _currentPosition = value;
                OnPropertyChanged(nameof(CurrentPosition));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Track(AudioPlayer audioPlayer, string filepath)
        {
            _audioPlayer = audioPlayer;
            Filepath = filepath;
            CurrentPosition = 0;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
