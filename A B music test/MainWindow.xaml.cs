using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Globalization;
using System.Diagnostics;
using System.Windows.Media.Animation;

namespace A_B_music_test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string aFile;
        public string bFile;
        public string aMeta;
        public string bMeta;
        public BitmapImage aImage;
        public BitmapImage bImage;
        public bool aSet = false;
        public bool bSet = false;
        public bool aPlaying = false;
        public bool bPlaying = false;
        private MediaPlayer aPlayer = new MediaPlayer();
        private MediaPlayer bPlayer = new MediaPlayer();
        private int switchDelay;
        private System.Timers.Timer switchTimer = new System.Timers.Timer();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void A_Click(object sender, RoutedEventArgs e)
        {
            if (getMusicFile(ref aFile) == true) //Method call to get a music file
            {
                aMeta = getMetaData(aFile); //Method call to get meta data
                aInfo.Text = aMeta;
                aImage = getCover(aFile); //Method call to get cover art
                aCover.Source = aImage;

                aSet = true;
                aReselect.Visibility = Visibility.Visible;
                A.Visibility = Visibility.Collapsed;

                if (bSet == true)
                {
                    Confirm.Visibility = Visibility.Visible;
                }
            }
        }


        private void B_Click(object sender, RoutedEventArgs e)
        {
            if (getMusicFile(ref bFile) == true) //Method call to get a music file
            {
                bMeta = getMetaData(bFile); //Method call to get meta data
                bInfo.Text = bMeta;
                bImage = getCover(bFile); //Method call to get cover art
                bCover.Source = bImage;

                bSet = true;
                bReselect.Visibility = Visibility.Visible;
                B.Visibility = Visibility.Collapsed;

                if (aSet == true)
                {
                    Confirm.Visibility = Visibility.Visible;
                }
            }
        }

        private void notBlind_Click(object sender, RoutedEventArgs e)
        {
            tabControl1.SelectedItem = notBlindTab;
            aInfo_NB.Text = aMeta;
            bInfo_NB.Text = bMeta;
            aCover_NB.Source = aImage;
            bCover_NB.Source = bImage;
            aPlayer.Open(new Uri(aFile));
            bPlayer.Open(new Uri(bFile));

        }

        private void Blind_Click(object sender, RoutedEventArgs e)
        {
            //tabControl1.SelectedItem = blindTab;
        }

        // ~~~~~~~~~~~~~~~~~~~~~~METHODS~~~~~~~~~~~~~~~~~~~~~~


        //Returns boolean so main can verify that fileName was set and do subsequent actions
        //the filename is passed by reference so it must be called with ref
        private Boolean getMusicFile(ref String fileName)
        {
            // Configure open file dialog box 
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = "*.mp3; *.m4a; *.wma; *.flac"; // Default file extension 
            dlg.Filter = "Music Files|*.mp3;*.m4a;*.wma;*.flac|All files (*.*)|*.*"; // Filter files by extension
            dlg.Multiselect = false;

            // Show open file dialog box 
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                // Set bFile to filename 
                fileName = dlg.FileName;
                return true;

            }
            else
                return false;
        }

        //Uses TagLib to retrieve metadata and then formats said data
        private string getMetaData(String fileName)
        {
            TagLib.File Tags = TagLib.File.Create(fileName);
            return Tags.Tag.Title + "\r" + Tags.Tag.JoinedPerformers + "\r" + Tags.Properties.AudioBitrate +
                 "kbps\r" + Tags.Properties.Duration.Minutes + ":" +
                 string.Format("{0:D2}",Tags.Properties.Duration.Seconds);
        }

        //Retrieves cover art (as a BitmapImage) from supplied filename
        private BitmapImage getCover(String fileName)
        {

            TagLib.File bTags = TagLib.File.Create(fileName);
            try
            {
                TagLib.IPicture pic = bTags.Tag.Pictures[0];
                MemoryStream ms = new MemoryStream(pic.Data.Data);
                ms.Seek(0, SeekOrigin.Begin);

                // ImageSource for System.Windows.Controls.Image
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.EndInit();

                // Create a System.Windows.Controls.Image control
                //bCover.Source = bitmap;
                return bitmap;
            }
            catch (IndexOutOfRangeException)
            {
                return null;
                //bCover.Source = new BitmapImage(new Uri("pack://application:,,,/YourAssemblyName;component/Resources/placeholder.png", UriKind.Absolute));
            }
        }

        //START OF WHAT USED TO BE OLD WINDOW

        private void pause(object sender, RoutedEventArgs e)
        {
            aPlayer.Pause();
            bPlayer.Pause();
            aPlaying = false;
            bPlaying = false;
            aIndicator.IsEnabled = true;
            bIndicator.IsEnabled = true;
            switchDelayInput.IsReadOnly = false;
            switchTimer.Stop();
            aIndicator.Content = "Play A First";
            bIndicator.Content = "Play B First";
            //((Storyboard)this.Resources["timerCircle"]).Stop(this);
        }

        private void play(object sender, RoutedEventArgs e)
        {
            aIndicator.Content = "A Is Playing";
            bIndicator.Content = "B Is Playing";
            InitializePropertyValues();
            //((Storyboard)this.Resources["timerCircle"]).Begin(this);

            if ((bool)aIndicator.IsChecked)
            {
                //Start playing with A
                aPlaying = true;
                bPlayer.Volume = 0;
                aPlayer.Play();
                bPlayer.Play();
            }
            else
            {
                //Start playing with B
                bPlaying = true;
                aPlayer.Volume = 0;
                aPlayer.Play();
                bPlayer.Play();
            }

            aIndicator.IsEnabled = false;
            bIndicator.IsEnabled = false;
            switchDelayInput.IsReadOnly = true;


            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
            InitTimer();
        }

        private void stop(object sender, MouseButtonEventArgs e)
        {
            aPlayer.Stop();
            bPlayer.Stop();
            playPause.IsChecked = false;
            aPlaying = false;
            bPlaying = false;
            aIndicator.IsEnabled = true;
            bIndicator.IsEnabled = true;
            switchTimer.Stop();
            switchDelayInput.IsReadOnly = false;
            aIndicator.Content = "Play A First";
            bIndicator.Content = "Play B First";
        }

        private void stop()
        {
            aPlayer.Stop();
            bPlayer.Stop();
            playPause.IsChecked = false;
            aPlaying = false;
            bPlaying = false;
            aIndicator.IsEnabled = true;
            bIndicator.IsEnabled = true;
            switchTimer.Stop();
            switchDelayInput.IsReadOnly = false;
            aIndicator.Content = "Play A First";
            bIndicator.Content = "Play B First";
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (aPlayer.Source != null && aPlayer.NaturalDuration.HasTimeSpan && aIndicator.IsChecked == true) //A time displayed
                Progress.Text = string.Format("{0} / {1}", aPlayer.Position.ToString(@"mm\:ss"), aPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));

            else if (bPlayer.Source != null && bPlayer.NaturalDuration.HasTimeSpan && bIndicator.IsChecked == true) //B time displayed
                Progress.Text = string.Format("{0} / {1}", bPlayer.Position.ToString(@"mm\:ss"), bPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));

            else
                Progress.Text = "No files selected..."; //A and B are not selected
        }
        void InitializePropertyValues()
        {
            // Set the media's starting Volume and SpeedRatio to the current value of the
            // their respective slider controls.

            aPlayer.Volume = aVolume.Value;
            bPlayer.Volume = (double)bVolume.Value;
        }

        public void InitTimer()
        {

            // Create a timer
            switchTimer = new System.Timers.Timer();
            // Tell the timer what to do when it elapses
            switchTimer.Elapsed += new ElapsedEventHandler(switchAB);
            // Set it to go off every five seconds (by default)
            switchTimer.Interval = (switchDelay * 1000);
            // And start it        
            switchTimer.Start();
        }

        private void switchAB(object sender, ElapsedEventArgs e)
        {
            
            this.Dispatcher.Invoke(() =>
            {

                if ((bool)aIndicator.IsChecked) //Switches from A to B
                {
                    aPlayer.Volume = 0;
                    aPlaying = false;
                    //bPlayer.Position = aPlayer.Position;
                    //aVolume.IsEnabled = false;
                    //bVolume.IsEnabled = true;
                    bPlaying = true;
                    bPlayer.Volume = (double)bVolume.Value;
                    bIndicator.IsChecked = true;
                }
                else //Switches from B to A
                {
                    bPlayer.Volume = 0;
                    bPlaying = false;
                    //aPlayer.Position = bPlayer.Position;
                    // aPlaying = true;
                    //bVolume.IsEnabled = false;
                    //aVolume.IsEnabled = true;
                    aPlaying = true;
                    aPlayer.Volume = (double)aVolume.Value;
                    aIndicator.IsChecked = true;
                }
            });
        }

        private void aVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(aPlaying == true)
                aPlayer.Volume = aVolume.Value;
        }

        private void bVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bPlaying == true)
                bPlayer.Volume = bVolume.Value;
        }

        private void switchDelayInput_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            switchDelay = (int)switchDelayInput.Value;
        }

        private void Back_NB_Click(object sender, RoutedEventArgs e)
        {
            tabControl1.SelectedItem = mainTab;
            stop();
        }

        private void aFileLocation(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Process.Start(@System.IO.Path.GetDirectoryName(aFile));
            }
            catch (Win32Exception win32Exception)
            {
                //The system cannot find the file specified...
                Console.WriteLine(win32Exception.Message);
            }
        }

        private void bFileLocation(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Process.Start(@System.IO.Path.GetDirectoryName(bFile));
            }
            catch (Win32Exception win32Exception)
            {
                //The system cannot find the file specified...
                Console.WriteLine(win32Exception.Message);
            }
        }
    }
}