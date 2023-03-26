using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;

namespace Bau.Controls.SharpTreeView
{
	/// <summary>
	///		Elemento del árbol
	/// </summary>
	public class SharpTreeViewItem : ListViewItem
	{
		// Variables privadas
		private Point _startPoint;
		private bool _wasSelected, _wasDoubleClick;

		static SharpTreeViewItem()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(SharpTreeViewItem), new FrameworkPropertyMetadata(typeof(SharpTreeViewItem)));
		}

		/// <summary>
		///		Sobrescribe el método KeyDown para las teclas F2 y Escape
		/// </summary>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (Node is not null)
				switch (e.Key) 
				{
					case Key.F2:
							if (Node.IsEditable && ParentTreeView?.SelectedItems.Count == 1 && ParentTreeView?.SelectedItems[0] == Node) 
							{
								Node.IsEditing = true;
								e.Handled = true;
							}
						break;
					case Key.Escape:
							if (Node.IsEditing) 
							{
								Node.IsEditing = false;
								e.Handled = true;
							}
						break;
				}
		}

		/// <summary>
		///		Trata el evento MouseLeftButton (comienzo del drag / selección)
		/// </summary>
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			// Guarda el valor que indica si estaba seleccionado
			_wasSelected = IsSelected;
			// Si no estaba seleccionado le pasa el evento
			if (!IsSelected) 
				base.OnMouseLeftButtonDown(e);
			// Si es el botón izquierdo, se prepara para un drag
			if (Mouse.LeftButton == MouseButtonState.Pressed) 
			{
				// Arranca el drag
				_startPoint = e.GetPosition(null);
				CaptureMouse();
				// Comprueba si se ha pulsado dos veces
				if (e.ClickCount == 2)
					_wasDoubleClick = true;
			}
		}

		/// <summary>
		///		Trata el evento MouseMove (drag)
		/// </summary>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (IsMouseCaptured && Node is not null && ParentTreeView is not null) 
			{
				Point currentPoint = e.GetPosition(null);

					if (Math.Abs(currentPoint.X - _startPoint.X) >= SystemParameters.MinimumHorizontalDragDistance ||
							Math.Abs(currentPoint.Y - _startPoint.Y) >= SystemParameters.MinimumVerticalDragDistance) 
						Node.StartDrag(this, ParentTreeView.GetTopLevelSelection().ToArray());
			}
		}

		/// <summary>
		///		Trata el evento MouseUp (sobre el botón izquierdo
		/// </summary>
		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			// Trata el doble click
			if (_wasDoubleClick) 
			{
				// Indica que ya no es un doble click
				_wasDoubleClick = false;
				// Cambia las propiedades del nodo
				if (Node is not null && ParentTreeView is not null)
				{
					// Activa el nodo
					Node.ActivateItem(e);
					// Cambia el valor que indica si está expandido / colapsado
					if (!e.Handled && (!Node.IsRoot || ParentTreeView.ShowRootExpander))
						Node.IsExpanded = !Node.IsExpanded;
				}
			}
			// Libera la captura del ratón
			ReleaseMouseCapture();
			// Si estaba seleccionado, se trata el evento base
			if (_wasSelected)
				base.OnMouseLeftButtonDown(e);
		}

		/// <summary>
		///		Pasa el evento de apertura del menú contextual al nodo
		/// </summary>
		protected override void OnContextMenuOpening(ContextMenuEventArgs e)
		{
			if (Node is not null)
				Node.ShowContextMenu(e);
		}

		/// <summary>
		///		Pasa el evento de inicio de drag a la vista padre
		/// </summary>
		protected override void OnDragEnter(DragEventArgs e)
		{
			ParentTreeView?.HandleDragEnter(this, e);
		}

		/// <summary>
		///		Pasa el evento de drag sobre el control a a la vista padre
		/// </summary>
		protected override void OnDragOver(DragEventArgs e)
		{
			ParentTreeView?.HandleDragOver(this, e);
		}

		/// <summary>
		///		Pasa el evento de drop a la vista padre
		/// </summary>
		protected override void OnDrop(DragEventArgs e)
		{
			ParentTreeView?.HandleDrop(this, e);
		}

		/// <summary>
		///		Pasa el evento de fin de drag a la vista padre
		/// </summary>
		protected override void OnDragLeave(DragEventArgs e)
		{
			ParentTreeView?.HandleDragLeave(e);
		}

		/// <summary>
		///		Vista padre del árbol
		/// </summary>
		public SharpTreeView? ParentTreeView { get; internal set; }

		/// <summary>
		///		Vista del nodo
		/// </summary>
		public SharpTreeNodeView? NodeView { get; internal set; }

		/// <summary>
		///		Datos del nodo
		/// </summary>
		public SharpTreeNode? Node
		{
			get 
			{ 
				if (DataContext is SharpTreeNode node)
					return node;
				else
					return null; 
			}
		}
	}
}
