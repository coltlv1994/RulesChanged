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
        private Hashtable m_itemToModify;
        private string m_tag;
        private Window m_parent;
        private MainWindow m_mainWindow;
        private int m_sublistIndex; // for convenience, this is passed by parent.
        ObservableCollection<AttributesDisplayItem> attributesList = new ObservableCollection<AttributesDisplayItem>();
        ObservableCollection<string> availableAttributeList = new ObservableCollection<string>();
        //List<string> availableAttributeList = new List<string>();
        public AttributesModWindow(string tagToDisplay, Window parent, int sIndex)
        {
            InitializeComponent();
            m_tag = tagToDisplay;
            m_parent = parent;
            m_sublistIndex = sIndex;
            m_mainWindow = (MainWindow)Application.Current.MainWindow;
            m_itemToModify = (m_mainWindow.dataSets[(int)m_mainWindow.tagCategorizedList[m_tag]])[m_tag];
            this.Title = "Attributes Modification: " + m_tag;
            attributesDataGrid.ItemsSource = attributesList;
            if (m_sublistIndex == 8) // uncategorized
            {
                this.attribute_comboBox.IsEnabled = false;
                this.Add_button.IsEnabled = false;
                this.Clear_button.IsEnabled = false;
            }
            else
            {
                availableAttributeList = new ObservableCollection<string>(m_mainWindow.attributesFieldAllowedList[(GlobalProperty.SublistIndex)m_sublistIndex]);
                attribute_comboBox.ItemsSource = availableAttributeList;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            m_parent.Show();
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
         
            //availableAttributeList = new List<string>(m_mainWindow.attributesFieldAllowedList[(GlobalProperty.SublistIndex)m_sublistIndex]);

            foreach (DictionaryEntry s in m_itemToModify)
            {
                this.attributesList.Add(new AttributesDisplayItem((string)s.Key, (string)s.Value));
                if (m_sublistIndex != 8)
                {
                    this.availableAttributeList.Remove((string)s.Key);
                }
            }
        }

        private void attributesDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // Very tricky and ugly to do so; but it works and saves time.
            // Should not have any memory leak issue.
            string attributesName_to_update = ((AttributesDisplayItem)((DataGridRow)e.Row).Item).BINDING_ATTRIBUTES_NAME;
            string attributesValue_to_update = ((TextBox)e.EditingElement).Text;

            m_itemToModify[attributesName_to_update] = attributesValue_to_update;
        }

        private void attributesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string attributeField = ((TextBlock)e.OriginalSource).Text;
            if ((attributeField != m_tag) && (m_mainWindow.tagCategorizedList.ContainsKey(attributeField)))
            {
                int sIndex = (int)m_mainWindow.tagCategorizedList[attributeField];
                AttributesModWindow newModWindow = new AttributesModWindow(attributeField, this, sIndex);
                this.Hide();
                newModWindow.Show();
            }
        }

        private void Add_button_Click(object sender, RoutedEventArgs e)
        {
            string attributeToAdd = this.attribute_comboBox.SelectedItem.ToString();
            string attributeValueToAdd = this.attributeValueTextbox.Text;

            if (attributeToAdd != "" && attributeValueToAdd != "")
            {
                if (Add_attribute(attributeToAdd, attributeValueToAdd))
                {
                    // success
                    this.attributeValueTextbox.Text = "";
                }
                else
                {
                    // failed
                }
            }
            else
            {
                // field cannot be empty
            }
        }

        private void Clear_button_Click(object sender, RoutedEventArgs e)
        {
            this.attributeValueTextbox.Text = "";
        }

        private bool Add_attribute(string attributeName, string attributeValue)
        {
            m_mainWindow.dataSets[m_sublistIndex][m_tag].Add(attributeName, attributeValue);
            availableAttributeList.Remove(attributeName);
            attributesList.Add(new AttributesDisplayItem(attributeName, attributeValue));
            return true;
        }

        private bool Remove_attribute(string attributeName)
        {
            if (m_mainWindow.dataSets[m_sublistIndex][m_tag].ContainsKey(attributeName))
            {
                m_mainWindow.dataSets[m_sublistIndex][m_tag].Remove(attributeName);
            }
            attributesList.Remove(attributesList.Where(i => i.BINDING_ATTRIBUTES_NAME == attributeName).Single());
            availableAttributeList.Add(attributeName);
            return true;
            
        }

        private void attributesDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                AttributesDisplayItem item = ((DataGridCell)e.OriginalSource).DataContext as AttributesDisplayItem;
                if (item != null)
                {
                    Remove_attribute(item.BINDING_ATTRIBUTES_NAME);
                }
            }
        }

        private void attribute_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.attributeValueTextbox.Text = "";
        }
    }
}
