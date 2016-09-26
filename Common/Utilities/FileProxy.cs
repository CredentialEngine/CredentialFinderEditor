using System.IO;
using System.IO.Compression;
using System;

namespace Utilities
{
	class FileProxy : IFileProxy
	{
		//https://github.com/fsprojects/Paket/blob/master/src/Paket.Bootstrapper/HelperProxies/FileProxy.cs

		public bool Exists(string filename)
		{
			return File.Exists(filename);
		}

		public void Copy(string fileFrom, string fileTo, bool overwrite)
		{
			File.Copy(fileFrom, fileTo, overwrite);
		}

		public void Delete(string filename)
		{
			File.Delete(filename);
		}

		public Stream Create(string filename)
		{
			return File.Create(filename);
		}
		public string Read(string filename)
		{
			string data = ""; ;

			data = File.ReadAllText(filename);

			return data;
		}
		public bool Write(string filename, string data)
		{
			bool isOK = true;

			File.WriteAllText(filename, data);

			return isOK;
		}
		public string GetLocalFileVersion(string filename)
		{
			//return BootstrapperHelper.GetLocalFileVersion(filename);
			if (!File.Exists(filename)) return "";

			try
			{
				var bytes = File.ReadAllBytes(filename);
				//var attr = Assembly.Load(bytes).GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false).Cast<AssemblyInformationalVersionAttribute>().FirstOrDefault();
				//if (attr == null) return "";
				//return attr.InformationalVersion;
				return "Not Implemented";
			}
			catch (Exception) { return ""; }
		}

		public void FileMove(string fromFile, string toFile)
		{
			//BootstrapperHelper.FileMove(fromFile, toFile);
			try
            {
                if (File.Exists(toFile))
                {
                    File.Delete(toFile);
                }
            }
            catch (FileNotFoundException)
            {

            }

            File.Move(fromFile, toFile);
        
		}

		public void ExtractToDirectory(string zipFile, string targetLocation)
		{
			//ZipFile.ExtractToDirectory(zipFile, targetLocation);
		}

		public DateTime GetLastWriteTime(string filename)
		{
			var fileInfo = new FileInfo(filename);
			return fileInfo.LastWriteTime;
		}

		public void Touch(string filename)
		{
			var fileInfo = new FileInfo(filename);
			fileInfo.LastWriteTime = fileInfo.LastAccessTime = DateTime.Now;
		}
	}
}