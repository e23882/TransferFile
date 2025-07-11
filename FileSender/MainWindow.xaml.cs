using System.IO;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Input;

namespace FileSender
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Window_Drop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null || files.Length == 0) return;

            string ip = IpTextBox.Text.Trim();
            int port = 9000; // 固定埠，接收端也需相同

            foreach (var filePath in files)
            {
                if (!File.Exists(filePath)) continue;

                try
                {
                    using (TcpClient client = new TcpClient())
                    {
                        await client.ConnectAsync(ip, port);
                        using (NetworkStream stream = client.GetStream())
                        using (BinaryWriter writer = new BinaryWriter(stream))
                        {
                            string fileName = System.IO.Path.GetFileName(filePath);
                            byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(fileName);
                            byte[] fileBytes = File.ReadAllBytes(filePath);

                            // 傳送檔名長度和檔名
                            writer.Write(nameBytes.Length);
                            writer.Write(nameBytes);

                            // 傳送檔案大小與內容
                            writer.Write(fileBytes.Length);
                            writer.Write(fileBytes);
                        }
                    }

                    MessageBox.Show($"成功傳送檔案：{System.IO.Path.GetFileName(filePath)}", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"傳送失敗：{ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double opacity = e.NewValue;
            this.Opacity = opacity;

            int percent = (int)(opacity * 100);
            if (OpacityValueText != null)
                OpacityValueText.Text = $"{percent}%";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove(); // 拖曳整個視窗
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}