using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace NaudioWrapper
{
    public class AudioPlayer
    {
        private AudioFileReader _audioFileReader;

        private WasapiOut _output;

        private string _filepath;

        //private readonly MMDevice _currentOutputDevice;

        public event Action PlaybackResumed;
        public event Action PlaybackStopped;
        public event Action PlaybackPaused;
        public event Action<double> UpdateTrackPosition;

        private PlaybackState _currentPlaybackState;

        public AudioPlayer()
        {
            //var enumerator = new MMDeviceEnumerator();
            //var defaultOutputDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            //_currentOutputDevice = defaultOutputDevice;
        }

        public void LoadFile(string filepath)
        {
            _filepath = filepath;
            InitializeStream();
            InitializeOutput();
        }

        private void InitializeStream()
        {
            _audioFileReader = new AudioFileReader(_filepath) {Volume = 1};
        }

        private void InitializeOutput()
        {
            _output = new WasapiOut(AudioClientShareMode.Shared, 200);
            _output.Init(_audioFileReader);
        }

        public void Play(PlaybackState playbackState, double currentVolumeLevel)
        {
            _currentPlaybackState = PlaybackState.Playing;

            if (playbackState == PlaybackState.Stopped)
            {
                _output.PlaybackStopped += _output_PlaybackStopped;
                InitializeStream();
                InitializeOutput();
                _output.Play();
            }
            else if (playbackState == PlaybackState.Paused)
            {
                _output.Play();
            }

            _audioFileReader.Volume = (float) currentVolumeLevel;

            if (PlaybackResumed != null)
            {
                PlaybackResumed();
            }
        }

        private void _output_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            _currentPlaybackState = PlaybackState.Stopped;
            if (PlaybackStopped != null)
            {
                PlaybackStopped();
            }
            Dispose();
        }

        public void Stop()
        {
            _currentPlaybackState = PlaybackState.Stopped;
            if (_output != null)
            {
                _output.Stop();
            }
        }

        public void Pause()
        {
            _currentPlaybackState = PlaybackState.Paused;
            if (_output != null)
            {
                if (PlaybackPaused != null)
                {
                    PlaybackPaused();
                }

                _output.Pause();
            }
        }

        public void TogglePlayPause(double currentVolumeLevel)
        {
            if (_output != null)
            {
                if (_output.PlaybackState == PlaybackState.Playing)
                {
                    Pause();
                }
                else
                {
                    Play(_output.PlaybackState, currentVolumeLevel);
                }
            }
            else
            {
                Play(PlaybackState.Stopped, currentVolumeLevel);
            }
        }

        public void Dispose()
        {
            if (_output != null)
            {
                if (_output.PlaybackState == PlaybackState.Playing)
                {
                    _output.Stop();
                }
                _output.Dispose();
                _output = null;
            }
            if (_audioFileReader != null)
            {
                _audioFileReader.Dispose();
                _audioFileReader = null;
            }
        }

        public double GetLenghtInSeconds()
        {
            if (_audioFileReader != null)
            {
                return _audioFileReader.TotalTime.TotalSeconds;
            }
            else
            {
                return 0;
            }
            
        }

        public double GetPositionInSeconds()
        {
            if (_audioFileReader != null)
            {
                return _audioFileReader.CurrentTime.TotalSeconds;
            }
            else
            {
                return 0;
            }
        }

        public float GetVolume()
        {
            if (_audioFileReader != null)
            {
                return _audioFileReader.Volume;
            }
            return 1;
        }

        public void SetPosition(double value)
        {
            if (_audioFileReader != null)
            {
                _audioFileReader.CurrentTime = TimeSpan.FromSeconds(value);
            }
        }

        public void SetVolume(float value)
        {
            if (_output != null)
            {
                _audioFileReader.Volume = value;
            }
        }
    }
}
