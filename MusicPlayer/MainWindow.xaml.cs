using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MusicPlayer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MediaPlayer mediaPlayer;
        
        public MainWindow()
        {
            InitializeComponent();
        }
        
        #region Открытие файла
        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(OpenFile));
            thread.IsBackground = true;
            thread.Start();
        }
        private void OpenFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                this.Dispatcher.Invoke((Action)delegate
                {
                    mediaPlayer = new MediaPlayer();
                    mediaPlayer.Open(new Uri(openFileDialog.FileName));
                    musicNameLabel.Content = Path.GetFileName(openFileDialog.FileName);

                    DispatcherTimer timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(1);
                    timer.Tick += Timer_Tick;
                    timer.Start();

                    mediaPlayer.Play();
                });
            }
        }
        #endregion

        #region Действия при Play, Pause, Stop
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer != null)
                mediaPlayer.Play();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer.Source != null)
            {
                timeLabel.Content = String.Format("{0} / {1}",
                    mediaPlayer.Position.ToString(@"mm\:ss"), mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
                progressSlider.Minimum = 0;
                progressSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                progressSlider.Value = mediaPlayer.Position.TotalSeconds;
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer != null)
                mediaPlayer.Pause();
        }
        
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer != null)
                mediaPlayer.Stop();
        }
        #endregion

        #region Движение слайдера
        private void ProgressSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            mediaPlayer.Position = TimeSpan.FromSeconds(progressSlider.Value);
        }
        #endregion

        #region Сохранение текста при закрытии приложения
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(SaveNotes));
            thread.Start();
        }
        public void SaveNotes()
        {
            using (var writer = new StreamWriter("file.txt", false, Encoding.Default))
            {
                this.Dispatcher.Invoke((Action)delegate
                {
                    writer.WriteLine(notesTextBox.Text);
                });
            }
        }
        #endregion
    }
}