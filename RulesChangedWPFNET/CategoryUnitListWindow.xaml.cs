using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Text.RegularExpressions;

namespace RulesChangedWPFNET
{
    /// <summary>
    /// Interaction logic for CategoryUnitListWindow.xaml
    /// </summary>
    public partial class CategoryUnitListWindow : Window
    {
        public CategoryUnitListWindow(GlobalProperty.SublistIndex windowIndex, MainWindow parent)
        {
            InitializeComponent();
            m_index = (int)windowIndex;
            m_parent = parent;
            mainListView.ItemsSource = displaySource;
            m_searchField = null;
            SearchBox.IsReadOnly = true;
        }

        private int m_index;
        private string m_tagSelected = "";
        private MainWindow m_parent;
        public ObservableCollection<DisplayItem> displaySource = new ObservableCollection<DisplayItem>();
        private Nullable<bool> m_searchField; // true for code name; false for display name;

        private void mainListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((FrameworkElement)e.OriginalSource).DataContext as DisplayItem;
            if (item != null)
            {
                AttributesModWindow newWindow = new AttributesModWindow(item.BINDING_CODENAME, this, this.m_index);
                this.Hide();
                newWindow.Show();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            m_parent.Show();
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (KeyValuePair<string, Hashtable> item in m_parent.dataSets[m_index])
            {
                var dpName = item.Value["Name"] as string;
                if (dpName == null)
                    dpName = "";
                this.displaySource.Add(new DisplayItem((string)item.Key, (string)((item.Value)["Name"])));
            }
            DeleteButton.IsEnabled = false;
        }

        private void RadioButton_codename_Click(object sender, RoutedEventArgs e)
        {
            m_searchField = true;
            SearchBox.IsReadOnly = false;
            SearchBox.Clear();
        }

        private void RadioButton_displayname_Click(object sender, RoutedEventArgs e)
        {
            m_searchField = false;
            SearchBox.IsReadOnly = false;
            SearchBox.Clear();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string textOrig = SearchBox.Text.ToLower();

            if (m_searchField == true)
            {
                var emFiltered = from item in displaySource
                                 let ename = item.BINDING_CODENAME.ToLower()
                                 where ename.Contains(textOrig)
                                 select item;

                mainListView.ItemsSource = emFiltered;
            }
            else
            {
                // Sometimes we have (displayname == null) and have to add a new condition
                // otherwise unwanted exception will raise.
                var emFiltered = from item in displaySource
                                 let ename = item.BINDING_DISPLAYNAME
                                 where ename != null && ename.ToLower().Contains(textOrig)
                                 select item;

                mainListView.ItemsSource = emFiltered;
            }
        }

        private void mainListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mainListView.SelectedItem != null)
            {
                DisplayItem itemSelected = mainListView.SelectedItem as DisplayItem;
                m_tagSelected = itemSelected.BINDING_CODENAME;
                label_selectionName.Content = m_tagSelected;
                DeleteButton.IsEnabled = true;
            }
            else
            {
                m_tagSelected = "";
                label_selectionName.Content = "NONE";
                DeleteButton.IsEnabled = false;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (m_parent.dataSets[m_index].ContainsKey(m_tagSelected))
            {
                m_parent.dataSets[m_index].Remove(m_tagSelected);
                m_parent.tagCategorizedList.Remove(m_tagSelected);
                if (m_index == 0) // building special treatment
                {
                    m_parent.buildingList_ordered.Remove(m_tagSelected);
                }
                this.displaySource.Remove(this.displaySource.Where(i => i.BINDING_CODENAME == m_tagSelected).Single());
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // all whitespaces are purged.
            Regex sWhitespace1 = new Regex(@"\s+");
            string itemToAdd = sWhitespace1.Replace(this.NewItemTextBox.Text,"");
            if (itemToAdd != "")
            {
                this.NewItemTextBox.Text = "";
                if (!m_parent.dataSets[m_index].ContainsKey(itemToAdd))
                {
                    m_parent.dataSets[m_index].Add(itemToAdd, new Hashtable());
                    m_parent.tagCategorizedList.Add(itemToAdd, (GlobalProperty.SublistIndex)m_index);
                    if (m_index == 0)
                    {
                        m_parent.buildingList_ordered.Add(itemToAdd);
                    }
                    this.displaySource.Add(new DisplayItem(itemToAdd, ""));
                    AttributesModWindow newWindow = new AttributesModWindow(itemToAdd, this, this.m_index);
                    this.Hide();
                    newWindow.Show();
                }
                else
                {
                    this.NewItemTextBox.Text = "";
                }
            }
        }
    }

    public class DisplayItem
    {
        public string BINDING_CODENAME { get; set; }
        public string BINDING_DISPLAYNAME { get; set; }

        public DisplayItem(string bc, string bd)
        {
            BINDING_CODENAME = bc;
            BINDING_DISPLAYNAME = bd;
        }

        //public override bool Equals(object obj) => this.Equals(obj as DisplayItem);

        //public bool Equals(DisplayItem item)
        //{
        //    if (item == null)
        //    {
        //        return false;
        //    }
            
        //    if (Object.ReferenceEquals(this, item))
        //    {
        //        return true;
        //    }

        //    if (this.GetType() != item.GetType())
        //    {
        //        return false;
        //    }

        //    return (this.BINDING_CODENAME == item.BINDING_CODENAME);
        //}
    }
}
