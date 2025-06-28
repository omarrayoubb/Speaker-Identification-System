using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms.VisualStyles;
using System.IO;
using Recorder.MFCC;
using System.CodeDom;
using System.Windows.Forms;

namespace Recorder.Ramora
{
    struct Temp
    {
        public string name;
        public Sequence Seq;
        
    }

    static class Init
    {
        
        public static void Write(Temp template, string filePath)
        {
            //filePath = @"C:\Users\DELL\Desktop\SPEAKER ID\[2] SPEAKER IDENTIFICATION\STARTUP CODE\Speaker Identification Startup Code\[TEMPLATE] SpeakerID\Data\Templates.CSV";
            bool Header = !File.Exists(filePath);
            StreamWriter file = new StreamWriter(filePath, true, Encoding.UTF8);

            string[] features = new string[13 * template.Seq.Frames.Length];
            for (int i = 0; i < template.Seq.Frames.Length; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    features[j + i * 13] = template.Seq.Frames[i].Features[j].ToString("G17");
                }
            }
            string line = template.name + "," + string.Join(",", features);

            try
            {
                file.WriteLine(line);
            }
            catch
            {
                throw new Exception();
            }
            file.Dispose();
        }

        public static List<Temp> read()
        {
            string filePath = @"C:\Users\DELL\Desktop\SPEAKER ID\[2] SPEAKER IDENTIFICATION\STARTUP CODE\Speaker Identification Startup Code\[TEMPLATE] SpeakerID\Data\Templates.CSV";
            List<Temp> templates = new List<Temp>();
            StreamReader file = new StreamReader(filePath, encoding: Encoding.UTF8);
            string line;

            while ((line = file.ReadLine()) != null)
            {
                Temp temp1 = new Temp();
                string[] cols = line.Split(',');
                temp1.name = cols[0];
                cols = cols.Skip(1).ToArray();
                double[] data = cols.Select(double.Parse).ToArray();
                int pos = 0;
                temp1.Seq = new Sequence();
                int number_of_frames = data.Length / 13 + (data.Length % 13 == 0 ? 0 : 1);
                temp1.Seq.Frames = new MFCCFrame[number_of_frames];

                for (int i = 0; i < number_of_frames; i++)
                {
                    temp1.Seq.Frames[i] = new MFCCFrame();
                    for (int j = 0; j < 13; j++)
                    {
                        temp1.Seq.Frames[i].Features[j] = data[pos++];
                    }
                }
                templates.Add(temp1);
            }
            return templates;
        }

        public static void begin()
        {

            string path1 = @"C:\Users\DELL\Desktop\SPEAKER ID\[2] SPEAKER IDENTIFICATION\TEST CASES\[1] SAMPLE\Pruning Test\1 min\[Input] Why Study Algorithms - (1 min).wav";
            string path2 = @"C:\Users\DELL\Desktop\SPEAKER ID\[2] SPEAKER IDENTIFICATION\TEST CASES\[1] SAMPLE\Pruning Test\1 min\[Template] Big-Oh Notation (1 min).wav";

            TestCases.Test_sample();
            // TestCases.Test_sample();
            /*List<User> testCases = TestcaseLoader.LoadTestcase2Testing(@"C:\Users\city_lap\Downloads\[2] SPEAKER IDENTIFICATION-20250505T104725Z-1-001\[2] SPEAKER IDENTIFICATION\TEST CASES\[2] COMPLETE\Case2\Medium samples\TestingList1Sample.txt");
            Temp[] test = new Temp[17];
            for (int i = 0; i <= 15; i++) test[i] = new Temp();
            int idx = 0;

            foreach (var i in testCases)
            {
                foreach (var x in i.UserTemplates)
                {
                    test[idx].Seq = AudioOperations.ExtractFeatures(x);
                    test[idx++].name = i.UserName;
                }
            }

            List<User> TrainngSet = TestcaseLoader.LoadTestcase2Training(@"C:\Users\city_lap\Downloads\[2] SPEAKER IDENTIFICATION-20250505T104725Z-1-001\[2] SPEAKER IDENTIFICATION\TEST CASES\[2] COMPLETE\Case2\Medium samples\TrainingList5Samples.txt");
            Temp[] train = new Temp[5 * 16];
            for (int i = 0; i < train.Length; i++) train[i] = new Temp();

            foreach (var i in TrainngSet)
            {
                foreach (var x in i.UserTemplates)
                {
                    train[idx].Seq = AudioOperations.ExtractFeatures(x);
                    train[idx++].name = i.UserName;
                }
            }

            idx = 0;
            int accuracy = 0;
            for (int idxx = 0; idxx < 16; idxx++)
            {
                DTW bestResult = new DTW("", double.MaxValue);
                for (int i = 0; i < train.Length; i++)
                {
                    if (test[idxx].Seq == null) continue;
                    DTW temp = new DTW(train[i].name, double.MaxValue);
                    double ans = temp.DTW_with_pruning_by_search_paths(test[idxx].Seq, train[i].Seq, test[idxx].Seq.Frames.Length, train[i].Seq.Frames.Length, 28);
                    if (ans < bestResult.getVal())
                    {
                        bestResult = temp;
                    }
                }
                accuracy += (bestResult.getname() == test[idxx].name ? 1 : 0);
            }
            double acc = ((double)accuracy / (double)454.0) * 100;
            Console.WriteLine("Accurracy = " + acc);*/
        }
    } // This closes the Init class
} // This closes the namespace