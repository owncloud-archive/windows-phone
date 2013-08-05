using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Net;
using OwnCloud.Extensions;

namespace OwnCloud.Data
{
    public class File : Entity
    {
        private string _name = "";
        /// <summary>
        /// Current Name of file
        /// </summary>
        public string FileName
        {
            get 
            {
                return Uri.UnescapeDataString(_name);
            }
            set
            {
                if (value == String.Empty)
                {
                    throw new Exception("Model_File_Filename_Empty".Translate());
                }
                else
                {
                    _name = value;
                }
            }
        }

        /// <summary>
        /// The relative resource location
        /// </summary>
        private string _path = "";
        public string FilePath
        {
            get { return _path; }
            set
            {
                _path = value;
            }
        }

        private long _size = 0;
        /// <summary>
        /// Size of file
        /// </summary>
        public long FileSize
        {
            get { return _size; }
            set { _size = value; }
        }

        /// <summary>
        /// Gets the filesize in readable format by adding KB, MB etc
        /// </summary>
        public string Size
        {
            get
            {
                return Utility.FormatBytes(_size);
            }
        }

        private string _type = "";
        /// <summary>
        /// Mime type of file
        /// </summary>
        public string FileType
        {
            get { return _type; }
            set { _type = value.ToString(); }
        }

        /// <summary>
        /// Gets the file type
        /// </summary>
        public string Type
        {
            get
            {
                return MimeTypes.GetNameOf(_type.Split(';')[0]);
            }
        }

        bool _synced = false;
        /// <summary>
        /// Tells if the file is in sync state
        /// </summary>
        public bool IsSynced
        {
            get
            {
                return _synced;
            }
        }

        /// <summary>
        /// Tells or sets if the file should be synced
        /// </summary>
        public bool Sync
        {
            get;
            set;
        }

        /// <summary>
        /// Tells or sets if the file should be treated as directory
        /// </summary>
        public bool IsDirectory
        {
            get;
            set;
        }

        DateTime _mtime;
        /// <summary>
        /// Gets or sets the last modification time
        /// </summary>
        public DateTime FileLastModified
        {
            get
            {
                return _mtime == null? new DateTime() : _mtime;
            }
            set
            {
                _mtime = value;
            }
        }

        DateTime _ctime;
        /// <summary>
        /// Gets or sets the creation time
        /// </summary>
        public DateTime FileCreated
        {
            get
            {
                return _ctime == null? new DateTime() : _ctime;
            }
            set
            {
                _ctime = value;
            }
        }

        /// <summary>
        /// Gets the last modification time of the file in readable format
        /// </summary>
        public string LastModified
        {
            get
            {
                return "✎ " + FileLastModified.ToString("");
            }
        }

        /// <summary>
        /// Gets the creation time of the file in readable format
        /// </summary>
        public string Created
        {
            get
            {
                return "✎ " + FileCreated.ToString("");
            }
        }
    }
}