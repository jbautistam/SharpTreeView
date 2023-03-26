using System;
using System.Windows;
using System.Windows.Media;

namespace Bau.Controls.SharpTreeView
{
	/// <summary>
	///		Clase para el dibujo de líneas entre nodos
	/// </summary>
	internal class LinesRenderer : FrameworkElement
	{
		// Variables privadas
		private static Pen _pen;

		static LinesRenderer()
		{
			_pen = new Pen(Brushes.LightGray, 1);
			_pen.Freeze();
		}

		/// <summary>
		///		Dibuja la línea
		/// </summary>
		protected override void OnRender(DrawingContext dc)
		{
			if (NodeView is not null)
			{
				int indent = NodeView.CalculateIndent();
				Point point = new Point(indent + 4.5, 0);

					if (NodeView.Node is not null)
					{
						// Dibuja la línea en el medio
						if (!NodeView.Node.IsRoot || (NodeView.ParentTreeView?.ShowRootExpander ?? false))
							dc.DrawLine(_pen, new Point(point.X, ActualHeight / 2), new Point(point.X + 10, ActualHeight / 2));
						// Dibuja la línea al nodo anterior / posterior
						if (!NodeView.Node.IsRoot)
						{
							SharpTreeNode current = NodeView.Node;

								// Si es el último, dibuja la línea en el medio, si no dibuja la anterior
								if (NodeView.Node.IsLast)
									dc.DrawLine(_pen, point, new Point(point.X, ActualHeight / 2));
								else
									dc.DrawLine(_pen, point, new Point(point.X, ActualHeight));
								// Dibuja el resto de líneas
								while (point.X > 0 && current is not null) 
								{
									point.X -= 19;
									current = current.Parent;
									if (current is not null && !current.IsLast && point.X >= 0)
										dc.DrawLine(_pen, point, new Point(point.X, ActualHeight));
								}
						}
					}
			}
		}

		/// <summary>
		///		Obtiene la vista del nodo
		/// </summary>
		public SharpTreeNodeView? NodeView
		{
			get { return TemplatedParent as SharpTreeNodeView; }
		}
	}
}
