using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace A_B_music_test
{
    public class Music
    {
        public string filePath { get; set; }
        public string metadata { get; set; }
        public BitmapImage cover { get; set; }
        public bool playing { get; set; }

        public Music()
        {
            filePath = "";
            metadata = "";
            cover = null;
            playing = false;
        }

        //constuctor for initializing fields    
        public Music(string filePathPassed)
        {
            filePath = filePathPassed;
            extractMetadata(); //Method call to get meta data
            extractCover(); //Method call to get cover art
            playing = false;      
        }

        public void openFileLocation()
        {
            try
            {
                Process.Start(@System.IO.Path.GetDirectoryName(filePath));
            }
            catch (Win32Exception win32Exception)
            {
                //The system cannot find the file specified...
                Console.WriteLine(win32Exception.Message);
            }
        }

        //method for displaying customer records (functionality)    
        public void print()
        {
            Debug.WriteLine("Filepath=" + filePath);
            Debug.WriteLine("Metadata=" + metadata);
            Debug.WriteLine("Is Playing=" + playing);

        }

        //Uses TagLib to retrieve metadata and then formats said data
        private void extractMetadata()
        {
            TagLib.File tags = TagLib.File.Create(filePath);
            Debug.WriteLine(tags.Properties.AudioBitrate);
            //Bitrate = samplerate x bitdepth x channels
            Debug.WriteLine(tags.Properties.AudioSampleRate);
            Debug.WriteLine(tags.Properties.AudioChannels);
            Debug.WriteLine(tags.Properties.BitsPerSample);

            this.metadata = tags.Tag.Title + "\r" + tags.Tag.JoinedPerformers + "\r" + tags.Properties.AudioBitrate +
                 "kbps " + tags.Properties.AudioSampleRate + "Hz\r"+ tags.Properties.Duration.Minutes + ":" + string.Format("{0:D2}", tags.Properties.Duration.Seconds);
        }

        //Retrieves cover art (as a BitmapImage) from supplied filename
        private void extractCover()
        {
            TagLib.File tags = TagLib.File.Create(filePath);
            try
            {
                TagLib.IPicture pic = tags.Tag.Pictures[0];
                MemoryStream ms = new MemoryStream(pic.Data.Data);
                ms.Seek(0, SeekOrigin.Begin);

                // ImageSource for System.Windows.Controls.Image
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.EndInit();

                // Create a System.Windows.Controls.Image control
                this.cover = bitmap;
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("Music file " + filePath + "has no cover");
            }
        }



    }
}
