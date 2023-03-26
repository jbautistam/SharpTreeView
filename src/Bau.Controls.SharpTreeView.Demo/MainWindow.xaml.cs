using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Bau.Controls.SharpTreeView.Demo
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			treeView1.Root = new Nodes.FolderNode("c:\\");
			treeView1.ShowRoot = false;
			//treeView1.SelectionChanged += new SelectionChangedEventHandler(treeView1_SelectionChanged);

			treeView2.Root = new Nodes.FolderNode("c:\\");
			treeView2.ShowRootExpander = true;
			//treeView2.ShowRoot = false;

		}

		public static Image LoadIcon(string name)
		{
			var frame = BitmapFrame.Create(new Uri("pack://application:,,,/Images/" + name, UriKind.Absolute));
			Image result = new Image();
			result.Source = frame;
			return result;
		}
	}
}
