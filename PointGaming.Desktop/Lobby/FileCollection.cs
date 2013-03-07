using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PointGaming.Desktop.Lobby
{
    public sealed class FileCollection : ObservableCollection<FileSystemInfo>
    {
        public FileCollection()
        {
            // put files in %windir%\web\wallpaper into the collection

            // get %windir%
            string windir = Environment.GetEnvironmentVariable("WINDIR");
            if (string.IsNullOrEmpty(windir))
                return;

            // put files into collection
            string wallpaperPath = string.Format(@"{0}\Web\Wallpaper", windir);
            DirectoryInfo info = new DirectoryInfo(wallpaperPath);
            FileSystemInfo[] files = info.GetFileSystemInfos();
            AddFiles(files);
        }

        private void AddFiles(FileSystemInfo[] files)
        {
            foreach (FileSystemInfo fi in files)
            {
                // exlude hidden files 
                if ((fi.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                {
                    if ((fi.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        AddFiles(new DirectoryInfo(fi.FullName).GetFileSystemInfos());
                        continue;
                    }
                    Add(fi);
                }
            }
        }
    }
}
