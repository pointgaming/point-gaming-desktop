using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PointGaming.Desktop.HomeTab
{
    /// <summary>
    /// Interaction logic for DebugTab.xaml
    /// </summary>
    public partial class DebugTab : UserControl
    {
        public DebugTab()
        {
            InitializeComponent();
        }

        private bool _once;
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            if (!_once)
            {
                _once = true;
                StartConsole();
            }
        }

        private void StartConsole()
        {
            var t = new Thread(ConsoleThread)
            {
                Name = "MainWindow.Console",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
            t.Start();
        }

        private void ConsoleThread()
        {
            FileStream fs = null;
            StreamWriter fw = null;
            try
            {
                var ms = new SynchronizedMemoryStream();
                var sw = new StreamWriter(ms);

                Console.SetOut(sw);

                bool triedToMakeDebugFile = false;

                while (true)
                {
                    if (!triedToMakeDebugFile)
                    {
                        triedToMakeDebugFile = true;
                        if (fs == null)
                        {
                            try
                            {
                                string path = App.SettingsPath + "\\debug.log";
                                fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                                fs.Seek(0, SeekOrigin.End);
                                fw = new StreamWriter(fs, Encoding.UTF8);
                            }
                            catch { }
                        }
                    }

                    Thread.Sleep(250);
                    string text;

                    lock (ms.WriteSynch)
                    {
                        sw.Flush();
                        long end = ms.Position;
                        ms.Position = 0;

                        text = Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)end);
                    }

                    if (text.Length > 0 && fw != null)
                    {
                        fw.Write(text);
                        fw.Flush();
                    }

                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action<string>(delegate(string s)
                    {
                        textBoxConsole.AppendText(s);
                        if (!textBoxConsole.IsKeyboardFocused)
                            textBoxConsole.ScrollToEnd();
                    }), text);
                }
            }
            finally
            {
                if (fs != null) fs.Dispose();
            }
        }


        class SynchronizedMemoryStream : MemoryStream
        {
            public readonly object WriteSynch = new object();
            public override void Write(byte[] buffer, int offset, int count)
            {
                lock (WriteSynch)
                {
                    base.Write(buffer, offset, count);
                }
            }
            public override void WriteByte(byte value)
            {
                lock (WriteSynch)
                {
                    base.WriteByte(value);
                }
            }
        }
    }
}
