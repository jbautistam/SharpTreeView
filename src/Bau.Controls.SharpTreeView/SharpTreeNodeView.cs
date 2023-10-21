using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.ComponentModel;
using Bau.Controls.SharpTreeView.Extensors;
using Bau.Controls.SharpTreeView.Adorners;

namespace Bau.Controls.SharpTreeView
{
    public class SharpTreeNodeView : Control
	{
		// Propiedades de dependencia
		public static readonly DependencyProperty TextBackgroundProperty = DependencyProperty.Register(nameof(TextBackground), 
																									   typeof(Brush), typeof(SharpTreeNodeView));
		
		public static readonly DependencyProperty CellEditorProperty = DependencyProperty.Register(nameof(CellEditor), 
																								   typeof(Control), typeof(SharpTreeNodeView),
																								   new FrameworkPropertyMetadata());

		static SharpTreeNodeView()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(SharpTreeNodeView),
			                                         new FrameworkPropertyMetadata(typeof(SharpTreeNodeView)));
		}

		/// <summary>
		///		Aplica la plantilla
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			LinesRenderer = Template.FindName("linesRenderer", this) as LinesRenderer;
			UpdateTemplate();
		}

		/// <summary>
		///		Cambia los datos cuando se modifica el padre
		/// </summary>
		protected override void OnVisualParentChanged(DependencyObject oldParent)
		{
			base.OnVisualParentChanged(oldParent);
			ParentItem = this.FindAncestor<SharpTreeViewItem>();
			if (ParentItem is not null)
				ParentItem.NodeView = this;
		}

		/// <summary>
		///		Actualiza los nodos cuando se cambia el contexto
		/// </summary>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			if (e.Property == DataContextProperty)
				UpdateDataContext(e.OldValue as SharpTreeNode, e.NewValue as SharpTreeNode);
		}

		/// <summary>
		///		Modifica el contexto de los datos
		/// </summary>
		private void UpdateDataContext(SharpTreeNode? oldNode, SharpTreeNode? newNode)
		{
			if (newNode != null) 
			{
				newNode.PropertyChanged += Node_PropertyChanged;
				if (Template != null)
					UpdateTemplate();
			}
			if (oldNode != null)
				oldNode.PropertyChanged -= Node_PropertyChanged;
		}

		/// <summary>
		///		Responde al evento de propiedad modificada
		/// </summary>
		private void Node_PropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(e.PropertyName))
			{
				if (e.PropertyName.Equals(nameof(SharpTreeNode.IsEditing), StringComparison.CurrentCultureIgnoreCase))
					OnIsEditingChanged();
				else if (Node is not null)
				{
					if (e.PropertyName.Equals(nameof(SharpTreeNode.IsLast), StringComparison.CurrentCultureIgnoreCase)) 
					{
						if (ParentTreeView is not null && ParentTreeView.ShowLines) 
							foreach (SharpTreeNode child in Node.VisibleDescendantsAndSelf()) 
								if (ParentTreeView.ItemContainerGenerator.ContainerFromItem(child) is SharpTreeViewItem container && 
										container is not null)
									container.NodeView?.LinesRenderer?.InvalidateVisual();
					} 
					else if (e.PropertyName.Equals(nameof(SharpTreeNode.IsExpanded), StringComparison.CurrentCultureIgnoreCase) && 
							Node.IsExpanded)
						ParentTreeView?.HandleExpanding(Node);
				}
			}
		}

		/// <summary>
		///		Trata el cambio del modo de edición
		/// </summary>
		private void OnIsEditingChanged()
		{
			if (Template.FindName("textEditorContainer", this) is Border textEditorContainer)
			{
				if (Node?.IsEditing ?? false) 
				{
					if (CellEditor is null)
						textEditorContainer.Child = new Editors.EditTextBox() { Item = ParentItem };
					else
						textEditorContainer.Child = CellEditor;
				}
				else
					textEditorContainer.Child = null;
			}
		}

		/// <summary>
		///		Actualiza la plantilla
		/// </summary>
		private void UpdateTemplate()
		{
			if (Template.FindName("spacer", this) is FrameworkElement spacer)
			{
				// Calcula la indentación
				spacer.Width = CalculateIndent();
				// Muestra / oculta el icono de expandir / colapsar
				if (Template.FindName("expander", this) is ToggleButton expander)
				{
					if (ParentTreeView is not null && ParentTreeView.Root == Node && !ParentTreeView.ShowRootExpander) 
						expander.Visibility = Visibility.Collapsed;
					else 
						expander.ClearValue(VisibilityProperty);
				}
			}
		}

		/// <summary>
		///		Calcula la indentación
		/// </summary>
		internal int CalculateIndent()
		{
			int result = 19 * Node?.Level ?? 0;

				// Ajusta el resultado dependiendo del nodo raíz
				if (ParentTreeView is not null && ParentTreeView.ShowRoot) 
				{
					if (!ParentTreeView.ShowRootExpander && ParentTreeView.Root != Node) 
						result -= 15;
				}
				else 
					result -= 19;
				// El resultado no puede ser menor que cero
				result = Math.Max(result, 0);
				return result;
		}

		/// <summary>
		///		Elemento padre
		/// </summary>
		public SharpTreeViewItem? ParentItem { get; private set; }

		/// <summary>
		///		Arbol padre
		/// </summary>
		public SharpTreeView? ParentTreeView
		{
			get { return ParentItem?.ParentTreeView; }
		}

		/// <summary>
		///		Nodo
		/// </summary>
		public SharpTreeNode? Node
		{
			get { return DataContext as SharpTreeNode; }
		}

		/// <summary>
		///		Control para el dibujo de las líneas
		/// </summary>
		internal LinesRenderer? LinesRenderer { get; private set; }

		/// <summary>
		///		Fondo del texto
		/// </summary>
		public Brush TextBackground
		{
			get { return (Brush) GetValue(TextBackgroundProperty); }
			set { SetValue(TextBackgroundProperty, value); }
		}
		
		/// <summary>
		///		Editor de la celda
		/// </summary>
		public Control CellEditor 
		{
			get { return (Control) GetValue(CellEditorProperty); }
			set { SetValue(CellEditorProperty, value); }
		}
	}
}
