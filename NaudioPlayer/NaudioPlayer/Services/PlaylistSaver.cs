using System.Collections.Generic;
using System.IO;
using NaudioPlayer.Models;

namespace NaudioPlayer.Services
{
    public class PlaylistSaver
    {
        public void Save(ICollection<Track> playlist, string destinationFilename)
        {
            using (var sw = new StreamWriter(destinationFilename))
            {
                foreach (var track in playlist)
                {
                    sw.WriteLine(track.Filepath);
                }
            }
        }
    }
}
