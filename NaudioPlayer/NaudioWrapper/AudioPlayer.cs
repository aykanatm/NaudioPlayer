using System;
using NAudio.Wave;

namespace NaudioWrapper
{
    public class AudioPlayer
    {
        public enum PlaybackStopTypes
        {
            PlaybackStoppedByUser, PlaybackStoppedReachingEndOfFile
        }

        private AudioFileReader _audioFileReader;

        private DirectSoundOut _output;

        private string _filepath;

        //private readonly MMDevice _currentOutputDevice;

        public event Action PlaybackResumed;
        public event Action PlaybackStopped;
        public event Action PlaybackPaused;

        public AudioPlayer()
        {
            //var enumerator = new MMDeviceEnumerator();
            //var defaultOutputDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            //_currentOutputDevice = defaultOutputDevice;
        }

        public void LoadFile(string filepath, float volume)
        {
            _filepath = filepath;
            InitializeStream(volume);
            InitializeOutput();
        }

        private void InitializeStream(float volume)
        {
            _audioFileReader = new AudioFileReader(_filepath) {Volume = volume};
        }

        private void InitializeOutput()
        {
            _output = new DirectSoundOut(200);
            _output.PlaybackStopped += _output_PlaybackStopped;
            
            var wc = new WaveChannel32(_audioFileReader);
            wc.PadWithZeroes = false;
            _output.Init(wc);
        }

        public void Play(PlaybackState playbackState, double currentVolumeLevel)
        {
            if (playbackState == PlaybackState.Stopped)
            {
                InitializeStream(_audioFileReader.Volume);
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
            Dispose();
            if (PlaybackStopped != null)
            {
                PlaybackStopped();
            }
        }

        public void Stop()
        {
            if (_output != null)
            {
                Dispose();

                if (PlaybackStopped != null)
                {
                    PlaybackStopped();
                }
            }
        }

        public void Pause()
        {
            if (_output != null)
            {
                _output.Pause();

                if (PlaybackPaused != null)
                {
                    PlaybackPaused();
                }
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
