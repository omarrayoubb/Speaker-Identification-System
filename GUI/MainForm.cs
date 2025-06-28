using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Accord.Audio;
using Accord.Audio.Formats;
using Accord.DirectSound;
using Accord.Audio.Filters;
using Recorder.Recorder;
using Recorder.MFCC;
using Recorder.Ramora;
using System.Collections.Generic;
using System.Linq;

namespace Recorder
{
    /// <summary>
    ///   Speaker Identification application.
    /// </summary>
    /// 
    public partial class MainForm : Form
    {
        /// <summary>
        /// Data of the opened audio file, contains:
        ///     1. signal data
        ///     2. sample rate
        ///     3. signal length in ms
        /// </summary>
        private AudioSignal signal = null;
        Sequence seq = null;

        private string path;

        private Encoder encoder;
        private Decoder decoder;

        private bool isRecorded;

        public MainForm()
        {
            InitializeComponent();

            // Configure the wavechart
            chart.SimpleMode = true;
            chart.AddWaveform("wave", Color.Green, 1, false);
            updateButtons();
        }


        /// <summary>
        ///   Starts recording audio from the sound card
        /// </summary>
        /// 
        private void btnRecord_Click(object sender, EventArgs e)
        {
            isRecorded = true;
            this.encoder = new Encoder(source_NewFrame, source_AudioSourceError);
            this.encoder.Start();
            updateButtons();
        }

        /// <summary>
        ///   Plays the recorded audio stream.
        /// </summary>
        /// 
        private void btnPlay_Click(object sender, EventArgs e)
        {
            InitializeDecoder();
            // Configure the track bar so the cursor
            // can show the proper current position
            if (trackBar1.Value < this.decoder.frames)
                this.decoder.Seek(trackBar1.Value);
            trackBar1.Maximum = this.decoder.samples;
            this.decoder.Start();
            updateButtons();
        }

        private void InitializeDecoder()
        {
            if (isRecorded)
            {
                // First, we rewind the stream
                this.encoder.stream.Seek(0, SeekOrigin.Begin);
                this.decoder = new Decoder(this.encoder.stream, this.Handle, output_AudioOutputError, output_FramePlayingStarted, output_NewFrameRequested, output_PlayingFinished);
            }
            else
            {
                this.decoder = new Decoder(this.path, this.Handle, output_AudioOutputError, output_FramePlayingStarted, output_NewFrameRequested, output_PlayingFinished);
            }
        }

        /// <summary>
        ///   Stops recording or playing a stream.
        /// </summary>
        /// 
        private void btnStop_Click(object sender, EventArgs e)
        {
            Stop();
            updateButtons();
            updateWaveform(new float[BaseRecorder.FRAME_SIZE], BaseRecorder.FRAME_SIZE);
        }

        /// <summary>
        ///   This callback will be called when there is some error with the audio 
        ///   source. It can be used to route exceptions so they don't compromise 
        ///   the audio processing pipeline.
        /// </summary>
        /// 
        private void source_AudioSourceError(object sender, AudioSourceErrorEventArgs e)
        {
            throw new Exception(e.Description);
        }

        /// <summary>
        ///   This method will be called whenever there is a new input audio frame 
        ///   to be processed. This would be the case for samples arriving at the 
        ///   computer's microphone
        /// </summary>
        /// 
        private void source_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            this.encoder.addNewFrame(eventArgs.Signal);
            updateWaveform(this.encoder.current, eventArgs.Signal.Length);
        }


        /// <summary>
        ///   This event will be triggered as soon as the audio starts playing in the 
        ///   computer speakers. It can be used to update the UI and to notify that soon
        ///   we will be requesting additional frames.
        /// </summary>
        /// 
        private void output_FramePlayingStarted(object sender, PlayFrameEventArgs e)
        {
            updateTrackbar(e.FrameIndex);

            if (e.FrameIndex + e.Count < this.decoder.frames)
            {
                int previous = this.decoder.Position;
                decoder.Seek(e.FrameIndex);

                Signal s = this.decoder.Decode(e.Count);
                decoder.Seek(previous);

                updateWaveform(s.ToFloat(), s.Length);
            }
        }

        /// <summary>
        ///   This event will be triggered when the output device finishes
        ///   playing the audio stream. Again we can use it to update the UI.
        /// </summary>
        /// 
        private void output_PlayingFinished(object sender, EventArgs e)
        {
            updateButtons();
            updateWaveform(new float[BaseRecorder.FRAME_SIZE], BaseRecorder.FRAME_SIZE);
        }

        /// <summary>
        ///   This event is triggered when the sound card needs more samples to be
        ///   played. When this happens, we have to feed it additional frames so it
        ///   can continue playing.
        /// </summary>
        /// 
        private void output_NewFrameRequested(object sender, NewFrameRequestedEventArgs e)
        {
            this.decoder.FillNewFrame(e);
        }


        void output_AudioOutputError(object sender, AudioOutputErrorEventArgs e)
        {
            throw new Exception(e.Description);
        }

        /// <summary>
        ///   Updates the audio display in the wave chart
        /// </summary>
        /// 
        private void updateWaveform(float[] samples, int length)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    chart.UpdateWaveform("wave", samples, length);
                }));
            }
            else
            {
                if (this.encoder != null) { chart.UpdateWaveform("wave", this.encoder.current, length); }
            }
        }

        /// <summary>
        ///   Updates the current position at the trackbar.
        /// </summary>
        /// 
        private void updateTrackbar(int value)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    trackBar1.Value = Math.Max(trackBar1.Minimum, Math.Min(trackBar1.Maximum, value));
                }));
            }
            else
            {
                trackBar1.Value = Math.Max(trackBar1.Minimum, Math.Min(trackBar1.Maximum, value));
            }
        }

        private void updateButtons()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(updateButtons));
                return;
            }

            if (this.encoder != null && this.encoder.IsRunning())
            {
                btnAdd.Enabled = false;
                btnIdentify.Enabled = false;
                btnPlay.Enabled = false;
                btnStop.Enabled = true;
                btnRecord.Enabled = false;
                trackBar1.Enabled = false;
                
            }
            else if (this.decoder != null && this.decoder.IsRunning())
            {
                btnAdd.Enabled = false;
                btnIdentify.Enabled = false;
                btnPlay.Enabled = false;
                btnStop.Enabled = true;
                btnRecord.Enabled = false;
                trackBar1.Enabled = true;
            }
            else
            {
                btnAdd.Enabled = this.path != null || this.encoder != null;
                btnIdentify.Enabled = (this.signal != null || this.encoder != null);
                btnPlay.Enabled = this.path != null || this.encoder != null;//stream != null;
                btnStop.Enabled = false;
                btnRecord.Enabled = true;
                trackBar1.Enabled = this.decoder != null;
                trackBar1.Value = 0;
            }
        }



        private void MainFormFormClosed(object sender, FormClosedEventArgs e)
        {
            Stop();
        }

        private void saveFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.encoder != null)
            {
                Stream fileStream = saveFileDialog1.OpenFile();
                this.encoder.Save(fileStream);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog(this);
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            if (this.encoder != null) { lbLength.Text = String.Format("Length: {0:00.00} sec.", this.encoder.duration / 1000.0); }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            if (open.ShowDialog() == DialogResult.OK)
            {
                isRecorded = false;
                path = open.FileName;
                //Open the selected audio file
                signal = AudioOperations.OpenAudioFile(path);
                signal = AudioOperations.RemoveSilence(signal);
                seq = AudioOperations.ExtractFeatures(signal);
                for (int i = 0; i < seq.Frames.Length; i++)
                {
                    for (int j = 0; j < 13; j++)
                    {

                        if (double.IsNaN(seq.Frames[i].Features[j]) || double.IsInfinity(seq.Frames[i].Features[j]))
                            throw new Exception("NaN");
                    }
                }
                updateButtons();

            }
        }

        private void Stop()
        {
            if (this.encoder != null) { this.encoder.Stop(); }
            if (this.decoder != null) { this.decoder.Stop(); }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (this.encoder == null)
                return;

            // Create a simple input dialog
            Form prompt = new Form()
            {
                Width = 300,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Speaker Identification",
                StartPosition = FormStartPosition.CenterScreen
            };

            Label textLabel = new Label() { Left = 20, Top = 20, Text = "Enter speaker name:" };
            TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 250, Text = "Speaker1" };
            Button confirmation = new Button() { Text = "OK", Left = 200, Width = 70, Top = 80, DialogResult = DialogResult.OK };

            confirmation.Click += (s, args) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            if (prompt.ShowDialog() != DialogResult.OK || string.IsNullOrWhiteSpace(textBox.Text))
                return;

            string speakerName = textBox.Text;


            try
            {
                Temp T = new Temp();
                T.Seq = ProcessRecordedAudio();
                T.name = speakerName;
                string filePath =@"C:\Users\DELL\Desktop\SPEAKER ID\[2] SPEAKER IDENTIFICATION\STARTUP CODE\Speaker Identification Startup Code\[TEMPLATE] SpeakerID\Data\Templates.csv";
                try
                {
                    Init.Write(T, filePath);
                    MessageBox.Show($"Data saved to: {filePath}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Save failed: {ex.Message}");
                }
            }
            finally
            {
                //if (File.Exists(tempFile))
                //{
                //    File.Delete(tempFile);
                //}
            }
        }



        private void loadTrain1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.ShowDialog();

            var hobba = TestcaseLoader.LoadTestcase2Training(fileDialog.FileName);
        }


        private Sequence ProcessRecordedAudio()
        {
            if (this.encoder == null)
                throw new InvalidOperationException("No audio recording available");

            string tempFile = Path.GetTempFileName();
            FileStream fileStream = null;

            try
            {
                // 1. Save recording
                fileStream = File.Create(tempFile);
                this.encoder.Save(fileStream);

                // 2. Close the file handle immediately
                fileStream.Dispose();  // ← Critical addition
                fileStream = null;

                // 3. Process audio
                this.signal = AudioOperations.OpenAudioFile(tempFile);
                this.signal = AudioOperations.RemoveSilence(this.signal);
                return AudioOperations.ExtractFeatures(this.signal);
            }
            finally
            {
                // 4. Clean up in correct order
                fileStream?.Dispose(); // If still open

                try
                {
                    if (File.Exists(tempFile))
                        File.Delete(tempFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Cleanup warning: {ex.Message}");
                    // Optional: Add retry logic here if needed
                }
            }
        }

        private void btnIdentify_Click(object sender, EventArgs e)
        {
            //if (signal == null || seq == null)
            //{
            //    MessageBox.Show("Please load or record audio first!");
            //    return;
            //}

            Form methodDialog = new Form()
            {
                Width = 350,
                Height = 250,
                Text = "Select DTW Method",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent
            };

            Label lblPrompt = new Label()
            {
                Text = "Choose DTW calculation method:",
                Left = 20,
                Top = 20,
                Width = 300
            };

            ComboBox cmbMethods = new ComboBox()
            {
                Left = 20,
                Top = 50,
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Add available DTW methods
            cmbMethods.Items.AddRange(new string[] {
                "1. Full DTW (No Pruning)",
                "2. DTW with Pruning by Search Paths",
                "3. DTW with Pruning by Cost",
                "4. DTW with Time Sync",
                "5. DWT with Time Sync by Search paths"
            });

            Button btnConfirm = new Button()
            {
                Text = "Identify Speaker",
                Left = 20,
                Top = 150,
                Width = 300,
                DialogResult = DialogResult.OK
            };



            // Create dialog for method selection
            List<Temp> templates = Init.read();

            if (templates == null || templates.Count == 0)
            {
                MessageBox.Show("No speaker templates available!");
                return;
            }




            methodDialog.Controls.AddRange(new Control[] { lblPrompt, cmbMethods, btnConfirm });
            methodDialog.AcceptButton = btnConfirm;

            if (methodDialog.ShowDialog(this) == DialogResult.OK)
            {
                if (cmbMethods.SelectedIndex == -1)
                {
                    MessageBox.Show("Please select a method!");
                    return;
                }

                // Load templates (assuming you have them stored)

                // Find best match
                DTW bestMatch = new DTW("KK", double.MaxValue);
                string selectedMethod = cmbMethods.SelectedItem.ToString();

                //string tempFile = Path.GetTempFileName();
                //using (FileStream fileStream = File.Create(tempFile))
                //{
                //    this.encoder.Save(fileStream);
                //}

                //this.signal = AudioOperations.OpenAudioFile(tempFile);
                //signal = AudioOperations.RemoveSilence(signal);
                //this.seq = AudioOperations.ExtractFeatures(signal);
                Sequence seq = ProcessRecordedAudio();

                if (cmbMethods.SelectedIndex == 3)
                {

 
                    //double distance = bestMatch.DTW_TIME_SYNC(seq, templates);
                    Console.WriteLine("distance: ");
                    //Console.WriteLine(distance);
                    Console.WriteLine("best match value: ");
                    Console.WriteLine(bestMatch.getVal());

                }
                foreach (var template in templates)
                {
                    if (template.Seq == null) continue;

                    DTW current = new DTW(template.name, double.MaxValue);
                    double distance = double.MaxValue;

                    try
                    {
                        if (cmbMethods.SelectedIndex == 0)
                        {
                            distance = current.DTW_without_pruning(seq, template.Seq, seq.Frames.Length, template.Seq.Frames.Length);
                        }
                        if (cmbMethods.SelectedIndex == 1)
                        {

                            distance = current.DTW_with_pruning_by_search_paths(
                                    seq, template.Seq,
                                    seq.Frames.Length,
                                    template.Seq.Frames.Length,
                                    70); // You can make this parameter configurable
                        }
                        if (cmbMethods.SelectedIndex == 2)
                        {
                            distance = current.DTW_with_pruning_by_cost(
                                    seq, template.Seq,
                                    seq.Frames.Length,
                                    template.Seq.Frames.Length,
                                    100); // Window size parameter

                        }
                        if (distance < bestMatch.getVal())
                        {
                            bestMatch = current;
                        }


                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error processing {template.name}: {ex.Message}");
                        continue;
                    }
                }


                // Show results
                MessageBox.Show($"Identified Speaker: {bestMatch.getname()}\n" +
                               $"Using Method: {selectedMethod}\n" +
                               $"Distance Score: {bestMatch.getVal():F2}",
                               "Identification Result",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Information);
            }
        }



        private void test_cases_btn(object sender, EventArgs e)
        {
            Button callingButton = sender as Button;
            using (var form = new Form())
            {
                form.Text = "Select Test Case";
                form.Size = new Size(350, 250);
                form.StartPosition = FormStartPosition.CenterParent;

                // Create radio buttons for all options
                var options = new Dictionary<string, RadioButton>()
        {
            {"Test_sample", new RadioButton() { Text = "test sample Case", Location = new Point(20, 20) }},
            {"Pruning", new RadioButton() { Text = "Pruning Test", Location = new Point(20, 50) }},
            {"Small", new RadioButton() { Text = "Small Test Case", Location = new Point(20, 80), Checked = true }},
            {"Medium", new RadioButton() { Text = "Medium Test Case", Location = new Point(20, 110) }},
            {"Large", new RadioButton() { Text = "Large Test Case", Location = new Point(20, 140) }}
        };

                // Create threshold input (only needed for pruning)
                var thresholdLabel = new Label()
                {
                    Text = "Pruning Width:",
                    Location = new Point(150, 20),
                    AutoSize = true,
                    Enabled = false
                };

                var thresholdInput = new NumericUpDown()
                {
                    Location = new Point(150, 40),
                    Width = 100,
                    Minimum = 0,
                    Maximum = 100,
                    Value = 23,
                    Enabled = false
                };

                // Path inputs
                var path1Label = new Label() { Text = "Path 1:", Location = new Point(150, 70), AutoSize = true };
                var path1Input = new TextBox() { Location = new Point(150, 90), Width = 180 };

                var path2Label = new Label() { Text = "Path 2:", Location = new Point(150, 120), AutoSize = true };
                var path2Input = new TextBox() { Location = new Point(150, 140), Width = 180 };

                // Enable/disable controls based on selection
                foreach (var option in options)
                {
                    option.Value.CheckedChanged += (s, ev) =>
                    {
                bool enableThreshold = option.Key != "Test_sample";
                thresholdLabel.Enabled = enableThreshold;
                thresholdInput.Enabled = enableThreshold;

                        // Set default paths based on selection
                        if (option.Key == "Small")
                        {
                            path1Input.Text = @"C:\Users\city_lap\Desktop\TEST CASES\[2] COMPLETE\Case2\Medium samples\TrainingList.txt";
                            path2Input.Text = @"C:\Users\city_lap\Desktop\TEST CASES\[2] COMPLETE\Case2\Medium samples\TrainingList5Samples.txt";
                        }
                        else if (option.Key == "Medium")
                        {
                            path1Input.Text = @"C:\Users\city_lap\Desktop\TEST CASES\[2] COMPLETE\Case2\Medium samples\TestingList1Sample.txt";
                            path2Input.Text = @"C:\Users\city_lap\Desktop\TEST CASES\[2] COMPLETE\Case2\Medium samples\TrainingList5Samples.txt";
                        }
                    };
                }

                // Add OK and Cancel buttons
                var okButton = new Button()
                {
                    Text = "Run Test",
                    DialogResult = DialogResult.OK,
                    Location = new Point(50, 170)
                };

                var cancelButton = new Button()
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(150, 170)
                };

                // Add browse buttons for paths
                var browse1 = new Button() { Text = "Browse", Location = new Point(340, 90), Size = new Size(75, 23) };
                browse1.Click += (s, evt) =>
                {
                    var dialog = new OpenFileDialog();
                    if (dialog.ShowDialog() == DialogResult.OK)
                        path1Input.Text = dialog.FileName;
                };

                var browse2 = new Button() { Text = "Browse", Location = new Point(340, 140), Size = new Size(75, 23) };
                browse2.Click += (s, evt) =>
                {
                    var dialog = new OpenFileDialog();
                    if (dialog.ShowDialog() == DialogResult.OK)
                        path2Input.Text = dialog.FileName;
                };

                // Add controls to the form
                foreach (var option in options.Values)
                {
                    form.Controls.Add(option);
                }
                form.Controls.Add(thresholdLabel);
                form.Controls.Add(thresholdInput);
                form.Controls.Add(path1Label);
                form.Controls.Add(path1Input);
                form.Controls.Add(path2Label);
                form.Controls.Add(path2Input);
                form.Controls.Add(browse1);
                form.Controls.Add(browse2);
                form.Controls.Add(okButton);
                form.Controls.Add(cancelButton);

                // Show the dialog
                var result = form.ShowDialog(this);

                if (result == DialogResult.OK)
                {
                    string selectedOption = options.FirstOrDefault(x => x.Value.Checked).Key;

                    switch (selectedOption)
                    {
                        case "Pruning":
                            if (!string.IsNullOrEmpty(path1Input.Text) && !string.IsNullOrEmpty(path2Input.Text))
                            {
                                TestCases.Test_Pruning(path1Input.Text, path2Input.Text, (int)thresholdInput.Value);
                            }
                            else
                            {
                                MessageBox.Show("Please select both audio files for pruning test");
                            }
                            break;

                        case "Small":
                            if (!string.IsNullOrEmpty(path1Input.Text) && !string.IsNullOrEmpty(path2Input.Text))
                            {
                                TestCases.load_complete_tests(1, path1Input.Text, path2Input.Text, (int)thresholdInput.Value);
                            }
                            else
                            {
                                MessageBox.Show("Please select both audio files for pruning test");
                            }
                            break;

                        case "Medium":

                            if (!string.IsNullOrEmpty(path1Input.Text) && !string.IsNullOrEmpty(path2Input.Text))
                            {
                                TestCases.load_complete_tests(2, path1Input.Text, path2Input.Text, (int)thresholdInput.Value);
                            }
                            else
                            {
                                MessageBox.Show("Please select both audio files for pruning test");
                            }
                            break;


                        case "Large":
                            if (!string.IsNullOrEmpty(path1Input.Text) && !string.IsNullOrEmpty(path2Input.Text))
                            {
                                TestCases.load_complete_tests(3, path1Input.Text, path2Input.Text, (int)thresholdInput.Value);
                            }
                            else
                            {
                                MessageBox.Show("Please select both audio files for pruning test");
                            }
                            break;
                           
                    }

                    // Update the button text that triggered this event
                    if (callingButton != null)
                    {
                        callingButton.Text = $"Last Run: {selectedOption}";

                        callingButton.Text += $" (Width: {thresholdInput.Value})";
                    }
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}
