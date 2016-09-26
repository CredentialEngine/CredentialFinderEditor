using System;
using System.IO;

namespace Utilities
{
	//https://github.com/fsprojects/Paket/blob/master/src/Paket.Bootstrapper/HelperProxies/IFileProxy.cs
	public interface IFileProxy
	{
		bool Exists(string filename);
		void Copy(string fileFrom, string fileTo, bool overwrite = false);
		void Delete(string filename);
		Stream Create(string tmpFile);
		string Read(string filename);
		bool Write(string filename, string data);
		string GetLocalFileVersion(string filename);
		void FileMove(string fromFile, string toFile);
		void ExtractToDirectory(string zipFile, string targetLocation);
		DateTime GetLastWriteTime(string filename);
		void Touch(string filename);
	}
}