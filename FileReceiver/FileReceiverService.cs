using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileReceiver
{
    public class FileReceiverService
    {
        private int _port;
        private string _savePath;
        private TcpListener _listener;
        private CancellationTokenSource _cts;

        public event EventHandler<ReceivedFile> FileReceived;

        public FileReceiverService(int port, string savePath)
        {
            _port = port;
            _savePath = savePath;

            if (!Directory.Exists(_savePath))
                Directory.CreateDirectory(_savePath);
        }

        private readonly object _pathLock = new object();
        public void SetSavePath(string newPath)
        {
            lock (_pathLock)
            {
                _savePath = newPath;
                if (!Directory.Exists(_savePath))
                    Directory.CreateDirectory(_savePath);
            }
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();

            Task.Run(() => AcceptLoop(_cts.Token));
        }

        public void Stop()
        {
            _cts?.Cancel();
            _listener?.Stop();
        }

        private async Task AcceptLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync(token);
                    _ = Task.Run(() => HandleClient(client), token);
                }
                catch
                {
                    // Listener stopped or error occurred
                }
            }
        }

        private void HandleClient(TcpClient client)
        {
            using (client)
            using (var stream = client.GetStream())
            using (var reader = new BinaryReader(stream))
            {
                try
                {
                    int nameLen = reader.ReadInt32();
                    string fileName = Encoding.UTF8.GetString(reader.ReadBytes(nameLen));

                    int fileSize = reader.ReadInt32();
                    byte[] fileData = reader.ReadBytes(fileSize);

                    string fullPath = Path.Combine(_savePath, fileName);
                    File.WriteAllBytes(fullPath, fileData);

                    var fileInfo = new FileInfo(fullPath);
                    FileReceived?.Invoke(this, new ReceivedFile
                    {
                        Time = DateTime.Now.ToString("HH:mm:ss"),
                        FileName = fileName,
                        SizeKB = (fileInfo.Length / 1024.0).ToString("F1")
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[接收錯誤] {ex.Message}");
                }
            }
        }
    }
}