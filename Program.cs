using System;
using System.Windows.Forms;
using Recorder.MFCC;
using Recorder.Ramora;
namespace Recorder
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            if (Environment.OSVersion.Version.Major >= 6)
                SetProcessDPIAware();

           /* AudioSignal audio = new MFCC.AudioSignal();
            audio = AudioOperations.OpenAudioFile("C:\\Users\\DELL\\Desktop\\SPEAKER ID\\[2] SPEAKER IDENTIFICATION\\TEST CASES\\[1] SAMPLE\\Input sample\\ItIsPlausible_Rich_US_English.wav");
            audio = AudioOperations.RemoveSilence(audio);
            Sequence seq = AudioOperations.ExtractFeatures(audio);
            for (int i = 0; i < 13; i++)
            {
                for (int j = 0; j < seq.Frames.Length; j++)
                {
                    Console.Write(seq.Frames[j].Features[i] + "\t\t");
                }
                Console.WriteLine();
            }*/
            //Init.begin();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}
