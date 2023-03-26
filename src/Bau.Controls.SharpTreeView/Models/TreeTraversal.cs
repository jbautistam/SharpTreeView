using System;
using System.Collections.Generic;

namespace Bau.Controls.SharpTreeView.Models
{
    /// <summary>
    ///		Métodos estáticos para atravesar árboles
    /// </summary>
    internal static class TreeTraversal
    {
        /// <summary>
        ///		Convierte una estructura de árbol en una lista plana atravesándolos en preorden
        /// </summary>
        internal static IEnumerable<TypeData> PreOrder<TypeData>(TypeData root, Func<TypeData, IEnumerable<TypeData>> recursion)
        {
            return PreOrder(new TypeData[] { root }, recursion);
        }

        /// <summary>
        ///		Convierte una estructura de árbol en una lista plana atravesándolos en preorden
        /// </summary>
        internal static IEnumerable<TypeData> PreOrder<TypeData>(IEnumerable<TypeData> input, Func<TypeData, IEnumerable<TypeData>> recursion)
        {
            Stack<IEnumerator<TypeData>> stack = new Stack<IEnumerator<TypeData>>();

            try
            {
                stack.Push(input.GetEnumerator());
                while (stack.Count > 0)
                {
                    while (stack.Peek().MoveNext())
                    {
                        TypeData element = stack.Peek().Current;

                        yield return element;
                        IEnumerable<TypeData> children = recursion(element);

                        if (children is not null)
                            stack.Push(children.GetEnumerator());
                    }
                    // Quita el elemento de la pila
                    stack.Pop().Dispose();
                }
            }
            finally
            {
                // Limpia la pila
                while (stack.Count > 0)
                    stack.Pop().Dispose();
            }
        }
    }
}
