using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using OwnCloud.Data;

namespace OwnCloud.View.Controls
{
    public partial class FileMultiTileViewControl : UserControl
    {
        private bool _enabled = false;
        private Account _info;
        private File _file;

        /// <summary>
        /// Try to display contents of the file.
        /// The action can fail depending on the file type and size, then the default icon view will be generated.
        /// Must be created an refreshed linear (no async!) as it would raise I/O-Exceptions.
        /// 
        /// </summary>
        /// <param name="info">Associated Account info</param>
        /// <param name="file">Desired file</param>
        /// <param name="preview">If the current environment does not allow previews (remote view, no WLAN,) set this to false</param>
        public FileMultiTileViewControl(Account info, File file, bool preview)
        {
            InitializeComponent();
            _enabled = preview;
            _info = info;
            _file = file;
            this.FileName.Text = file.FileName;
            RefreshPreview(file);
        }

        /// <summary>
        /// Holds the File-Properties only available if DataContext was set properly.
        /// </summary>
        public File FileProperties
        {
            get
            {
                return _file;
            }
        }

        /// <summary>
        /// Refresh preview (if needed). Dependends on current environment and changed ETag.
        /// The preview is valid for remote and local view.
        /// First call will always lead to preview.
        /// 
        /// /Files/LocalPreview/Server/Path-To-File/file.preview.ext
        /// /Files/LocalPreview/Server/Path-To-File/file.etag
        /// </summary>
        /// <param name="file"></param>
        public void RefreshPreview(File file)
        {
            _file = file;
            bool createEtag = false;
            string path = "Files/LocalPreview/" + _info.Hostname + "/" + _file.FilePath + ".etag";
            var isf = App.DataContext.Storage;
            {
                if (isf.FileExists(path))
                {
                    var reader = new System.IO.StreamReader(isf.OpenFile(path, System.IO.FileMode.Open));
                    if (reader.ReadToEnd() == _file.ETag)
                    {
                        reader.Close();
                        return;
                    }
                    reader.Close();
                }
                else
                {
                    createEtag = true;
                }
            }

            // create main type
            if (_enabled)
            {
                switch (_file.FileType.Split('/').First())
                {
                    case "text":
                        TextPreview();
                        break;
                    case "image":
                        ImagePreview();
                        break;
                    case "application":
                        // try PDF content fetch
                        TextContentPreview();
                        break;
                    default:
                        PreviewIcon(file.IsDirectory);
                        break;
                }
            }
            else
            {
                PreviewIcon();
                createEtag = false;
            }

            if (createEtag)
            {
                    var storagePath = System.IO.Path.GetDirectoryName(path);
                    if (!isf.DirectoryExists(storagePath)) isf.CreateDirectory(storagePath);
                    var stream = isf.CreateFile(path);
                    System.IO.StreamWriter writer = new System.IO.StreamWriter(stream);
                    writer.Write(_file.ETag);
                    writer.Close();
            }
        }

        /// <summary>
        /// Try to display the image. Always create a tiny preview image in /Files/Thumbnail.
        /// </summary>
        protected void ImagePreview()
        {
            this.Background = new SolidColorBrush() { Color = Colors.Magenta };
            this.ImagePreviewGrid.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// Try fetch readable text from file or fallback to icon preview.
        /// </summary>
        protected void TextContentPreview()
        {
            this.Background = new SolidColorBrush() { Color = Colors.Cyan };
        }

        /// <summary>
        /// Activate the text preview.
        /// </summary>
        protected void TextPreview()
        {
            this.Background = new SolidColorBrush() { Color = Colors.Red };
            this.TextPreviewGrid.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// Activate the icon preview.
        /// </summary>
        protected void PreviewIcon(bool isDirectory = false)
        {
            this.IconPreviewGrid.Visibility = System.Windows.Visibility.Visible;
            if (isDirectory)
            {
                this.Background = new SolidColorBrush() { Color = Colors.Gray };
                this.Icon.Source = new BitmapImage(_file.ImageIcon);
            }
            else
            {
                this.Background = new SolidColorBrush() { Color = Colors.Blue };
            }
        }
    }
}
