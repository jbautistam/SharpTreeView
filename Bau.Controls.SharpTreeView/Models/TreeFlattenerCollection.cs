using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Bau.Controls.SharpTreeView.Models
{
    /// <summary>
    ///     Colección que mantiene el árbol en una colección plana
    /// </summary>
    internal sealed class TreeFlattenerCollection : IList, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        /// <summary>
        /// The root node of the flat list tree.
        /// This is not necessarily the root of the model!
        /// </summary>
        internal SharpTreeNode _root;
        private readonly bool _includeRoot;
        private readonly object _syncRoot = new object();

        public TreeFlattenerCollection(SharpTreeNode modelRoot, bool includeRoot)
        {
            _root = modelRoot;
            while (_root.listParent != null)
                _root = _root.listParent;
            _root.TreeFlattener = this;
            _includeRoot = includeRoot;
        }

        public void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, e);
        }

        public void NodesInserted(int index, IEnumerable<SharpTreeNode> nodes)
        {
            if (!_includeRoot)
                index--;
            foreach (SharpTreeNode node in nodes)
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, node, index++));
        }

        public void NodesRemoved(int index, IEnumerable<SharpTreeNode> nodes)
        {
            if (!_includeRoot)
                index--;
            foreach (SharpTreeNode node in nodes)
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, node, index));
        }

        public void Stop()
        {
            Debug.Assert(_root.TreeFlattener == this);
            _root.TreeFlattener = null;
        }

        public object this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException();
                return SharpTreeNode.GetNodeByVisibleIndex(_root, _includeRoot ? index : index + 1);
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public int Count
        {
            get
            {
                return _includeRoot ? _root.GetTotalListLength() : _root.GetTotalListLength() - 1;
            }
        }

        public int IndexOf(object? item)
        {
            if (item is SharpTreeNode node)
            {
                if (node.IsVisible && node.GetListRoot() == _root)
                {
                    if (_includeRoot)
                        return SharpTreeNode.GetVisibleIndexForNode(node);
                    else
                        return SharpTreeNode.GetVisibleIndexForNode(node) - 1;
                }
            }
            return -1;
        }

        public bool IsReadOnly => true;

        public bool IsFixedSize => false;

        public bool IsSynchronized => false;

        public object SyncRoot => _syncRoot;

        public void Insert(int index, object? item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public int Add(object? item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(object? item)
        {
            return IndexOf(item) >= 0;
        }

        public void CopyTo(Array array, int arrayIndex)
        {
            foreach (object item in this)
                array.SetValue(item, arrayIndex++);
        }

        public void Remove(object? item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///		Obtiene el enumerador
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return this[i];
        }
    }
}
