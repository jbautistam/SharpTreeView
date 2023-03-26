using System;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;

namespace Bau.Controls.SharpTreeView
{
	/// <summary>
	///		Adorner general
	/// </summary>
	public class GeneralAdorner : Adorner
	{
		// Variables privadas
		private FrameworkElement _child = default!;

		public GeneralAdorner(UIElement target) : base(target) {}

		/// <summary>
		///		Obiene el número de elementos hijo
		/// </summary>
		protected override int VisualChildrenCount
		{
			get { return _child is null ? 0 : 1; }
		}

		/// <summary>
		///		Obtiene el control hijo
		/// </summary>
		protected override Visual GetVisualChild(int index)
		{
			return _child;
		}

		/// <summary>
		///		Mide el control
		/// </summary>
		protected override Size MeasureOverride(Size constraint)
		{
			// Si tiene algún elemento hijo, mide el tamaño deseado del elemento hijo
			if (_child != null) 
			{
				_child.Measure(constraint);
				return _child.DesiredSize;
			}
			// Si no tiene ningún elemento hijo, devuelve el tamaño vacío
			return new Size();
		}

		/// <summary>
		///		Sobrescribe el tamaño de los controles
		/// </summary>
		protected override Size ArrangeOverride(Size finalSize)
		{
			// Si tiene algún elemento hijo, calcula el tamaño del elemento hijo
			if (_child is not null)
			{
				_child.Arrange(new Rect(finalSize));
				return finalSize;
			}
			// Si no tiene ningún elemento hijo, devuelve el tamaño vacío
			return new Size();
		}

		/// <summary>
		///		Elemento hijo
		/// </summary>
		public FrameworkElement Child
		{
			get { return _child; }
			set
			{
				if (_child != value) 
				{
					// Elimina los controles visuales anteriores
					RemoveVisualChild(_child);
					RemoveLogicalChild(_child);
					// Asigna el control
					_child = value;
					// Añade los controles visuales
					AddLogicalChild(value);
					AddVisualChild(value);
					InvalidateMeasure();
				}
			}
		}
	}
}
