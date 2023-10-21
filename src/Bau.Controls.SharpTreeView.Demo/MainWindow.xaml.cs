using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Bau.Controls.SharpTreeView.Demo
{
	/// <summary>
	///		Ventana de ejemplo
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			treeView1.Root = new Nodes.FolderNode("c:\\");
			treeView1.ShowRoot = false;

			treeView2.Root = new Nodes.FolderNode("c:\\");
			treeView2.ShowRootExpander = true;
		}

		public static Image LoadIcon(string name)
		{

		// pack://application:,,,/ChessDataBase.Plugin;component/Resources/ChessBoard/
			// var frame = BitmapFrame.Create(new Uri("/Resources/Images/" + name, UriKind.Relative));
			var frame = BitmapFrame.Create(new Uri("pack://application:,,,/Images/" + name, UriKind.Absolute));
			Image result = new Image();
			result.Source = frame;
			return result;
		}
	}
}
