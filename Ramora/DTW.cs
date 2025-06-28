using Accord.Math;
using AForge.Math.Metrics;
using NAudio.Gui;
using NAudio.Mixer;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Recorder.MFCC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;

namespace Recorder.Ramora
{
    public struct pair
    {

        public int frameNum;
        public int TempNum;
        //public double cost;

        public pair(int frameNum, int TempNum /*int cost*/)
        {
            //this.cost = cost;
            this.frameNum = frameNum;
            this.TempNum = TempNum;
        }
       
    };
    public struct boolPair
    {

        public double cost;
        public bool prev;
        //public double cost;

        

    };
    class DTW
    {
        double DissimilarityMeasure;
        string name;
        
        public DTW(string name, double d) 
        {
          
            this.name = name;
            DissimilarityMeasure = d;
        }
        public double getVal()
        {
            return DissimilarityMeasure;
        }
        public string getname()
        {
            return name;
        }

        

        private double Euclidian_Distance(double[] input_frame, double[] template_frame)
        {
            double answer = 0.0;

            for (int i = 0; i < 13; i++)
            {
                double diff = input_frame[i] - template_frame[i];
                answer += (diff * diff);
            }
            return Math.Sqrt(answer);
        }




         public double DTW_without_pruning(Sequence input, Sequence template, int n, int m)
         {
            double[,] memory = new double[2, m + 1];

            
            for (int i = 0; i <= m; i++)
            {

                memory[0, i] = double.MaxValue;
                
            }
            
            memory[0, 0] = 0.0;
            int curr = 0;
             for (int i = 1; i <= n; i++)
             {
                curr = 1 - curr;
                memory[curr, 0] = double.MaxValue;
                 for (int j = 1; j <= m; j++)
                 {
                     double distance = Euclidian_Distance(input.Frames[i - 1].Features, template.Frames[j - 1].Features);
                     double stretch_state = memory[1 - curr, j];
                     double shrink_state = (j >= 2) ? memory[1 - curr, j - 2] : double.MaxValue;
                     double normal_state = memory[1 - curr, j - 1];
                    memory[curr, j] = Math.Min(Math.Min(stretch_state, shrink_state), normal_state) + distance;
                 }
                 
             }
           
            return DissimilarityMeasure = memory[curr, m];
         }
 

      
        public double DTW_with_pruning_by_search_paths(Sequence input, Sequence template, int n, int m, int w)
        {
            w = Math.Max(w, 2 * Math.Abs(n - m));
            double[,] memory = new double[2, w + 100];

            int comm_w = w / 2;
            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < w + 100; i++)
                {
                    memory[j, i] = double.MaxValue;
                }
            }
            memory[0, comm_w] = 0.0;
            int curr = 0;
            for (int i = 1; i <= n; i++)
            {
                curr = 1 - curr;
                for (int j = Math.Max(1, i - comm_w); j <= Math.Min(m, i + comm_w); j++)
                {
                    int temp_w = j - i + comm_w;
                    double distance = Euclidian_Distance(input.Frames[i - 1].Features, template.Frames[j - 1].Features);
                    double val = double.MaxValue;
                    val = Math.Min(val, memory[1 - curr, temp_w]);
                    int stretch = j - (i - 1) + comm_w;
                    if (stretch >= 0)
                        val = Math.Min(val, memory[1 - curr, stretch]);
                    int shrink = (j - 2) - (i - 1 ) + comm_w;
                    if (shrink >= 0)
                    {
                        val = Math.Min(val, memory[1 - curr, shrink]);
                    }
                    memory[curr, temp_w] = val + distance;
                }
            }

            int temp = m - n + comm_w;
           // Console.WriteLine(temp);
            return DissimilarityMeasure = memory[curr, temp];
          
            
        }
         
        public double DTW_with_pruning_by_cost(Sequence input, Sequence template, int n, int m, double T)
        {
            Dictionary<int, double>[] memory = new Dictionary<int, double>[n + 5];
            for (int i = 0; i <= n; i++)
            {
                memory[i] = new Dictionary<int, double>();
            }
        
            memory[0][0] = 0.0;
            double Current_Cost = 0.0;
            for (int i = 1; i <= n; i++)
            {
                double bestFrameCost = double.MaxValue;
                for (int j = 1; j <= m; j++)
                {
                    double val = double.MaxValue;
                    double distance = Euclidian_Distance(input.Frames[i - 1].Features, template.Frames[j - 1].Features);
                    if (memory[i - 1].ContainsKey(j))
                    {
                        if (memory[i - 1][j] <= Current_Cost)
                            val = Math.Min(memory[i - 1][j], val);
                    }

                    if (memory[i - 1].ContainsKey(j - 1))
                    {
                        if (memory[i - 1][j - 1] <= Current_Cost)
                            val = Math.Min(memory[i - 1][j - 1], val);
                    }

                    if (memory[i - 1].ContainsKey(j - 2))
                    {
                        if (memory[i - 1][j - 2] <= Current_Cost)
                            val = Math.Min(memory[i - 1][j - 2], val);
                    }
                    if (val != double.MaxValue)
                    {
                        memory[i][j] = val + distance;
                        bestFrameCost = Math.Min(memory[i][j], bestFrameCost);
                    }
                }
                Current_Cost = bestFrameCost + T;
            }
            
            return DissimilarityMeasure = memory[n][m];
        }
        
        
        public double DTW_TIME_SYNC(Sequence input, List<Temp> people)
        {
            List<Sequence>templates = new List<Sequence>();
            for (int i = 0; i < people.Count; i++)
            { 
                templates.Add(people[i].Seq);
            }

            int input_size = input.Frames.Length;
            int temp_size = templates.Count;

            double max_ans = double.MaxValue;
            Queue<pair> q = new Queue<pair>();
            List<double>[] prev = new List<double>[temp_size]; 
            for (int i = 0; i < temp_size; i++)
            {
                q.Enqueue(new pair(1, i));
                q.Enqueue(new pair(2, i));
                prev[i] = new List<double>(templates[i].Frames.Length + 1);
                for (int j = 0; j < prev[i].Capacity; j++)
                {
                    prev[i].Add(double.MaxValue);
                }
                prev[i][0] = 0.0;
                
            }
            int levels = 1;
            while (q.Count != 0)
            {
                int size = q.Count;
                List<double>[] curr = new List<double>[temp_size];
                int mxFrames = 0;
                for (int i = 0; i < templates.Count; i++)
                {
                    curr[i] = new List<double>(templates[i].Frames.Length + 1);
                    for (int j = 0; j < curr[i].Capacity; j++)
                    {
                        curr[i].Add( double.MaxValue);
                    }
                    mxFrames = Math.Max(mxFrames, templates[i].Frames.Length + 1);
                }
                bool[,] vis = new bool[temp_size, mxFrames];
                while (size > 0)
                {
                    pair front = q.Dequeue();
                    int size_of_template = templates[front.TempNum].Frames.Length;
                    int frameNum = front.frameNum;
                    int tempNum = front.TempNum;
                    for (int i = 0; i < 3; i++)
                    {

                        if (frameNum + i <= size_of_template && !vis[tempNum, frameNum + i] && levels < input.Frames.Length)
                        {
                            q.Enqueue(new pair(frameNum + i, tempNum));
                            vis[tempNum, frameNum + i] = true;
                        }
                        
                    }
                    
                    double cost = Euclidian_Distance(input.Frames[levels - 1].Features, templates[tempNum].Frames[frameNum - 1].Features);
                    double stretch = ((prev[tempNum][frameNum] != double.MaxValue) ? prev[tempNum][frameNum]: double.MaxValue);
                    double normal = ((frameNum >= 1 && prev[tempNum][frameNum - 1] != double.MaxValue) ? prev[tempNum][frameNum - 1]: double.MaxValue);
                    double shrink = ((frameNum >= 2 && prev[tempNum][frameNum - 2] != double.MaxValue) ? prev[tempNum][frameNum - 2] : double.MaxValue);
                    curr[tempNum][frameNum] = cost + Math.Min(stretch, Math.Min(normal, shrink));
                   
                    size--;
                }

                prev = curr;
                levels++;
            }
            int idx = 0;
            for (int i = 0; i < templates.Count; i++)
            {
                if (max_ans > prev[i][templates[i].Frames.Length])
                {
                    idx = i;
                    max_ans = prev[i][templates[i].Frames.Length];
                }
            }
            this.name = people[idx].name;
            return DissimilarityMeasure = max_ans;

        }
    }
}
