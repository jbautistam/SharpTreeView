using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace Bau.Controls.SharpTreeView.Editors
{
    /// <summary>
    ///		Cuadro de texto de edición para el árbol
    /// </summary>
    public class EditTextBox : TextBox
    {
        // Variables privadas
        private bool commiting;

        static EditTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EditTextBox), new FrameworkPropertyMetadata(typeof(EditTextBox)));
        }

        public EditTextBox()
        {
            Loaded += (_, _) => Init();
        }

        /// <summary>
        ///		Inicializa el cuadro de texto
        /// </summary>
        public void Init()
        {
            // Cambia el texto
            Text = Node?.LoadEditText();
            // Selecciona el control y todo su texto
            Focus();
            SelectAll();
        }

        /// <summary>
        ///		Finaliza la edición
        /// </summary>
        private void Commit()
        {
            if (!commiting)
            {
                // Indica que se está finalizando la edición
                commiting = true;
                // Cambia los datos del nodo que indican si está editando
                if (Node is not null)
                {
                    // Indica que ya no está editando
                    Node.IsEditing = false;
                    // Graba los datos del nodo y cambia el foco
                    if (!Node.SaveEditText(Text))
                        Item?.Focus();
                    // Lanza el evento de modificación de la propiedad
                    Node.RaisePropertyChanged(nameof(SharpTreeNode.Text));
                }
                // Indica que se ha finalizado la edición
                commiting = false;
            }
        }

        /// <summary>
        ///		Sobrescribe el evento de tecla pulsada
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Commit();
            else if (e.Key == Key.Escape && Node is not null)
                Node.IsEditing = false;
        }

        /// <summary>
        ///		Sobrescribe el evento de pérdida de foco
        /// </summary>
        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            if (Node is not null && Node.IsEditing)
                Commit();
        }

        /// <summary>
        ///		Elemento sobre el que se muestra el cuadro de texto de edición
        /// </summary>
        public SharpTreeViewItem? Item { get; set; }

        /// <summary>
        ///		Nodo sobre el que se muestra el cuadro de texto de edición
        /// </summary>
        public SharpTreeNode? Node
        {
            get { return Item?.Node; }
        }
    }
}
