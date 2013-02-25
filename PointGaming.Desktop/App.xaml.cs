using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;

namespace PointGaming.Desktop
{
    public partial class App : Application
    {
        public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss zzz";

        public static bool IsShuttingDown = false;

        public static readonly string SettingsPath =
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar
            + PointGaming.Desktop.Properties.Settings.Default.UserFolder;

        static App()
        {
            try
            {
                var di = new DirectoryInfo(SettingsPath);
                if (!di.Exists)
                    di.Create();
            }
            catch (Exception e)
            {
                MessageBox.Show("Could not create user settings folder!  Details: " + e.Message);
            }
        }

        public static void LogLine(string message)
        {
            Console.WriteLine(DateTime.Now.ToString(DateTimeFormat) + " " + message);
        }
    }

    static class UIExtensionMethods
    {
        public static void BeginInvokeUI(this Control c, Action a)
        {
            try
            {
                c.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)delegate()
                {
                    if (App.IsShuttingDown) return;
                    a();
                });
            }
            catch { }
        }
        public static void InvokeUI(this Control c, Action a)
        {
            if (Thread.CurrentThread.ManagedThreadId == HomeWindow.GuiThreadId)
            {
                a();
            }
            else
            {
                AutoResetEvent areSelect = new AutoResetEvent(false);
                try
                {
                    c.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)delegate()
                    {
                        try
                        {
                            if (App.IsShuttingDown) return;
                            a();
                        }
                        finally
                        {
                            areSelect.Set();
                        }
                    });
                }
                catch
                {
                    areSelect.Set();
                }
                areSelect.WaitOne();
            }
        }




        public static bool TryGetRowItem<T>(this DataGrid d, MouseButtonEventArgs e, out T rowItem)
        {
            var element = d.InputHitTest(e.GetPosition(d));
            return TryGetX(element, out rowItem);
        }

        private static bool TryGetX<T>(IInputElement element, out T friend)
        {
            friend = default(T);

            DataGridRow row;
            if (!TryGetRow((DependencyObject)element, out row))
                return false;

            friend = (T)row.Item;
            return true;
        }

        private static bool TryGetRow(DependencyObject element, out DataGridRow row)
        {
            row = null;
            while (element != null)
            {
                if (element is DataGridRow)
                {
                    row = (DataGridRow)element;
                    return true;
                }
                element = VisualTreeHelper.GetParent(element);
            }
            return false;
        }
        
        public static T FindDescendant<T>(this DependencyObject obj) where T : DependencyObject
        {
            if (obj == null) return default(T);
            int numberChildren = VisualTreeHelper.GetChildrenCount(obj);
            if (numberChildren == 0) return default(T);

            for (int i = 0; i < numberChildren; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T)
                {
                    return (T)(object)child;
                }
            }

            for (int i = 0; i < numberChildren; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                var potentialMatch = FindDescendant<T>(child);
                if (potentialMatch != default(T))
                {
                    return potentialMatch;
                }
            }

            return default(T);
        }
    }
}
