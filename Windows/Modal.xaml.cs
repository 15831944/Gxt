
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Gxt.ElevationProfileDesigner;
using Autodesk.AutoCAD.DatabaseServices;

namespace Gxt.Windows
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ModalDialog : Window
    {
        public ModalDialog()
        {
            InitializeComponent();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }


        private void DataGrid_Loader(object sender, RoutedEventArgs e)
        {
            // ... Create a List of objects.
            var items = new List<ProfileObject>();
            Nod nod = new Nod();

            foreach (DBDictionaryEntry dbEntry in nod.AppDictionary)
            {
                Xrecord values = trans.
                items.Add()
            }

            // ... Assign ItemsSource of DataGrid.
            var grid = sender as DataGrid;
            grid.ItemsSource = items;
        }

    }
}

