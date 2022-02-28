using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RulesChangedWPFNET
{
    /// <summary>
    /// Interaction logic for BuildingItemMod.xaml
    /// </summary>

    public class AttributesDisplayItem
    {
        public string BINDING_ATTRIBUTES_NAME { get; set; }
        public string BINDING_ATTRIBUTES_VALUE { get; set; }

        public AttributesDisplayItem(string an, string av)
        {
            BINDING_ATTRIBUTES_NAME = an;
            BINDING_ATTRIBUTES_VALUE = av;
        }
    }

    public partial class AttributesModWindow : Window
    {
        private Hashtable itemToModify;
        private Window m_parent;
        ObservableCollection<AttributesDisplayItem> attributesList = new ObservableCollection<AttributesDisplayItem>();
        public AttributesModWindow(Hashtable ht, Window parent)
        {
            InitializeComponent();
            itemToModify = ht;
            m_parent = parent;
            attributesDataGrid.ItemsSource = attributesList;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            m_parent.Show();
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (DictionaryEntry s in itemToModify)
            {
                this.attributesList.Add(new AttributesDisplayItem((string)s.Key, (string)s.Value));
            }
        }

        private void attributesDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // Very tricky and ugly to do so; but it works and saves time.
            // Should not have any memory leak issue.
            string attributesName_to_update = ((AttributesDisplayItem)((DataGridRow)e.Row).Item).BINDING_ATTRIBUTES_NAME;
            string attributesValue_to_update = ((TextBox)e.EditingElement).Text;

            itemToModify[attributesName_to_update] = attributesValue_to_update;
        }
    }
}
