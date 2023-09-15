using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using Microsoft.VisualBasic.Logging;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace VoiceAndSoundRecord
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IWaveIn loopbackCapture;
        WaveFileWriter _writer;
        WaveFileWriter _micWriter;
        IWaveIn microphoneCapture;

        private const string LOOPBACK_FILENAME= "loopbackCaptureWav";
        private const string MIC_FILENAME = "micCaptureWav";

        bool _IsRecording;

        private CSettings _appSettings = new CSettings();

        public MainWindow()
        {
            InitializeComponent();

            InitialiseComboBox();

            SetWindowDefaults();

            RecActiveComp.Visibility = Visibility.Hidden;
            RecActiveMic.Visibility = Visibility.Hidden;
        }

        public void InitialiseComboBox()
        {

            for (int n = 0; n < WaveIn.DeviceCount; n++)
            {
                var caps = WaveIn.GetCapabilities(n);
                cmbInputDevice.Items.Add((caps.ProductName));

            }

        }

        private void SetWindowDefaults()
        {
            try
            {
                _appSettings = CSettings.Load();
            }
            catch
            {

                _appSettings = new CSettings();
            }

            cmbInputDevice.SelectedIndex = _appSettings.DefaultMicIndex;
            ComputerAudio.Value = _appSettings.LoopBackAudioLevel;
            MicAudio.Value = _appSettings.MicAudioLevel;
        }
        private void RecordClick(object sender, RoutedEventArgs e)
        {

            if (_IsRecording == true)
            {
                StopRecording();

            }
            else
            {

                StartRecording();
            }

        }

        public void StartRecording()
        {
            TestAppFolder();
            // microphoneCapture.WaveFormat = new WaveFormat(44100, 1);
            loopbackCapture = new WasapiLoopbackCapture();
            loopbackCapture.WaveFormat = new WaveFormat(_appSettings.Qualitykbs, _appSettings.BitDepth, 2);

            microphoneCapture = new WaveInEvent
            {

                DeviceNumber = cmbInputDevice.SelectedIndex,
                WaveFormat = loopbackCapture.WaveFormat,
                BufferMilliseconds = 20


            };
            Record.Content = "Stop ";
            Mix.IsEnabled = false;
            CopyRaw.IsEnabled = false;
            Settings.IsEnabled = false;
            cmbInputDevice.IsEnabled = false;

            _writer = new WaveFileWriter(_appSettings.GetAppFolder() + "\\" + LOOPBACK_FILENAME + ".wav", loopbackCapture.WaveFormat);
            _micWriter = new WaveFileWriter(_appSettings.GetAppFolder() + "\\" + MIC_FILENAME + ".wav", microphoneCapture.WaveFormat);

            loopbackCapture.DataAvailable += OnDataAvailable;
            microphoneCapture.DataAvailable += OnMicDataAvailable;

            loopbackCapture.RecordingStopped += OnRecordingStopped;
            microphoneCapture.RecordingStopped += OnRecordingStopped;

            loopbackCapture.StartRecording();
            Log("Loopback Recording Started");

            microphoneCapture.StartRecording();
            Log("Mic Recording Started");

            _IsRecording = true;

        }

        public void StopRecording()
        {
            if (loopbackCapture == null)
            {
                return;
            }

            loopbackCapture.StopRecording();
            microphoneCapture.StopRecording();

            RecActiveComp.Visibility = Visibility.Hidden;
            RecActiveMic.Visibility = Visibility.Hidden;

            Log("Recording Stopped");
            Record.Content = "Record";
            _IsRecording = false;
            Mix.IsEnabled = true;
            CopyRaw.IsEnabled = true;
            Settings.IsEnabled = true;
            cmbInputDevice.IsEnabled = true;
        }

        void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            // Writer Close() needs to come first otherwise NAudio will lock up.
            if (_writer != null)
            {
                _writer.Close();
                _writer = null;
            }
            if (loopbackCapture != null)
            {
                loopbackCapture.Dispose();
                loopbackCapture = null;
            }

            if (microphoneCapture != null)
            {
                microphoneCapture.Dispose();
                microphoneCapture = null;
            }
            // Writer Close() needs to come first otherwise NAudio will lock up.
            if (_micWriter != null)
            {
                _micWriter.Close();
                _micWriter = null;
            }



            _IsRecording = false;
            if (e.Exception != null)
            {
                throw e.Exception;
            }
        }

        void OnDataAvailable(object sender, WaveInEventArgs e)
        {

            _writer.Write(e.Buffer, 0, e.BytesRecorded);

            if (RecActiveComp.Visibility == Visibility.Visible && e.Buffer.Max() > 0)
            {
                RecActiveComp.Dispatcher.BeginInvoke(new Action(() =>
                {
                    RecActiveComp.Visibility = Visibility.Hidden;
                }));

            }
            else
            {
                RecActiveComp.Dispatcher.BeginInvoke(new Action(() =>
                {
                    RecActiveComp.Visibility = Visibility.Visible;
                }));

            }

        }


        void OnMicDataAvailable(object sender, WaveInEventArgs e)
        {

            _micWriter.Write(e.Buffer, 0, e.BytesRecorded);
            if (RecActiveMic.Visibility == Visibility.Visible && e.Buffer.Max() > 0)
            {
                RecActiveMic.Dispatcher.BeginInvoke(new Action(() =>
                {
                    RecActiveMic.Visibility = Visibility.Hidden;
                }));

            }
            else
            {
                RecActiveMic.Dispatcher.BeginInvoke(new Action(() =>
                {
                    RecActiveMic.Visibility = Visibility.Visible;
                }));

            }


        }



        private string MixWavFiles()
        {

            // generate filename
            string currentDirectory = _appSettings.GetAppFolder();

            string filename = currentDirectory + "\\CVR" + GetDateTimeFilename() +".wav";



            try
            {

            
                using (var LoopBackInput = new AudioFileReader(currentDirectory + "\\" + LOOPBACK_FILENAME + ".wav"))
                using (var MicroPhoneInput = new AudioFileReader(currentDirectory + "\\" + MIC_FILENAME+ ".wav"))
                {
                    LoopBackInput.Volume = (float)ComputerAudio.Value;
                    MicroPhoneInput.Volume = (float)MicAudio.Value;
                    var mixer = new MixingSampleProvider(new[] { LoopBackInput, MicroPhoneInput });

                    WaveFileWriter.CreateWaveFile16(filename, mixer);
                }

            }
            catch
            {

                MessageBox.Show("Could not mix files, is there issue with file access","Cannot mix", MessageBoxButton.OK, MessageBoxImage.Error);
                Log("ERROR could mix files - is the file access available, or are the files open in another location?");

            }
            
            return filename;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string filename = MixWavFiles();

            MessageBox.Show("Mixed Output and Voice to:" + filename, "Mixed", MessageBoxButton.OK, MessageBoxImage.Information);
            Log("Mixed Output and Voice to:" + filename);
        }

        private void OpenRecording_Click(object sender, RoutedEventArgs e)
        {
            CreateRecordingDirector();
            Process.Start("explorer.exe", _appSettings.GetAppFolder());
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow setSettings = new SettingsWindow(_appSettings);
            setSettings.ShowDialog();

        }

        private void CreateRecordingDirector()
        {

            // test if the app data folder exists, if it doesn't create
            if (Directory.Exists(_appSettings.GetAppFolder()))
            {
                return;
            }
            else
            {
                Directory.CreateDirectory(_appSettings.GetAppFolder());
            }

        }

        private void MainClose(object sender, System.ComponentModel.CancelEventArgs e)
        {

            _appSettings.DefaultMicIndex = cmbInputDevice.SelectedIndex;
            _appSettings.LoopBackAudioLevel = (float)ComputerAudio.Value;
            _appSettings.MicAudioLevel = (float)MicAudio.Value;
            _appSettings.Save();

        }

        public void Log(string message)
        {

            string logMessage = DateTime.UtcNow.ToString() + " : " + message;

            // File.AppendAllText(_appSettings.GetAppFolder + "\\CVRLog.txt");
            try
            {
                File.AppendAllText(_appSettings.GetAppFolder() + "\\CVRLog.txt", logMessage + Environment.NewLine);
            }
            catch
            {
                MessageBox.Show("Could not Log message : " + logMessage, "Cannot Access Log File", MessageBoxButton.OK, MessageBoxImage.Error);
            }



        }

        private void CopyRaw_Click(object sender, RoutedEventArgs e)
        {

            try
            {

                File.Copy(_appSettings.GetAppFolder() + "\\" + LOOPBACK_FILENAME + ".wav", _appSettings.GetAppFolder() + "\\" + GetDateTimeFilename() + "loopCap.wav");
                File.Copy(_appSettings.GetAppFolder() + "\\" + MIC_FILENAME + ".wav", _appSettings.GetAppFolder() + "\\" + GetDateTimeFilename() + "micCap.wav");
                MessageBox.Show("Copies of Loopback and Mic Capture Files made", "Copied Capture files", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {

                MessageBox.Show("Are the files open in another location?", "Could not copy capture files", MessageBoxButton.OK, MessageBoxImage.Error);
                Log("Could not copy capture files, are the files open in another location, do the source files exist? Details in next messages:");
                Log("Source Files: "+ _appSettings.GetAppFolder() + "\\" + LOOPBACK_FILENAME + ".wav" + " and " + _appSettings.GetAppFolder() + "\\" + MIC_FILENAME + ".wav");
            }

        }

        private string GetDateTimeFilename()
        {
            string filename;
            filename = DateTime.Now.Year.ToString()
            + AddLeadingZero(DateTime.Now.Month.ToString())
            + AddLeadingZero(DateTime.Now.Day.ToString())
            + "_"
            + DateTime.Now.Hour.ToString()
            + DateTime.Now.Minute.ToString()
            + DateTime.Now.Second.ToString();

            return filename;
        }

        private string AddLeadingZero(string value)
        {
            string returnString;
            value = "00"+value;
            returnString = value.Substring(value.Length - 2);

            return returnString;


        }

        public void TestAppFolder()
        {
            try
            {
           
                if(Directory.Exists(_appSettings.GetAppFolder()))
                {
                    return;
                }
                else
                {
                    Directory.CreateDirectory(_appSettings.GetAppFolder());
                }

            }
            catch (Exception ex) 
            {
                MessageBox.Show("Cannot Access " + _appSettings.GetAppFolder(), "Cannot access file",MessageBoxButton.OK,MessageBoxImage.Error);
            
            }




        }




    }
}
