﻿using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using Spider.Service;

namespace Spider
{
    public partial class Form1 : Form
    {
        private bool _selectAllDoneStartUrl;
        private bool _selectAllDoneSearchText;
        private bool _selectAllDoneNThreads;
        private bool _selectAllDoneNUrls;

        private readonly SearchEngine _spider;
        private readonly TaskFactory factory = new TaskFactory();

        public Form1()
        {
            InitializeComponent();
            INotifier notifier = new ConfigurableNotifier((notification) =>
                {
                    progressTextBox.BeginInvoke((MethodInvoker)(() => progressTextBox.AppendText(notification)));
                },
                (completedMessage) =>
                {
                    resultsTextBox.BeginInvoke((MethodInvoker)(() => resultsTextBox.AppendText(completedMessage)));
                },
                (percentage) =>
                {
                    progressBarBackgroundWorker.ReportProgress(percentage);
                });
            _spider = new SearchEngine(notifier);
        }

        private void startButton_click(object sender, EventArgs e)
        {
            progressTextBox.Text = "";
            completedLabel.Text = "";
            resultsTextBox.Text = "";

            pauseButton.Enabled = true;
            stopButton.Enabled = true;

            int nThreads;
            if (!int.TryParse(nThreadsTextBox.Text, out nThreads) || nThreads < 1)
            {
                using (DialogBox dialogBox = new DialogBox("Number of threads should be positive integer"))
                {
                    dialogBox.ShowDialog();
                    nThreadsTextBox.Text = "";
                    nThreadsTextBox.Focus();
                    return;
                }
            }

            int nUrls;
            if (!int.TryParse(nUrlsTextBox.Text, out nUrls) || nUrls < 1)
            {
                using (DialogBox dialogBox = new DialogBox("Number of urls should be positive integer"))
                {
                    dialogBox.ShowDialog();
                    nUrlsTextBox.Text = "";
                    nUrlsTextBox.Focus();
                    return;
                }
            }

            startButton.Enabled = false;
            while (progressBarBackgroundWorker.CancellationPending || progressBarBackgroundWorker.IsBusy) { }
            progressBarBackgroundWorker.RunWorkerAsync();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var url = startUrlTextBox.Text;
            url = url.StartsWith("http://") || url.StartsWith("https://")
                ? url
                : "http://" + url;
            var mainTask = factory.StartNew(() =>
                _spider.BrowseNet(url,
                    searchTextTextBox.Text,
                    Convert.ToInt32(nThreadsTextBox.Text),
                    Convert.ToInt32(nUrlsTextBox.Text)))
                    .ContinueWith((t) =>
                    {
                        completedLabel.BeginInvoke((MethodInvoker) (() => completedLabel.Text = "COMPLETED"));
                        progressBarBackgroundWorker.ReportProgress(100);
                        startButton.BeginInvoke((MethodInvoker)(() => startButton.Enabled = true));
                        pauseButton.BeginInvoke((MethodInvoker)(() => pauseButton.Enabled = false));
                        pauseButton.BeginInvoke((MethodInvoker)(() => pauseButton.Enabled = false));
                    });

            mainTask.Wait();
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void progressBarBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            startButton.Enabled = true;
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            _spider.CancelPreviousBrowsing();
            stopButton.Enabled = false;
            pauseButton.Enabled = false;
            resumeButton.Visible = false;
        }

        private void resumeButton_Click(object sender, EventArgs e)
        {
            _spider.Resume();
            resumeButton.Visible = false;
            pauseButton.Enabled = true;
        }

        private void pauseButton_Click(object sender, EventArgs e)
        {
            _spider.Pause();
            resumeButton.Visible = true;
            pauseButton.Enabled = false;
        }

        private void startUrlTextBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (!_selectAllDoneStartUrl && startUrlTextBox.SelectionLength == 0)
            {
                _selectAllDoneStartUrl = true;
                startUrlTextBox.SelectAll();
            }
        }

        private void startUrlTextBox_Enter(object sender, EventArgs e)
        {
            if (MouseButtons == MouseButtons.None)
            {
                startUrlTextBox.SelectAll();
                _selectAllDoneStartUrl = true;
            }
        }

        private void startUrlTextBox_Leave(object sender, EventArgs e)
        {
            _selectAllDoneStartUrl = false;
        }

        private void searchTextTextBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (!_selectAllDoneSearchText && searchTextTextBox.SelectionLength == 0)
            {
                _selectAllDoneSearchText = true;
                searchTextTextBox.SelectAll();
            }
        }

        private void searchTextTextBox_Enter(object sender, EventArgs e)
        {
            if (MouseButtons == MouseButtons.None)
            {
                searchTextTextBox.SelectAll();
                _selectAllDoneSearchText = true;
            }
        }

        private void searchTextTextBox_Leave(object sender, EventArgs e)
        {
            _selectAllDoneSearchText = false;
        }

        private void nThreadsTextBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (!_selectAllDoneNThreads && nThreadsTextBox.SelectionLength == 0)
            {
                _selectAllDoneNThreads = true;
                nThreadsTextBox.SelectAll();
            }
        }

        private void nThreadsTextBox_Enter(object sender, EventArgs e)
        {
            if (MouseButtons == MouseButtons.None)
            {
                nThreadsTextBox.SelectAll();
                _selectAllDoneNThreads = true;
            }
        }

        private void nThreadsTextBox_Leave(object sender, EventArgs e)
        {
            _selectAllDoneNThreads = false;
        }

        private void nUrlsTextBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (!_selectAllDoneNUrls && nUrlsTextBox.SelectionLength == 0)
            {
                _selectAllDoneNUrls = true;
                nUrlsTextBox.SelectAll();
            }
        }

        private void nUrlsTextBox_Enter(object sender, EventArgs e)
        {
            if (MouseButtons == MouseButtons.None)
            {
                nUrlsTextBox.SelectAll();
                _selectAllDoneNUrls = true;
            }
        }

        private void nUrlsTextBox_Leave(object sender, EventArgs e)
        {
            _selectAllDoneNUrls = false;
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
