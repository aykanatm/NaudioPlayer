using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using MyLibrary.StringOperations.ExtensionMethods;
using NaudioPlayer.Models;

namespace NaudioPlayer.Services
{
    public class PlaylistLoader
    {
        public ICollection<Track> Load(string filePath)
        {
            using (var sr = new StreamReader(filePath))
            {
                var tracks = new Collection<Track>();

                var line = sr.ReadLine();
                do
                {
                    tracks.Add(new Track(line,line.RemovePath().Remove(line.RemovePath().Length - 4)));
                    line = sr.ReadLine();
                } while (line != null);

                return tracks;
            }
        }
    }
}
