using Microsoft.SqlServer.Server;
using Recorder.MFCC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using SharpDX.Multimedia;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using System.Runtime.InteropServices;

namespace Recorder.Ramora
{
    public static class TestCases
    {
        public static void Test_sample()
        {
            int pruning1 = 11;
            int pruning2 = 63;
            Stopwatch stopwatch = new Stopwatch();
            
            AudioSignal input = AudioOperations.OpenAudioFile(@"C:\Users\DELL\Desktop\SPEAKER ID\[2] SPEAKER IDENTIFICATION\TEST CASES\[1] SAMPLE\Input sample\ItIsPlausible_Rich_US_English.wav");

            AudioSignal temp1 = AudioOperations.OpenAudioFile(@"C:\Users\DELL\Desktop\SPEAKER ID\[2] SPEAKER IDENTIFICATION\TEST CASES\[1] SAMPLE\Training set\conspiracy_Crystal_US_English.wav");
            AudioSignal temp2 = AudioOperations.OpenAudioFile(@"C:\Users\DELL\Desktop\SPEAKER ID\[2] SPEAKER IDENTIFICATION\TEST CASES\[1] SAMPLE\Training set\conspiracy_Mike_US_English.wav");
            AudioSignal temp3 = AudioOperations.OpenAudioFile(@"C:\Users\DELL\Desktop\SPEAKER ID\[2] SPEAKER IDENTIFICATION\TEST CASES\[1] SAMPLE\Training set\conspiracy_Rich_US_English.wav");
            AudioSignal temp4 = AudioOperations.OpenAudioFile(@"C:\Users\DELL\Desktop\SPEAKER ID\[2] SPEAKER IDENTIFICATION\TEST CASES\[1] SAMPLE\Training set\plausible_Crystal_US_English.wav");
            AudioSignal temp5 = AudioOperations.OpenAudioFile(@"C:\Users\DELL\Desktop\SPEAKER ID\[2] SPEAKER IDENTIFICATION\TEST CASES\[1] SAMPLE\Training set\plausible_Mike_US_English.wav");
            AudioSignal temp6 = AudioOperations.OpenAudioFile(@"C:\Users\DELL\Desktop\SPEAKER ID\[2] SPEAKER IDENTIFICATION\TEST CASES\[1] SAMPLE\Training set\plausible_Rich_US_English.wav");

            Sequence inputseq = AudioOperations.ExtractFeatures(input);
            Sequence tempseq = AudioOperations.ExtractFeatures(temp1);
            Sequence tempseq2 = AudioOperations.ExtractFeatures(temp2);
            Sequence tempseq3 = AudioOperations.ExtractFeatures(temp3);
            Sequence tempseq4 = AudioOperations.ExtractFeatures(temp4);
            Sequence tempseq5 = AudioOperations.ExtractFeatures(temp5);
            Sequence tempseq6 = AudioOperations.ExtractFeatures(temp6);
           
            List<String> outt = new List<string>();
            outt.Add("Crystal");
            outt.Add("Mike");
            outt.Add("Rich");
            outt.Add("Crystal");
            outt.Add("Mike");
            outt.Add("Rich");
            List<Sequence> list = new List<Sequence>();
            list.Add(tempseq);
            list.Add(tempseq2);
            list.Add(tempseq3);
            list.Add(tempseq4);
            list.Add(tempseq5);
            list.Add(tempseq6);

            List<Temp> li = new List<Temp>();
            for (int i = 0; i < 6; i++)
            {
                Temp n = new Temp();
                n.name = outt[i];
                n.Seq = list[i];
                li.Add(n);
            }
            DTW best_rest = new DTW("", double.MaxValue);
            DTW best_rest2 = new DTW("", double.MaxValue);
            DTW best_rest3 = new DTW("", double.MaxValue);

            DTW best_rest4 = new DTW("", double.MaxValue);
            best_rest4.DTW_TIME_SYNC(inputseq, li);
            Console.WriteLine("name = " + best_rest4.getname());
            Console.WriteLine("Distance = " + best_rest4.getVal());
            for (int i = 0; i < 6; i++)
            {
                DTW d = new DTW(outt[i], double.MaxValue);
                DTW t = new DTW(outt[i], double.MaxValue);
                DTW s = new DTW(outt[i], double.MaxValue);

                double ans = d.DTW_without_pruning(inputseq, list[i], inputseq.Frames.Length, list[i].Frames.Length);
                double ans2 = t.DTW_with_pruning_by_search_paths(inputseq, list[i], inputseq.Frames.Length, list[i].Frames.Length, pruning1);
                double ans3 = s.DTW_with_pruning_by_search_paths(inputseq, list[i], inputseq.Frames.Length, list[i].Frames.Length, pruning2);

 
                if (ans < best_rest.getVal())
                {
                    best_rest = d;
                }
                if (ans2 < best_rest2.getVal())
                {
                    best_rest2 = t;

                }
                if (ans3 < best_rest3.getVal())
                {
                    best_rest3 = s;
                }
            }
            stopwatch.Stop();
            Console.WriteLine("Name = " + best_rest.getname());
            Console.WriteLine("Result Without Pruning = " + best_rest.getVal());
            Console.WriteLine("Result With Pruning(11) = " + best_rest2.getVal());
            Console.WriteLine("Result With Pruning(63) = " + best_rest3.getVal());
           
        }
        public static void Test_Pruning(string path, string tempPath, int Pruning_Width)
        {
            if (Pruning_Width < 0)
            {
                Console.WriteLine("Error: pruning must be bigger than or equal to 0 ");
            }

            AudioSignal audio = new AudioSignal(), audio2 = new AudioSignal();
            audio = AudioOperations.OpenAudioFile(path);

            audio = AudioOperations.RemoveSilence(audio);
            audio2 = AudioOperations.OpenAudioFile(tempPath);
            audio2 = AudioOperations.RemoveSilence(audio2);
            Sequence inn = AudioOperations.ExtractFeatures(audio);
            Sequence outt = AudioOperations.ExtractFeatures(audio2);
            Stopwatch stopwatch = new Stopwatch();

            DTW Answer = new DTW("", double.MaxValue);

            stopwatch = Stopwatch.StartNew();
            double distance = Answer.DTW_with_pruning_by_search_paths(inn, outt, inn.Frames.Length, outt.Frames.Length, Pruning_Width);
            stopwatch.Stop();
            Console.WriteLine("Time elapsed = " + (double)stopwatch.ElapsedMilliseconds);
            Console.WriteLine("Distance = " + distance);
        }

        public static void load_complete_tests(int complete_test_Cases, string path1, string path2, int prune)
        {

            switch (complete_test_Cases)
            {
                case 1:
                    load_small(path1, path2, prune);
                    break;
                case 2:
                    load_medium(path1, path2, prune);
                    break;
                case 3:
                    load_large(path1, path2, prune);
                    break;
            }
        }
        private static void load_small(string path1, string path2, int pruning)
        {
            path1 = @"C:\Users\DELL\Desktop\SPEAKER ID\[2] SPEAKER IDENTIFICATION\TEST CASES\[2] COMPLETE\Case1\Small samples\TrainingList.txt";
            path2 = @"C:\Users\DELL\Desktop\SPEAKER ID\[2] SPEAKER IDENTIFICATION\TEST CASES\[2] COMPLETE\Case1\Small samples\TestingList5Samples.txt";
            pruning = 23;
            Stopwatch stopwatch = new Stopwatch();
            Stopwatch stopwatch2 = new Stopwatch();
            List<User> users = new List<User>();
            stopwatch = Stopwatch.StartNew();
            users = TestcaseLoader.LoadTestcase1Testing(path2);
            stopwatch.Stop();
            List<User> Users = new List<User>();
            stopwatch2 = Stopwatch.StartNew(); 

            Users = TestcaseLoader.LoadTestcase1Training(path1);
            stopwatch2.Stop();
            
            List<Temp> test = new List<Temp>();
            List<Temp> train = new List<Temp>();

            stopwatch.Start();
            foreach (var i in users)
            {
                foreach (var x in i.UserTemplates)
                {
                    Temp temp = new Temp();
                    temp.name = i.UserName;
                    Sequence seq = new Sequence();
                    seq = AudioOperations.ExtractFeatures(x);
                    temp.Seq = seq;
                    test.Add(temp);
                }
            }
            stopwatch.Stop();
            stopwatch2.Start();
            foreach (var i in Users)
            {
                foreach (var x in i.UserTemplates)
                {
                    Temp temp = new Temp();
                    temp.name = i.UserName;
                    temp.Seq = AudioOperations.ExtractFeatures(x);
                    train.Add(temp);
                }
            }
            for (int i = 0; i < train.Count; i++)
            {
                Init.Write(train[i], @"C:\Users\DELL\Desktop\SPEAKER ID\[2] SPEAKER IDENTIFICATION\STARTUP CODE\Speaker Identification Startup Code\[TEMPLATE] SpeakerID\Data\smalltest.csv");
            }
            /*stopwatch2.Stop();
            Stopwatch stopwatchMatching = new Stopwatch();
            Stopwatch stopwatchMatching2 = new Stopwatch();

            stopwatch.Start();
            int accurracy = 0;
            int accurracyWith = 0;
            for (int i = 0; i < test.Count; i++)
            {
                DTW bestwithout = new DTW("", double.MaxValue);
                DTW bestwith = new DTW("", double.MaxValue);
                for (int j = 0; j < train.Count; j++)
                {
                    DTW d = new DTW(train[j].name, double.MaxValue);
                    DTW t = new DTW(train[j].name, double.MaxValue);
                    if (test[i].Seq == null || train[j].Seq == null) continue;
                    stopwatchMatching.Start();
                    double without = d.DTW_without_pruning(test[i].Seq, train[j].Seq, test[i].Seq.Frames.Length, train[j].Seq.Frames.Length);
                    stopwatchMatching.Stop();
                    stopwatchMatching2.Start();
                    double with = t.DTW_with_pruning_by_search_paths(test[i].Seq, train[j].Seq, test[i].Seq.Frames.Length, train[j].Seq.Frames.Length, pruning);
                    stopwatchMatching2.Stop();
                    if (without <= bestwithout.getVal())
                    {
                        bestwithout = d;
                    }
                    if (with <= bestwith.getVal())
                    {
                        bestwith = t;
                    }

                }
                accurracy += (bestwithout.getname() == test[i].name ? 1 : 0);
                accurracyWith += (bestwith.getname() == test[i].name ? 1 : 0);

            }
            stopwatch.Stop();
            double acc1 = (double)accurracy / (double)test.Count;
            double acc2 = (double)accurracyWith / (double)test.Count;
            Console.WriteLine("Accurracy of DTW Without Pruning = " + acc1 * 100.0);
            Console.WriteLine("Accurracy of DTW With Pruning = " + acc2 * 100.0);
            Console.WriteLine("Time elapsed in Load And Extract test Files and DTW With and without Pruning " + stopwatch.Elapsed);
            Console.WriteLine("Time elapsed in matching DTW without PRunig " + stopwatchMatching.Elapsed);
            Console.WriteLine("Time elapsed in matching DTW with pruning " + stopwatchMatching2.Elapsed);*/


        }
        private static void load_medium(string path1, string path2, int prune)
        {
            path1 = @"C:\Users\DELL\Desktop\SPEAKER ID\[2] SPEAKER IDENTIFICATION\TEST CASES\[2] COMPLETE\Case2\Medium samples\TestingList1Sample.txt";
            path2 = @"C:\Users\DELL\Desktop\SPEAKER ID\[2] SPEAKER IDENTIFICATION\TEST CASES\[2] COMPLETE\Case2\Medium samples\TrainingList5Samples.txt";
            prune = 55;
            Stopwatch stopwatch = new Stopwatch();
            Stopwatch stopwatch2 = new Stopwatch();
            List<User> users = new List<User>();
            stopwatch = Stopwatch.StartNew();
            users = TestcaseLoader.LoadTestcase1Testing(path1);
            stopwatch.Stop();
            List<User> Users = new List<User>();
            stopwatch2 = Stopwatch.StartNew();

            Users = TestcaseLoader.LoadTestcase1Training(path2);
            stopwatch2.Stop();

            List<Temp> test = new List<Temp>();
            List<Temp> train = new List<Temp>();

            stopwatch.Start();
            foreach (var i in users)
            {
                foreach (var x in i.UserTemplates)
                {
                    Temp temp = new Temp();
                    temp.name = i.UserName;
                    Sequence seq = new Sequence();
                    seq = AudioOperations.ExtractFeatures(x);
                    temp.Seq = seq;
                    test.Add(temp);
                }
            }
            stopwatch.Stop();
            stopwatch2.Start();
            foreach (var i in Users)
            {
                foreach (var x in i.UserTemplates)
                {
                    Temp temp = new Temp();
                    temp.name = i.UserName;
                    temp.Seq = AudioOperations.ExtractFeatures(x);
                    train.Add(temp);
                }
            }
            /*for (int i = 0; i < test.Count; i++)
            {
                Init.Write(train[i], @"C:\Users\DELL\Desktop\SPEAKER ID\[2] SPEAKER IDENTIFICATION\STARTUP CODE\Speaker Identification Startup Code\[TEMPLATE] SpeakerID\Data\medium.csv");
            }*/
            stopwatch2.Stop();
            Stopwatch stopwatchMatching = new Stopwatch();


            stopwatch.Start();
            int accurracyWith = 0;
            for (int i = 0; i < test.Count; i++)
            {

                DTW bestwith = new DTW("", double.MaxValue);
                for (int j = 0; j < train.Count; j++)
                {
                    DTW t = new DTW(train[j].name, double.MaxValue);
                    if (test[i].Seq == null || train[j].Seq == null) continue;
                    stopwatchMatching.Start();
                    double with = t.DTW_with_pruning_by_search_paths(test[i].Seq, train[j].Seq, test[i].Seq.Frames.Length, train[j].Seq.Frames.Length, prune);
                    stopwatchMatching.Stop();


                    if (with < bestwith.getVal())
                    {
                        bestwith = t;
                    }

                }

                accurracyWith += (bestwith.getname() == test[i].name ? 1 : 0);
                Console.WriteLine(bestwith.getname() == test[i].name ? 1 : 0);

            }
            stopwatch.Stop();
            double acc2 = (double)accurracyWith / (double)test.Count;

            Console.WriteLine("Accurracy of DTW With Pruning = " + acc2 * 100.0);
            Console.WriteLine("Time elapsed in Load And Extract test Files and DTW With and without Pruning " + stopwatch.Elapsed);
            Console.WriteLine("Time elapsed in matching DTW with Pruning " + stopwatchMatching.Elapsed);


        }

        private static void load_large(string path1, string path2, int pruning)
        {
           /* string path1 = @"C:\Users\DELL\Desktop\SPEAKER ID\[2] SPEAKER IDENTIFICATION\TEST CASES\[2] COMPLETE\Case3\Large samples\TestingList.txt";
            string path2 = @"C: \Users\DELL\Desktop\SPEAKER ID\[2] SPEAKER IDENTIFICATION\TEST CASES\[2] COMPLETE\Case3\Large samples\TrainingList1Sample.txt";
            int pruning = 11;*/
            Stopwatch stopwatch = new Stopwatch();
            Stopwatch stopwatch2 = new Stopwatch();
            List<User> users = new List<User>();
            stopwatch = Stopwatch.StartNew();
            users = TestcaseLoader.LoadTestcase1Testing(path1);
            stopwatch.Stop();
            List<User> Users = new List<User>();
            stopwatch2 = Stopwatch.StartNew();

            Users = TestcaseLoader.LoadTestcase1Training(path2);
            stopwatch2.Stop();

            List<Temp> test = new List<Temp>();
            List<Temp> train = new List<Temp>();

            stopwatch.Start();
            foreach (var i in users)
            {
                foreach (var x in i.UserTemplates)
                {
                    Temp temp = new Temp();
                    temp.name = i.UserName;
                    Sequence seq = new Sequence();
                    seq = AudioOperations.ExtractFeatures(x);
                    temp.Seq = seq;
                    test.Add(temp);
                }
            }
            stopwatch.Stop();
            stopwatch2.Start();
            foreach (var i in Users)
            {
                foreach (var x in i.UserTemplates)
                {
                    Temp temp = new Temp();
                    temp.name = i.UserName;
                    temp.Seq = AudioOperations.ExtractFeatures(x);
                    train.Add(temp);
                }
            }
            for (int i = 0; i < test.Count; i++)
            {
                Init.Write(train[i], @"C:\Users\DELL\Desktop\SPEAKER ID\[2] SPEAKER IDENTIFICATION\STARTUP CODE\Speaker Identification Startup Code\[TEMPLATE] SpeakerID\Data\large.csv");
            }
            /* stopwatch2.Stop();
             Stopwatch stopwatchMatching = new Stopwatch();


             stopwatch.Start();
             int accurracyWith = 0;
             for (int i = 0; i < test.Count; i++)
             {

                 DTW bestwith = new DTW("", double.MaxValue);
                 for (int j = 0; j < train.Count; j++)
                 {
                     DTW t = new DTW(train[j].name, double.MaxValue);
                     if (test[i].Seq == null || train[j].Seq == null) continue;
                     stopwatchMatching.Start();
                     double with = t.DTW_with_pruning_by_search_paths(test[i].Seq, train[j].Seq, test[i].Seq.Frames.Length, train[j].Seq.Frames.Length, pruning);
                     stopwatchMatching.Stop();


                     if (with < bestwith.getVal())
                     {
                         bestwith = t;
                     }

                 }

                 accurracyWith += (bestwith.getname() == test[i].name ? 1 : 0);
                 Console.WriteLine(bestwith.getname() == test[i].name ? 1 : 0);

             }
             stopwatch.Stop();
             double acc2 = (double)accurracyWith / (double)test.Count;

             Console.WriteLine("Accurracy of DTW With Pruning = " + acc2 * 100.0);
             Console.WriteLine("Time elapsed in Load And Extract test Files and DTW With and without Pruning " + stopwatch.Elapsed);
             Console.WriteLine("Time elapsed in matching DTW with Pruning " + stopwatchMatching.Elapsed);*/
        }

    }
}
