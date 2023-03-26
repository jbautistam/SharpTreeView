using System;
using System.IO;
using System.Windows;

namespace Bau.Controls.SharpTreeView.Demo.Nodes
{
	public class FileNode : FileSystemNode
	{
		private FileInfo _info;
		private string _name;

		public FileNode(string path)
		{
			_name = Path.GetFileName(path);
			_info = new FileInfo(path);
		}

		public override object Text
		{
			get
			{
				return _name;
			}
		}

		public override object Icon
		{
			get
			{
				return MainWindow.LoadIcon("File.png");
			}
		}

		public override object ToolTip
		{
			get
			{
				return _info.FullName;
			}
		}

		public override bool IsEditable
		{
			get
			{
				return true;
			}
		}

		public override string LoadEditText()
		{
			return _name;
		}

		public override bool SaveEditText(string value)
		{
			if (value.Contains("?")) {
				MessageBox.Show("?");
				return false;
			}
			else {
				_name = value;
				return true;
			}
		}

		public override long? FileSize
		{
			get { return _info.Length; }
		}

		public override DateTime? FileModified
		{
			get { return _info.LastWriteTime; }
		}

		public override string FullPath
		{
			get { return _info.FullName; }
		}

		public override bool CanPaste(IDataObject data)
		{
			return Parent.CanPaste(data);
		}
		
		public override void Paste(IDataObject data)
		{
			Parent.Paste(data);
		}
	}
}
