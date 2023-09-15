using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VoiceAndSoundRecord
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private CSettings _newSettings;

        public SettingsWindow(CSettings changeSettings)
        {
            InitializeComponent();

            _newSettings = changeSettings;
            PopulateForm();
        }


        private void PopulateForm()
        {

            cmbBitDepth.Items.Clear();
            cmbBitDepth.Items.Add("32");
            cmbBitDepth.Items.Add("24");
            cmbBitDepth.Items.Add("16");
            cmbBitDepth.Items.Add("8");

            cmbSampleRate.Items.Clear();
            cmbSampleRate.Items.Add("48000");
            cmbSampleRate.Items.Add("44100");
            cmbSampleRate.Items.Add("16000");
            cmbSampleRate.Items.Add("8000");

            int index = 0;
            cmbSampleRate.SelectedIndex = index;

            // now select the value for each
            foreach (object item in cmbSampleRate.Items) 
            { 
                if(item.ToString() == _newSettings.Qualitykbs.ToString())
                {
                    cmbSampleRate.SelectedIndex = index;

                }
                index++;
            }
            
            index = 0;
            cmbBitDepth.SelectedIndex = index;
            foreach (object item in cmbBitDepth.Items)
            {
                if (item.ToString() == _newSettings.BitDepth.ToString())
                {
                    cmbBitDepth.SelectedIndex = index;

                }
                index++;
            }
            //populate version and author

            Author.Text = "By Rmax 2023";
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            Version.Text = fileVersionInfo.ProductVersion;
            


        }


        private void OK_Click(object sender, RoutedEventArgs e)
        {
            _newSettings.BitDepth = int.Parse(cmbBitDepth.SelectedItem.ToString());
            _newSettings.Qualitykbs = int.Parse( cmbSampleRate.SelectedItem.ToString());
            this.Close();

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
