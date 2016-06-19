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
        private string _friendlyName;
        private string _filepath;

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
        
        public event PropertyChangedEventHandler PropertyChanged;

        public Track(string filepath, string friendlyName)
        {
            Filepath = filepath;
            FriendlyName = friendlyName;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
