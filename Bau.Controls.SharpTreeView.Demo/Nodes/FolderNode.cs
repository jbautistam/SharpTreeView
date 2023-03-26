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
			try {
				foreach (var p in Directory.GetDirectories(_path)
				         .OrderBy(d => Path.GetDirectoryName(d))) {
					Children.Add(new FolderNode(p));
				}
				foreach (var p in Directory.GetFiles(_path)
				         .OrderBy(f => Path.GetFileName(f))) {
					Children.Add(new FileNode(p));
				}
			}
			catch {
			}
		}
		
		public override bool CanPaste(IDataObject data)
		{
			return data.GetDataPresent(DataFormats.FileDrop);
		}
		
		public override void Paste(IDataObject data)
		{
			var paths = data.GetData(DataFormats.FileDrop) as string[];
			if (paths != null) {
				foreach (var p in paths) {
					if (File.Exists(p)) {
						Children.Add(new FileNode(p));
					} else {
						Children.Add(new FolderNode(p));
					}
				}
			}
		}
		
		public override void Drop(DragEventArgs e, int index)
		{
			var paths = e.Data.GetData(DataFormats.FileDrop) as string[];
			if (paths != null) {
				foreach (var p in paths) {
					if (File.Exists(p)) {
						Children.Insert(index++, new FileNode(p));
					} else {
						Children.Insert(index++, new FolderNode(p));
					}
				}
			}
		}
	}
}
