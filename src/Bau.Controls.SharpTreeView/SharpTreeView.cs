using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using Bau.Controls.SharpTreeView.Adorners;
using Bau.Controls.SharpTreeView.Models;

namespace Bau.Controls.SharpTreeView
{
    /// <summary>
    ///		Control que muestra un árbol con columnas adicionales
    /// </summary>
    public class SharpTreeView : ListView
	{
		/// <summary>
		///		Lugar donde se coloca el elemento soltado
		/// </summary>
		public enum DropPlace
		{
			/// <summary>Antes</summary>
			Before, 
			/// <summary>En el interior</summary>
			Inside, 
			/// <summary>Después</summary>
			After
		}
		// Propiedades de dependencia
		public static readonly DependencyProperty ShowLinesProperty = DependencyProperty.Register(nameof(ShowLines), typeof(bool), typeof(SharpTreeView),
																								  new FrameworkPropertyMetadata(true));
		public static readonly DependencyProperty AllowDropOrderProperty = DependencyProperty.Register(nameof(AllowDropOrder), typeof(bool), 
																									   typeof(SharpTreeView));
		public static readonly DependencyProperty RootProperty = DependencyProperty.Register(nameof(Root), typeof(SharpTreeNode), typeof(SharpTreeView));
		public static readonly DependencyProperty ShowRootProperty = DependencyProperty.Register(nameof(ShowRoot), typeof(bool), typeof(SharpTreeView),
																								 new FrameworkPropertyMetadata(true));
		public static readonly DependencyProperty ShowRootExpanderProperty = DependencyProperty.Register(nameof(ShowRootExpander), typeof(bool), 
																										 typeof(SharpTreeView),
																										 new FrameworkPropertyMetadata(false));
		public static readonly DependencyProperty ShowAlternationProperty = DependencyProperty.RegisterAttached(nameof(ShowAlternation), 
																												typeof(bool), typeof(SharpTreeView),
																												new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
		// Variables privadas
		private TreeFlattenerCollection? _flattener;
		private bool _doNotScrollOnExpanding;
		private SharpTreeNodeView? _previewNodeView;
		private InsertMarker? _insertMarker;

		static SharpTreeView()
		{
			// Asigna las propiedades
			DefaultStyleKeyProperty.OverrideMetadata(typeof(SharpTreeView), new FrameworkPropertyMetadata(typeof(SharpTreeView)));
			SelectionModeProperty.OverrideMetadata(typeof(SharpTreeView), new FrameworkPropertyMetadata(SelectionMode.Extended));
			AlternationCountProperty.OverrideMetadata(typeof(SharpTreeView), new FrameworkPropertyMetadata(2));
			DefaultItemContainerStyleKey = new ComponentResourceKey(typeof(SharpTreeView), nameof(DefaultItemContainerStyleKey));
			VirtualizingStackPanel.VirtualizationModeProperty.OverrideMetadata(typeof(SharpTreeView), new FrameworkPropertyMetadata(VirtualizationMode.Recycling));
			// Registra los comandos
			CommandManager.RegisterClassCommandBinding(typeof(SharpTreeView),
			                                           new CommandBinding(ApplicationCommands.Cut, HandleExecuted_Cut, HandleCanExecute_Cut));
			CommandManager.RegisterClassCommandBinding(typeof(SharpTreeView),
			                                           new CommandBinding(ApplicationCommands.Copy, HandleExecuted_Copy, HandleCanExecute_Copy));
			CommandManager.RegisterClassCommandBinding(typeof(SharpTreeView),
			                                           new CommandBinding(ApplicationCommands.Paste, HandleExecuted_Paste, HandleCanExecute_Paste));
			CommandManager.RegisterClassCommandBinding(typeof(SharpTreeView),
			                                           new CommandBinding(ApplicationCommands.Delete, HandleExecuted_Delete, HandleCanExecute_Delete));
		}

		public SharpTreeView()
		{
			SetResourceReference(ItemContainerStyleProperty, DefaultItemContainerStyleKey);
		}

		public static bool GetShowAlternation(DependencyObject obj)
		{
			return (bool) obj.GetValue(ShowAlternationProperty);
		}

		public static void SetShowAlternation(DependencyObject obj, bool value)
		{
			obj.SetValue(ShowAlternationProperty, value);
		}
		
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			if (e.Property == RootProperty || e.Property == ShowRootProperty || e.Property == ShowRootExpanderProperty)
				Reload();
		}

		public void Reload()
		{
			if (_flattener != null)
				_flattener.Stop();
			if (Root != null) 
			{
				if (!(ShowRoot && ShowRootExpander)) 
					Root.IsExpanded = true;
				_flattener = new TreeFlattenerCollection(Root, ShowRoot);
				_flattener.CollectionChanged += flattener_CollectionChanged;
				ItemsSource = _flattener;
			}
		}

		/// <summary>
		///		Cuando se cambia la colección, se deseleccionan los nodos que estaban ocultos si permanecen en el árbol
		/// </summary>
		private void flattener_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			// Deselect nodes that are being hidden, if any remain in the tree
			if (e.Action == NotifyCollectionChangedAction.Remove && Items.Count > 0 && e.OldItems is not null) 
			{
				List<SharpTreeNode>? selectedOldItems = null;

					// Guarda los elementos seleccionados al principio
					foreach (SharpTreeNode node in e.OldItems) 
					{
						if (node.IsSelected) 
						{
							if (selectedOldItems == null)
								selectedOldItems = new List<SharpTreeNode>();
							selectedOldItems.Add(node);
						}
					}
					// Obtiene los elementos del árbol menos los sellecionados
					if (selectedOldItems is not null) 
					{
						List<SharpTreeNode> list = SelectedItems.Cast<SharpTreeNode>().Except(selectedOldItems).ToList();

							// Selecciona los elementos de la lista
							SetSelectedItems(list);
							// Si hemos eliminado todos los nodos seleccionads, cambiamos el foco al nodo precedente al
							// primer elemento de los nodos seleccionados anteriormente
						if (SelectedItem is null && IsKeyboardFocusWithin) 
						{
							SelectedIndex = Math.Max(0, e.OldStartingIndex - 1);
							if (SelectedIndex >= 0 && SelectedItem is SharpTreeNode node)
								FocusNode(node);
						}
					}
			}
		}
		
		protected override DependencyObject GetContainerForItemOverride() => new SharpTreeViewItem();

		protected override bool IsItemItsOwnContainerOverride(object item) => item is SharpTreeViewItem;

		protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
			// Llama al método base
			base.PrepareContainerForItemOverride(element, item);
			// Prepara el contenedor del elemento
			if (element is SharpTreeViewItem container)
			{
				// Cambia el parent del contenedor
				container.ParentTreeView = this;
				// Se asegura que LineRenderer toma en cuenta los nuevos datos vinculados
				container.NodeView?.LinesRenderer?.InvalidateVisual();
			}
		}
		
		/// <summary>
		/// Handles the node expanding event in the tree view.
		/// This method gets called only if the node is in the visible region (a SharpTreeNodeView exists).
		/// </summary>
		internal void HandleExpanding(SharpTreeNode node)
		{
			if (_doNotScrollOnExpanding)
				return;
			SharpTreeNode lastVisibleChild = node;
			while (true) 
			{
				SharpTreeNode? tmp = lastVisibleChild.Children.LastOrDefault(c => c.IsVisible);

					if (tmp is not null) 
						lastVisibleChild = tmp;
					else 
						break;
			}
			if (lastVisibleChild != node) 
			{
				// Make the the expanded children are visible; but don't scroll down
				// to much (keep node itself visible)
				base.ScrollIntoView(lastVisibleChild);
				// For some reason, this only works properly when delaying it...
				Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () => base.ScrollIntoView(node));
			}
		}
		
		/// <summary>
		///		Trata los eventos de teclado
		/// </summary>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.OriginalSource is SharpTreeViewItem container)
				switch (e.Key) 
				{
					case Key.Left:
							if (ItemsControlFromItemContainer(container) == this) 
							{
								if (container.Node is not null)
								{
									if (container.Node.IsExpanded) 
										container.Node.IsExpanded = false;
									else if (container.Node.Parent != null) 
										FocusNode(container.Node.Parent);
								}
								e.Handled = true;
							}
						break;
					case Key.Right:
							if (ItemsControlFromItemContainer(container) == this) 
							{
								if (container.Node is not null)
								{
									if (!container.Node.IsExpanded && container.Node.ShowExpander) 
										container.Node.IsExpanded = true;
									else if (container.Node.Children.Count > 0) // jump to first child:
										container.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
								}
								e.Handled = true;
							}
						break;
					case Key.Return:
							if (Keyboard.Modifiers == ModifierKeys.None && SelectedItems.Count == 1 && SelectedItem == container.Node) 
							{
								e.Handled = true;
								container.Node.ActivateItem(e);
							}
						break;
					case Key.Space:
							if (Keyboard.Modifiers == ModifierKeys.None && SelectedItems.Count == 1 && SelectedItem == container.Node) 
							{
								e.Handled = true;
								if (container.Node is not null)
								{
									if (container.Node.IsCheckable) 
									{
										if (container.Node.IsChecked == null) // If partially selected, we want to select everything
											container.Node.IsChecked = true;
										else
											container.Node.IsChecked = !container.Node.IsChecked;
									} 
									else 
										container.Node.ActivateItem(e);
								}
							}
						break;
					case Key.Add:
							if (ItemsControlFromItemContainer(container) == this) 
							{
								if (container.Node is not null)
									container.Node.IsExpanded = true;
								e.Handled = true;
							}
						break;
					case Key.Subtract:
							if (ItemsControlFromItemContainer(container) == this) 
							{
								if (container.Node is not null)
									container.Node.IsExpanded = false;
								e.Handled = true;
							}
						break;
					case Key.Multiply:
							if (ItemsControlFromItemContainer(container) == this) 
							{
								if (container.Node is not null)
								{
									container.Node.IsExpanded = true;
									ExpandRecursively(container.Node);
								}
								e.Handled = true;
							}
						break;
				}
			// Si no se ha tratado el evento, se llama al base
			if (!e.Handled)
				base.OnKeyDown(e);
		}
		
		/// <summary>
		///		Expande un nodo recursivamente
		/// </summary>
		public void ExpandRecursively(SharpTreeNode node)
		{
			if (node.CanExpandRecursively) 
			{
				// Marca el nodo como expandido
				node.IsExpanded = true;
				// Expande los nodos hijo
				foreach (SharpTreeNode child in node.Children)
					ExpandRecursively(child);
			}
		}
		
		/// <summary>
		///		Coloca el nodo especificado en la vista y le asigna el foco del teclado
		/// </summary>
		public void FocusNode(SharpTreeNode node)
		{
			if (node is null)
				throw new ArgumentNullException("node");
			else
			{
				// Scroll a la vista
				ScrollIntoView(node);
				// El método ScrollIntoView() de WPF utiliza la misma construcción if / dispatcher, así que llamamos a OnFocusItem() después que
				// el elemento ha entrado en la vista
				if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
					OnFocusItem(node);
				else
					Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new DispatcherOperationCallback(OnFocusItem), node);
			}
		}
		
		/// <summary>
		///		Scroll del nodo a la vista
		/// </summary>
		public void ScrollIntoView(SharpTreeNode node)
		{
			if (node is null)
				throw new ArgumentNullException("node");
			else
			{
				// Indica que está realizando el scroll al expandir el nodo
				_doNotScrollOnExpanding = true;
				// Expande los nodos padre
				foreach (SharpTreeNode ancestor in node.Ancestors())
					ancestor.IsExpanded = true;
				// Indica que ha finalizado el scroll al expandir el nodo
				_doNotScrollOnExpanding = false;
				// Llama al método base
				base.ScrollIntoView(node);
			}
		}
		
		/// <summary>
		///		Cambia el foco sobre un elemento
		/// </summary>
		private object? OnFocusItem(object item)
		{
			// Cambia el foco sobre el elemento
			if (ItemContainerGenerator.ContainerFromItem(item) is FrameworkElement element)
				element.Focus();
			// Devuelve un nulo
			return null;
		}
				
		/// <summary>
		///		Sobrescribe el evento de selección
		/// </summary>
		protected override void OnSelectionChanged(SelectionChangedEventArgs e)
		{
			// Deselecciona los elementos eliminados
			foreach (SharpTreeNode node in e.RemovedItems)
				node.IsSelected = false;
			// Selecciona los elementos añadidos
			foreach (SharpTreeNode node in e.AddedItems)
				node.IsSelected = true;
			// Llama al evento base
			base.OnSelectionChanged(e);
		}

		/// <summary>
		///		Sobrescribe el evento de entrada de un elemento drag
		/// </summary>
		protected override void OnDragEnter(DragEventArgs e)
		{
			OnDragOver(e);
		}

		/// <summary>
		///		Sobrescribe el evento "sobre" de un elemento drag
		/// </summary>
		protected override void OnDragOver(DragEventArgs e)
		{
			// Cambia los efectos
			e.Effects = DragDropEffects.None;
			// Cambia el efecto cuando está sobre la raíz
			if (Root is not null && !ShowRoot) 
			{
				e.Handled = true;
				e.Effects = Root.GetDropEffect(e, Root.Children.Count);
			}
		}

		/// <summary>
		///		Sobrescribe el evento de drop
		/// </summary>
		protected override void OnDrop(DragEventArgs e)
		{
			// Cambia los efectos
			e.Effects = DragDropEffects.None;
			// Trata el drop sobre el elemento raíz
			if (Root is not null && !ShowRoot) 
			{
				// Indica que se ha tratado y los efectos
				e.Handled = true;
				e.Effects = Root.GetDropEffect(e, Root.Children.Count);
				// Ejecuta el drop sobre la raíz
				if (e.Effects != DragDropEffects.None)
					Root.InternalDrop(e, Root.Children.Count);
			}
		}
		
		/// <summary>
		///		Trata el evento drag sobre el control
		/// </summary>
		internal void HandleDragEnter(SharpTreeViewItem item, DragEventArgs e)
		{
			HandleDragOver(item, e);
		}
		
		/// <summary>
		///		Trata el evento drag over sobre el control
		/// </summary>
		internal void HandleDragOver(SharpTreeViewItem item, DragEventArgs e)
		{
			DropTarget? target = GetDropTarget(item, e);

				// Oculta la preview
				HidePreview();
				// Quita los efectos al cursor
				e.Effects = DragDropEffects.None;
				// Muestra el over sobre el objetivo si existe
				if (target is not null) 
				{
					e.Handled = true;
					e.Effects = target.Effect;
					ShowPreview(target.Item, target.Place);
				}
		}
		
		/// <summary>
		///		Trata el evento drop sobre el control
		/// </summary>
		internal void HandleDrop(SharpTreeViewItem item, DragEventArgs e)
		{
			DropTarget? target = GetDropTarget(item, e);

				// Oculta los datos previos
				HidePreview();
				// Trata el drop sobre el objetivo si existe
				if (target is not null) 
				{
					e.Handled = true;
					e.Effects = target.Effect;
					target.Node?.InternalDrop(e, target.Index);
				}
		}
		
		/// <summary>
		///		Trata el evento de abandonar el drag sobre el control
		/// </summary>
		internal void HandleDragLeave(DragEventArgs e)
		{
			HidePreview();
			e.Handled = true;
		}

		internal class DropTarget
		{
			public SharpTreeViewItem? Item;
			public DropPlace Place;
			public double Y;
			public SharpTreeNode? Node;
			public int Index;
			public DragDropEffects Effect;
		}

		internal DropTarget? GetDropTarget(SharpTreeViewItem item, DragEventArgs e)
		{
			List<DropTarget> dropTargets = BuildDropTargets(item, e);
			double y = e.GetPosition(item).Y;

				// Obtiene el objetivo que esté por encima de la posición y del cursor
				foreach (DropTarget target in dropTargets) 
					if (target.Y >= y) 
						return target;
				// Si ha llegado hasta aquí es porque no ha encontrado nada
				return null;
		}

		private List<DropTarget> BuildDropTargets(SharpTreeViewItem item, DragEventArgs e)
		{
			List<DropTarget> result = new();
			SharpTreeNode? node = item.Node;

			if (AllowDropOrder)
				TryAddDropTarget(result, item, DropPlace.Before, e);

			TryAddDropTarget(result, item, DropPlace.Inside, e);

			if (AllowDropOrder) 
			{
				if (node is not null && node.IsExpanded && node.Children.Count > 0) 
				{
					if (ItemContainerGenerator.ContainerFromItem(node.Children[0]) is SharpTreeViewItem firstChildItem)
						TryAddDropTarget(result, firstChildItem, DropPlace.Before, e);
				}
				else 
					TryAddDropTarget(result, item, DropPlace.After, e);
			}

			var h = item.ActualHeight;
			var y1 = 0.2 * h;
			var y2 = h / 2;
			var y3 = h - y1;

			if (result.Count == 2) 
			{
				if (result[0].Place == DropPlace.Inside && result[1].Place != DropPlace.Inside) 
					result[0].Y = y3;
				else if (result[0].Place != DropPlace.Inside && result[1].Place == DropPlace.Inside) 
					result[0].Y = y1;
				else 
					result[0].Y = y2;
			}
			else if (result.Count == 3) 
			{
				result[0].Y = y1;
				result[1].Y = y3;
			}
			if (result.Count > 0) 
				result[result.Count - 1].Y = h;
			return result;
		}

		/// <summary>
		///		Trata de conseguir el control nodo objetivo del drop
		/// </summary>
		private void TryAddDropTarget(List<DropTarget> targets, SharpTreeViewItem item, DropPlace place, DragEventArgs e)
		{
			(SharpTreeNode? node, int index) = GetNodeAndIndex(item, place);

				// Si se ha encontrado un nodo
				if (node is not null) 
				{
					DragDropEffects effect = node.GetDropEffect(e, index);

						// Añade el objetivo si es necesario
						if (effect != DragDropEffects.None) 
							targets.Add(new DropTarget() 
														{
															Item = item,
															Place = place,
															Node = node,
															Index = index,
															Effect = effect
														}
										);
				}
		}

		private (SharpTreeNode? node, int index) GetNodeAndIndex(SharpTreeViewItem item, DropPlace place)
		{
			SharpTreeNode? node = null;
			int index = 0;

				// Obtiene el nodo e índice
				if (place == DropPlace.Inside) 
				{
					node = item.Node;
					if (node is not null)
						index = node.Children.Count;
				}
				else if (place == DropPlace.Before) 
				{
					if (item.Node?.Parent is not null) 
					{
						node = item.Node.Parent;
						index = node.Children.IndexOf(item.Node);
					}
				}
				else if (item.Node?.Parent is not null) 
				{
					node = item.Node.Parent;
					index = node.Children.IndexOf(item.Node) + 1;
				}
				// Devuelve el resultado
				return (node, index);
		}

		private void ShowPreview(SharpTreeViewItem? item, DropPlace place)
		{
			//DropPlace _previewPlace = place;

			_previewNodeView = item?.NodeView;

			if (place == DropPlace.Inside) 
			{
				_previewNodeView.TextBackground = SystemColors.HighlightBrush;
				_previewNodeView.Foreground = SystemColors.HighlightTextBrush;
			}
			else 
			{
				// Crea el marcador de inserción si es necesario
				if (_insertMarker is null) 
				{
					AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
					GeneralAdorner adorner = new GeneralAdorner(this);

						// Crea el marcador
						_insertMarker = new InsertMarker();
						// Lo asigna al adorner
						adorner.Child = _insertMarker;
						adornerLayer.Add(adorner);
				}
				// Muestra el marcador
				_insertMarker.Visibility = Visibility.Visible;

				Point p1 = _previewNodeView.TransformToVisual(this).Transform(new Point());
				Point p = new Point(p1.X + _previewNodeView.CalculateIndent() + 4.5, p1.Y - 3);

				if (place == DropPlace.After)
					p.Y += _previewNodeView.ActualHeight;

				_insertMarker.Margin = new Thickness(p.X, p.Y, 0, 0);
				
				SharpTreeNodeView? secondNodeView = null;
				int index = _flattener.IndexOf(item?.Node);

				if (place == DropPlace.Before) 
				{
					if (index > 0) 
						secondNodeView = (ItemContainerGenerator.ContainerFromIndex(index - 1) as SharpTreeViewItem)?.NodeView;
				}
				else if (index + 1 < _flattener.Count) 
					secondNodeView = (ItemContainerGenerator.ContainerFromIndex(index + 1) as SharpTreeViewItem)?.NodeView;
				
				double width = p1.X + _previewNodeView.ActualWidth - p.X;

				if (secondNodeView != null) 
				{
					Point p2 = secondNodeView.TransformToVisual(this).Transform(new Point());
					width = Math.Max(width, p2.X + secondNodeView.ActualWidth - p.X);
				}

				_insertMarker.Width = width + 10;
			}
		}

		/// <summary>
		///		Oculta la previsualización
		/// </summary>
		private void HidePreview()
		{
			if (_previewNodeView is not null) 
			{
				_previewNodeView.ClearValue(SharpTreeNodeView.TextBackgroundProperty);
				_previewNodeView.ClearValue(SharpTreeNodeView.ForegroundProperty);
				if (_insertMarker is not null)
					_insertMarker.Visibility = Visibility.Collapsed;
				_previewNodeView = null;
			}
		}

		/// <summary>
		///		Corta los nodos
		/// </summary>
		static void HandleExecuted_Cut(object sender, ExecutedRoutedEventArgs e)
		{
			if (sender is SharpTreeView treeView)
			{
				SharpTreeNode[] nodes = treeView.GetTopLevelSelection().ToArray();

					// Corta los nodos
					if (nodes.Length > 0)
						nodes[0].Cut(nodes);
					// Indica que se ha tratado
					e.Handled = true;
			}
		}

		/// <summary>
		///		Comprueba si se pueden cortar los nodos
		/// </summary>
		static void HandleCanExecute_Cut(object sender, CanExecuteRoutedEventArgs e)
		{
			if (sender is SharpTreeView treeView)
			{
				SharpTreeNode[] nodes = treeView.GetTopLevelSelection().ToArray();

					// Comprueba si se puede ejecutar
					e.CanExecute = nodes.Length > 0 && nodes[0].CanCut(nodes);
					// Indica que se ha tratado el evento
					e.Handled = true;
			}
		}

		/// <summary>
		///		Copia los datos de los nodos
		/// </summary>
		static void HandleExecuted_Copy(object sender, ExecutedRoutedEventArgs e)
		{
			if (sender is SharpTreeView treeView)
			{
				SharpTreeNode[] nodes = treeView.GetTopLevelSelection().ToArray();

					// Copia los nodos
					if (nodes.Length > 0)
						nodes[0].Copy(nodes);
					// Indica que se ha tratado
					e.Handled = true;
			}
		}

		/// <summary>
		///		Comprueba si se pueden copiar los nodos
		/// </summary>
		static void HandleCanExecute_Copy(object sender, CanExecuteRoutedEventArgs e)
		{
			if (sender is SharpTreeView treeView)
			{
				SharpTreeNode[] nodes = treeView.GetTopLevelSelection().ToArray();

					// Comprueba si se pueden copiar
					e.CanExecute = nodes.Length > 0 && nodes[0].CanCopy(nodes);
					// Indica que se ha tratado
					e.Handled = true;
			}
		}

		/// <summary>
		///		Pega los datos
		/// </summary>
		static void HandleExecuted_Paste(object sender, ExecutedRoutedEventArgs e)
		{
			if (sender is SharpTreeView treeView)
			{
				IDataObject data = Clipboard.GetDataObject();

					// Si han venido datos del portapapeles
					if (data is not null) 
					{
						SharpTreeNode selectedNode = (treeView.SelectedItem as SharpTreeNode) ?? treeView.Root;

							// Pega los datos
							if (selectedNode is not null)
								selectedNode.Paste(data);
					}
					// Indica que se ha tratado el evento
					e.Handled = true;
			}
		}

		/// <summary>
		///		Comprueba si se pueden pegar datos
		/// </summary>
		static void HandleCanExecute_Paste(object sender, CanExecuteRoutedEventArgs e)
		{
			if (sender is SharpTreeView treeView)
			{
				IDataObject data = Clipboard.GetDataObject();

					if (data is null) 
						e.CanExecute = false;
					else 
					{
						SharpTreeNode selectedNode = (treeView.SelectedItem as SharpTreeNode) ?? treeView.Root;

							// Indica si se puede ejecutar
							e.CanExecute = selectedNode != null && selectedNode.CanPaste(data);
					}
					e.Handled = true;
			 }
		}
		
		/// <summary>
		///		Ejecuta el borrado de nodos
		/// </summary>
		static void HandleExecuted_Delete(object sender, ExecutedRoutedEventArgs e)
		{
			if (sender is SharpTreeView treeView)
			{
				SharpTreeNode[] nodes = treeView.GetTopLevelSelection().ToArray();

					// Borra los nodos
					if (nodes.Length > 0)
						nodes[0].Delete(nodes);
					// Indica que se ha tratado
					e.Handled = true;
			}
		}

		/// <summary>
		///		Borra los nodos
		/// </summary>
		static void HandleCanExecute_Delete(object sender, CanExecuteRoutedEventArgs e)
		{
			if (sender is SharpTreeView treeView)
			{
				SharpTreeNode[] nodes = treeView.GetTopLevelSelection().ToArray();

					// Indica si se puede ejecutar y que se ha tratado el evento
					e.CanExecute = nodes.Length > 0 && nodes[0].CanDelete(nodes);
					e.Handled = true;
			}
		}
		
		/// <summary>
		///		Obtiene los elementos seleccionados que no tienen seleccionado ningún otro elemento padre
		/// </summary>
		public IEnumerable<SharpTreeNode> GetTopLevelSelection()
		{
			IEnumerable<SharpTreeNode> selectedItems = SelectedItems.OfType<SharpTreeNode>();
			HashSet<SharpTreeNode> selectionHash = new HashSet<SharpTreeNode>(selectedItems);

				// Devuelve los elementos
				return selectedItems.Where(item => item.Ancestors().All(a => !selectionHash.Contains(a)));
		}

		/// <summary>
		///		Clave del estilo predeterminado sobre el contenedos
		/// </summary>
		public static ResourceKey DefaultItemContainerStyleKey { get; private set; }

		/// <summary>
		///		Nodo raíz
		/// </summary>
		public SharpTreeNode Root
		{
			get { return (SharpTreeNode) GetValue(RootProperty); }
			set { SetValue(RootProperty, value); }
		}

		/// <summary>
		///		Indica si se debe mostrar el nodo raíz
		/// </summary>
		public bool ShowRoot
		{
			get { return (bool) GetValue(ShowRootProperty); }
			set { SetValue(ShowRootProperty, value); }
		}

		/// <summary>
		///		Indica si debe mostrar el control de expansión sobre la raíz
		/// </summary>
		public bool ShowRootExpander
		{
			get { return (bool) GetValue(ShowRootExpanderProperty); }
			set { SetValue(ShowRootExpanderProperty, value); }
		}

		/// <summary>
		///		Indica si se permite un orden en el drop
		/// </summary>
		public bool AllowDropOrder
		{
			get { return (bool) GetValue(AllowDropOrderProperty); }
			set { SetValue(AllowDropOrderProperty, value); }
		}

		/// <summary>
		///		Indica si se deben mostrar las líneas
		/// </summary>
		public bool ShowLines
		{
			get { return (bool) GetValue(ShowLinesProperty); }
			set { SetValue(ShowLinesProperty, value); }
		}

		/// <summary>
		///		Activa / desactiva la alternancia de colores
		/// </summary>
		public bool ShowAlternation
		{
			get { return (bool) GetValue(ShowAlternationProperty); }
			set { SetValue(ShowAlternationProperty, value); }
		}
	}
}
