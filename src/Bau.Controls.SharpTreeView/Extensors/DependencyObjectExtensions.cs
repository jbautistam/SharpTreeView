using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows;

namespace Bau.Controls.SharpTreeView.Extensors
{
    /// <summary>
    ///		Métodos de extensión
    /// </summary>
    internal static class DependencyObjectExtensions
    {
        /// <summary>
        ///		Busca el ancestro de un elemento
        /// </summary>
        internal static TypeData? FindAncestor<TypeData>(this DependencyObject dependency) where TypeData : class
        {
            return dependency.AncestorsAndSelf().OfType<TypeData>().FirstOrDefault();
        }

        /// <summary>
        ///		Devuelve los elementos padre y él mismo
        /// </summary>
        internal static IEnumerable<DependencyObject> AncestorsAndSelf(this DependencyObject dependency)
        {
            while (dependency is not null)
            {
                // Deuelve el elemento
                yield return dependency;
                // Obtiene el padre
                dependency = VisualTreeHelper.GetParent(dependency);
            }
        }
    }
}
