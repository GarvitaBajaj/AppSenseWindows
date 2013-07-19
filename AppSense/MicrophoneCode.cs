using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using System.IO.IsolatedStorage;

namespace AppSense
{
    public class MicrophoneCode
    {
        Microphone m = Microphone.Default;
        byte[] data;
        MemoryStream ms = new MemoryStream();
        LogFormat log = new LogFormat();
        int itCount = 0, count = 0;
        EventSource source = default(EventSource);

        public MicrophoneCode()
        {
        }

        /// <summary>
        /// initializes the microphone buffer
        /// </summary>
        /// <param name="bufferDuration">the audio buffer duration of the microphone in milliseconds</param>
        public void InitializeMic(int bufferDuration)
        {
            m.BufferDuration = TimeSpan.FromMilliseconds(bufferDuration);
            data = new byte[m.GetSampleSizeInBytes(m.BufferDuration)];
            m.BufferReady += m_BufferReady;
        }

        /// <summary>
        /// starts recording audio by calling Microphone.Start()
        /// </summary>
        public void RecordSound()
        {
            if (m.State == MicrophoneState.Stopped)
            {
                m.Start();
                log.WriteLog("microphone started");
            }
        }

        /// <summary>
        /// stops audio recording
        /// </summary>
        public void StopRecording()
        {
            try
            {
                if (m.State != MicrophoneState.Stopped)
                {
                    m.Stop();
                    log.WriteLog("microphone stopped");
                }
            }
            catch (Exception e)
            {
                log.WriteLog(e.Message);
            }
        }

        /// <summary>
        /// saves the audio stream to file "Microphone_number"
        /// </summary>
        /// <param name="itCount">the number to be appended  to the fileName</param>
        public void SaveSoundToFile(int itCount)
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream fs = store.OpenFile("Microphone_" + itCount + ".wav", FileMode.OpenOrCreate))
                {
                    log.WriteLog("Microphone data saved");
                    MemoryStream hdr = new MemoryStream();
                    WriteWavHeader(hdr, m.SampleRate, (int)ms.Length);
                    hdr.WriteTo(fs);
                    ms.WriteTo(fs);
                    fs.Close();
                    ms = new MemoryStream();
                }
            }
        }

        /// <summary>
        /// release all resources 
        /// </summary>
        public void ReleaseMicResources()
        {
            ms.Dispose();
            log.WriteLog("Microphone Resources Released");
        }

        public void m_BufferReady(Object o, EventArgs a)
        {
            m.GetData(data);
            ms.Write(data, 0, data.Length);
        }

        public void WriteWavHeader(Stream stream, int sampleRate, int length)
        {
            const int bitsPerSample = 16;
            const int bytesPerSample = bitsPerSample / 8;
            var encoding = System.Text.Encoding.UTF8;

            // ChunkID Contains the letters "RIFF" in ASCII form (0x52494646 big-endian form).
            stream.Write(encoding.GetBytes("RIFF"), 0, 4);

            // NOTE this will be filled in later
            stream.Write(BitConverter.GetBytes(0), 0, 4);

            // Format Contains the letters "WAVE"(0x57415645 big-endian form).
            stream.Write(encoding.GetBytes("WAVE"), 0, 4);

            // Subchunk1ID Contains the letters "fmt " (0x666d7420 big-endian form).
            stream.Write(encoding.GetBytes("fmt "), 0, 4);

            // Subchunk1Size 16 for PCM. This is the size of therest of the Subchunk which follows this number.
            stream.Write(BitConverter.GetBytes(16), 0, 4);

            // AudioFormat PCM = 1 (i.e. Linear quantization) Values other than 1 indicate some form of compression.
            stream.Write(BitConverter.GetBytes((short)1), 0, 2);

            // NumChannels Mono = 1, Stereo = 2, etc.
            stream.Write(BitConverter.GetBytes((short)1), 0, 2);

            // SampleRate 8000, 44100, etc.
            stream.Write(BitConverter.GetBytes(sampleRate), 0, 4);

            // ByteRate = SampleRate * NumChannels * BitsPerSample/8
            stream.Write(BitConverter.GetBytes(sampleRate * bytesPerSample), 0, 4);

            // BlockAlign NumChannels * BitsPerSample/8 The number of bytes for one sample including all channels.
            stream.Write(BitConverter.GetBytes((short)(bytesPerSample)), 0, 2);

            // BitsPerSample 8 bits = 8, 16 bits = 16, etc.
            stream.Write(BitConverter.GetBytes((short)(bitsPerSample)), 0, 2);

            // Subchunk2ID Contains the letters "data" (0x64617461 big-endian form).
            stream.Write(encoding.GetBytes("data"), 0, 4);

            // NOTE to be filled in later
            stream.Write(BitConverter.GetBytes(length), 0, 4);
        }



        /// <summary>
        /// records data from default microphone for user defined intervals
        /// </summary>
        /// <param name="samplingFrequency">time between data collection(in minutes)</param>
        /// <param name="collectionDuration">duration of data collection(in minutes)</param>
        /// <param name="count">number of times data is to be collected in chunks</param>
        /// <param name="bufferDuration">duration of microphone buffer data in milliseconds</param>
        public void RecordMicData(int samplingFrequency, int collectionDuration, int count, int bufferDuration)
        {
            this.count = count;
            source = new EventSource(samplingFrequency, collectionDuration);
            source.OnEvent += new EventSource.TickEventHandler(Source_OnEvent);
            source.Start();
            itCount = 0;
            source.OffEvent += new EventSource.TickEventHandler(Source_OffEvent);
            InitializeMic(bufferDuration);
           
        }

        void Source_OnEvent()
        {
            log.WriteLog("Collecting Microphone Data");
            if (itCount >= count)
            {
                log.WriteLog("Finished Recording Audio succesfully!");
                source.Stop();
            }
            else
            {
                RecordSound();
            }
            itCount++;
        }

        void Source_OffEvent()
        {
            log.WriteLog("Saving audio to file . . .");
            StopRecording();
            SaveSoundToFile(itCount);
        }
    }
}

