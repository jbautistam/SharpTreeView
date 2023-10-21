using System;
using System.Linq;
using System.IO;
using System.Windows;

namespace Bau.Controls.SharpTreeView.Demo.Nodes
{
	public class FolderNode : FileSystemNode
	{
		// Variables privadas
		private string _path;

		public FolderNode(string path)
		{
			_path = path;
			LazyLoading = true;
		}


		public override object Text
		{
			get
			{
				var name = Path.GetFileName(_path);
				if (name == "") return _path;
				return name;
			}
		}

		public override object Icon
		{
			get
			{
				return MainWindow.LoadIcon("Folder.png");
			}
		}

		public override object ExpandedIcon
		{
			get
			{
				return MainWindow.LoadIcon("FolderOpened.png");
			}
		}

		public override bool IsCheckable
		{
			get
			{
				return true;
			}
		}

		public override string FullPath
		{
			get { return _path; }
		}

		protected override void LoadChildren()
		{
			try
			{
				foreach (string path in Directory.GetDirectories(_path).OrderBy(Path.GetDirectoryName)) 
					Children.Add(new FolderNode(path));
				foreach (string file in Directory.GetFiles(_path).OrderBy(Path.GetFileName)) 
					Children.Add(new FileNode(file));
			}
			catch 
			{
			}
		}
		
		public override bool CanPaste(IDataObject data)
		{
			return data.GetDataPresent(DataFormats.FileDrop);
		}
		
		public override void Paste(IDataObject data)
		{
			if (data.GetData(DataFormats.FileDrop) is string[] paths)
				foreach (string path in paths) 
					if (File.Exists(path)) 
						Children.Add(new FileNode(path));
					else 
						Children.Add(new FolderNode(path));
		}
		
		public override void Drop(DragEventArgs e, int index)
		{
			if (e.Data.GetData(DataFormats.FileDrop) is string[] paths)
				foreach (string path in paths) 
					if (File.Exists(path)) 
						Children.Insert(index++, new FileNode(path));
					else 
						Children.Insert(index++, new FolderNode(path));
		}
	}
}
