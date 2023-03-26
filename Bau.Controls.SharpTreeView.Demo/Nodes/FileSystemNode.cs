using System;
using System.Linq;
using System.Windows;

namespace Bau.Controls.SharpTreeView.Demo.Nodes
{
	public abstract class FileSystemNode : SharpTreeNode
	{
		public abstract string FullPath { get; }

		public virtual long? FileSize
		{
			get { return null; }
		}

		public virtual DateTime? FileModified
		{
			get { return null; }
		}

		public override string ToString()
		{
			return FullPath;
		}

		public override bool CanCopy(SharpTreeNode[] nodes)
		{
			return nodes.All(n => n is FileSystemNode);
		}
		
		protected override IDataObject GetDataObject(SharpTreeNode[] nodes)
		{
			var data = new DataObject();
			var paths = nodes.OfType<FileSystemNode>().Select(n => n.FullPath).ToArray();
			data.SetData(DataFormats.FileDrop, paths);
			return data;
		}
		
		public override bool CanDelete(SharpTreeNode[] nodes)
		{
			return nodes.All(n => n is FileSystemNode);
		}
		
		public override void Delete(SharpTreeNode[] nodes)
		{
			if (MessageBox.Show("Are you sure you want to delete " + nodes.Length + " items?", "Delete", MessageBoxButton.OKCancel) == MessageBoxResult.OK) {
				DeleteWithoutConfirmation(nodes);
			}
		}
		
		public override void DeleteWithoutConfirmation(SharpTreeNode[] nodes)
		{
			foreach (var node in nodes) {
				if (node.Parent != null)
					node.Parent.Children.Remove(node);
			}
		}

		
//		ContextMenu menu;
//
//		public override ContextMenu GetContextMenu()
//		{
//			if (menu == null) {
//				menu = new ContextMenu();
//				menu.Items.Add(new MenuItem() { Command = ApplicationCommands.Cut });
//				menu.Items.Add(new MenuItem() { Command = ApplicationCommands.Copy });
//				menu.Items.Add(new MenuItem() { Command = ApplicationCommands.Paste });
//				menu.Items.Add(new Separator());
//				menu.Items.Add(new MenuItem() { Command = ApplicationCommands.Delete });
//			}
//			return menu;
//		}
	}
}
