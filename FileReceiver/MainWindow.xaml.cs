using Ookii.Dialogs.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace FileReceiver
{
    public partial class MainWindow : Window
    {
        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double opacity = e.NewValue;
            this.Opacity = opacity;

            int percent = (int)(opacity * 100);
            if(OpacityValueText != null)
                OpacityValueText.Text = $"{percent}%";
        }
        private FileReceiverService _receiver;
        private ObservableCollection<ReceivedFile> _files = new ObservableCollection<ReceivedFile>();
        private void MainGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove(); // 拖曳整個視窗
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            FileListView.ItemsSource = _files;

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string savePath = SavePathBox.Text;
            _receiver = new FileReceiverService(9000, savePath);
            _receiver.FileReceived += OnFileReceived;
            _receiver.Start();

            StatusText.Text = $"正在監聽 Port 9000，儲存到：{savePath}";
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _receiver?.Stop();
        }

        private void OnFileReceived(object sender, ReceivedFile file)
        {
            Dispatcher.Invoke(() => _files.Insert(0, file));
        }

        private void ChooseFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog
            {
                Description = "請選擇儲存檔案的資料夾",
                UseDescriptionForTitle = true,
                SelectedPath = SavePathBox.Text
            };

            bool? result = dialog.ShowDialog(this); // this = owner window
            if (result == true)
            {
                SavePathBox.Text = dialog.SelectedPath;
                _receiver?.SetSavePath(dialog.SelectedPath);
                StatusText.Text = $"監聽中 (Port 9000)，儲存於：{dialog.SelectedPath}";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0); // 關閉應用程式
        }
    }
}