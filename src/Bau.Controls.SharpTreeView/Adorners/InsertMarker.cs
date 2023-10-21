using System;
using System.Windows.Controls;
using System.Windows;

namespace Bau.Controls.SharpTreeView.Adorners
{
    /// <summary>
    ///		Control de marcador del punto de inserción de un drop
    /// </summary>
    internal class InsertMarker : Control
    {
        static InsertMarker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(InsertMarker), new FrameworkPropertyMetadata(typeof(InsertMarker)));
        }
    }
}
