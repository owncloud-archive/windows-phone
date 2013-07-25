using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using OwnCloud.Extensions;

namespace OwnCloud.Data
{
    public class File : Entity
    {
        private string _name;
        /// <summary>
        /// Current Name of file
        /// </summary>
        public string Filename
        {
            get { return _name; }
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

        private long _size = 0;
        /// <summary>
        /// Size of file
        /// </summary>
        public long Filesize
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

        private string _type;
        /// <summary>
        /// Mime type of file
        /// </summary>
        public string Filetype
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
        bool Sync
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
                return _mtime;
            }
            set
            {
                _mtime = value;
            }
        }

        /// <summary>
        /// Gets the last modification time of the file in readable format
        /// </summary>
        public string LastModified
        {
            get
            {
                return "✎ " + _mtime.ToString("");
            }
        }
    }
}