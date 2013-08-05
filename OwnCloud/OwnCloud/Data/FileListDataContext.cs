using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using OwnCloud.Data;

namespace OwnCloud.Data
{
    public class FileListDataContext : Entity
    {
        public FileListDataContext()
        {
            Files = new ObservableCollection<File>();
        }

        public ObservableCollection<File> Files
        {
            get;
            set;
        }
    }
}
