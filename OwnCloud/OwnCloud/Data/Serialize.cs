using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml.Serialization;

namespace OwnCloud
{
    class Serialize
    {
        /// <summary>
        /// Serializes an object to isolatedStorage
        /// Path will automaticly created.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="any"></param>
        static public void WriteFile(string path, object any)
        {
            try
            {
                IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
                if (Path.GetDirectoryName(path) != String.Empty && !isf.DirectoryExists(Path.GetDirectoryName(path)))
                {
                    isf.CreateDirectory(Path.GetDirectoryName(path));
                }
                IsolatedStorageFileStream stream = new IsolatedStorageFileStream(path, FileMode.Create, isf);
                XmlSerializer serializer = new XmlSerializer(any.GetType());
                serializer.Serialize(stream, any);
                MemoryStream mem = new MemoryStream();
                serializer.Serialize(mem, any);

                stream.Close();
            }
            catch (Exception ex)
            {
                Utility.Debug(ex.Message);
            }
        }

        /// <summary>
        /// Reads an object from isolatedStorage
        /// </summary>
        /// <param name="path"></param>
        /// <param name="objType"></param>
        static public object ReadFile(string path, Type objType)
        {
            try
            {
                IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
                if (!isf.FileExists(path)) return null;
                IsolatedStorageFileStream stream = new IsolatedStorageFileStream(path, FileMode.Open, isf);
                XmlSerializer serialize = new XmlSerializer(objType);
                object o = serialize.Deserialize(stream);
                stream.Close();
                return o;
            }
            catch (Exception ex)
            {
                Utility.Debug(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Tries to remove a file
        /// </summary>
        /// <param name="path"></param>
        static public bool RemoveFile(string path)
        {
            try
            {
                IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
                if (!isf.FileExists(path)) return false;
                isf.DeleteFile(path);
                return true;
            }
            catch (Exception ex)
            {
                Utility.Debug(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Lists all files of given path in isolatedStorage
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static public string[] ListFiles(string path)
        {
            try
            {
                IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
                string directory = Path.GetDirectoryName(path) == String.Empty ? path : Path.GetDirectoryName(path);

                if (isf.DirectoryExists(directory))
                {
                    return isf.GetFileNames(directory + @"/*");
                }
                return new string[0];
            }
            catch (Exception ex)
            {
                Utility.Debug(ex.Message);
                return new string[0];
            }            
        }
    }
}
