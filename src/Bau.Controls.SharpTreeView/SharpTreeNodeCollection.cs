using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Collections.Specialized;

namespace Bau.Controls.SharpTreeView
{
	/// <summary>
	/// Collection that validates that inserted nodes do not have another parent.
	/// </summary>
	public sealed class SharpTreeNodeCollection : IList<SharpTreeNode>, INotifyCollectionChanged
	{
		public event NotifyCollectionChangedEventHandler? CollectionChanged;
		bool isRaisingEvent;
		
		public SharpTreeNodeCollection(SharpTreeNode parent)
		{
			Parent = parent;
		}
		
		void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			Debug.Assert(!isRaisingEvent);
			isRaisingEvent = true;
			try {
				Parent.OnChildrenChanged(e);
				if (CollectionChanged != null)
					CollectionChanged(this, e);
			} finally {
				isRaisingEvent = false;
			}
		}
		
		private void ThrowOnReentrancy()
		{
			if (isRaisingEvent)
				throw new InvalidOperationException();
		}
		
		private void ThrowIfValueIsNullOrHasParent(SharpTreeNode node)
		{
			if (node == null)
				throw new ArgumentNullException("node");
			if (node.modelParent != null)
				throw new ArgumentException("The node already has a parent", "node");
		}
		
		public SharpTreeNode this[int index] {
			get {
				return Nodes[index];
			}
			set {
				ThrowOnReentrancy();
				var oldItem = Nodes[index];
				if (oldItem == value)
					return;
				ThrowIfValueIsNullOrHasParent(value);
				Nodes[index] = value;
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem, index));
			}
		}
		
		public int IndexOf(SharpTreeNode node)
		{
			if (node == null || node.modelParent != Parent)
				return -1;
			else
				return Nodes.IndexOf(node);
		}
		
		public void Insert(int index, SharpTreeNode node)
		{
			ThrowOnReentrancy();
			ThrowIfValueIsNullOrHasParent(node);
			Nodes.Insert(index, node);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, node, index));
		}
		
		public void InsertRange(int index, IEnumerable<SharpTreeNode> nodes)
		{
			if (nodes == null)
				throw new ArgumentNullException("nodes");
			ThrowOnReentrancy();
			List<SharpTreeNode> newNodes = nodes.ToList();
			if (newNodes.Count == 0)
				return;
			foreach (SharpTreeNode node in newNodes) {
				ThrowIfValueIsNullOrHasParent(node);
			}
			Nodes.InsertRange(index, newNodes);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newNodes, index));
		}
		
		public void RemoveAt(int index)
		{
			ThrowOnReentrancy();
			var oldItem = Nodes[index];
			Nodes.RemoveAt(index);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, index));
		}
		
		public void RemoveRange(int index, int count)
		{
			ThrowOnReentrancy();
			if (count == 0)
				return;
			var oldItems = Nodes.GetRange(index, count);
			Nodes.RemoveRange(index, count);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems, index));
		}
		
		public void Add(SharpTreeNode node)
		{
			ThrowOnReentrancy();
			ThrowIfValueIsNullOrHasParent(node);
			Nodes.Add(node);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, node, Nodes.Count - 1));
		}
		
		public void AddRange(IEnumerable<SharpTreeNode> nodes)
		{
			InsertRange(Count, nodes);
		}
		
		public void Clear()
		{
			ThrowOnReentrancy();
			var oldList = Nodes;
			Nodes.Clear();
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldList, 0));
		}
		
		public bool Contains(SharpTreeNode node)
		{
			return IndexOf(node) >= 0;
		}
		
		public void CopyTo(SharpTreeNode[] array, int arrayIndex)
		{
			Nodes.CopyTo(array, arrayIndex);
		}
		
		public bool Remove(SharpTreeNode item)
		{
			int pos = IndexOf(item);
			if (pos >= 0) 
			{
				RemoveAt(pos);
				return true;
			} 
			else
				return false;
		}
		
		/// <summary>
		///		Obtiene el enumerador
		/// </summary>
		public IEnumerator<SharpTreeNode> GetEnumerator() => Nodes.GetEnumerator();
		
		/// <summary>
		///		Obtiene el enumerador
		/// </summary>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => Nodes.GetEnumerator();
		
		public void RemoveAll(Predicate<SharpTreeNode> match)
		{
			if (match == null)
				throw new ArgumentNullException("match");
			ThrowOnReentrancy();
			int firstToRemove = 0;
			for (int i = 0; i < Nodes.Count; i++) {
				bool removeNode;
				isRaisingEvent = true;
				try {
					removeNode = match(Nodes[i]);
				} finally {
					isRaisingEvent = false;
				}
				if (!removeNode) {
					if (firstToRemove < i) {
						RemoveRange(firstToRemove, i - firstToRemove);
						i = firstToRemove - 1;
					} else {
						firstToRemove = i + 1;
					}
					Debug.Assert(firstToRemove == i + 1);
				}
			}
			if (firstToRemove < Nodes.Count) {
				RemoveRange(firstToRemove, Nodes.Count - firstToRemove);
			}
		}

		/// <summary>
		///		Nodo padre
		/// </summary>
		private SharpTreeNode Parent { get; }
		
		/// <summary>
		///		Número de elementos de la lista
		/// </summary>
		public int Count => Nodes.Count;
		
		/// <summary>
		///		Indica si la colección es de sólo lectura
		/// </summary>
		bool ICollection<SharpTreeNode>.IsReadOnly => false;

		/// <summary>
		///		Nodos de la lista
		/// </summary>
		private List<SharpTreeNode> Nodes { get; } = new();
	}
}
