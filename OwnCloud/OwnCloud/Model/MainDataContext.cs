using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;


namespace OwnCloud.Model
{
    public class MainDataContext : INotifyPropertyChanged
    {
        public MainDataContext()
        {
            this.Files = new ObservableCollection<File>();
        }

        /// <summary>
        /// Example File List
        /// </summary>
        public ObservableCollection<File> Files { get; private set; }


        public bool IsDataLoaded
        {
            get;
            private set;
        }

        private long _storageLocalFree = 0;
        /// <summary>
        /// Gets the available storage on local device in bytes
        /// </summary>
        public long LocalStorageFreeBytes
        {
            get
            {
                return _storageLocalFree;
            }
            set
            {
                _storageLocalFree = value;
                NotifyPropertyChanged("AvailableLocalStorage");
            }
        }

        public string AvailableLocalStorage
        {
            get
            {
                return Utility.FormatBytes(_storageLocalFree);
            }
        }

        public void LoadData()
        {
            // Example data
            this.Files.Add(new File() { Filename = "Longfilename does not fit here.txt", Filesize = 234, Filetype = "text/plain", FileLastModified= DateTime.Parse("2013-07-05T22:05:21+00:00") });
            this.Files.Add(new File() { Filename = "Something.html", Filesize = 12940, Filetype = "text/html;charset=utf-8", FileLastModified = DateTime.Parse("2013-02-05T23:16:21+00:00") });

            this.IsDataLoaded = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}