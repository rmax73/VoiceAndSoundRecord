using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using System.Configuration;

namespace VoiceAndSoundRecord
{
    public  class CSettings
    {

        public float MicAudioLevel { get; set; }
        public float LoopBackAudioLevel { get; set; }
        public int Qualitykbs { get; set; }
        public int DefaultMicIndex { get; set; }
        
        public int RecordingHistory { get; set; }

        private int _bitDepth;
        public int BitDepth 
        {   get { return _bitDepth; }

            set
            {
                int newValue = value;
                if (newValue != 8 && newValue != 16 && newValue != 24 && newValue != 32)
                { 
                    throw new ArgumentException();
                }
                _bitDepth = newValue;
            }
        }

        

        public CSettings() 
        { 

            MicAudioLevel = 1.0f;
            LoopBackAudioLevel = 0.2f;
            Qualitykbs = 8000;
            DefaultMicIndex = 0;
            RecordingHistory = 1;
            BitDepth = 16;
        }


        public string GetAppFolder()
        {

            return Environment.SpecialFolder.ApplicationData.ToString();
        }

        public void Save()
        {
            string fileName = Directory.GetCurrentDirectory() + "\\Settings.json";
            string jsonString = JsonSerializer.Serialize(this);
            File.WriteAllText(fileName, jsonString);
        }

        public static CSettings Load()
        {
            string fileName = Directory.GetCurrentDirectory() + "\\Settings.json";
            string jsonString = File.ReadAllText(fileName);
            CSettings appSettings = JsonSerializer.Deserialize<CSettings>(jsonString)!;
            return appSettings;
        }

    }
}
