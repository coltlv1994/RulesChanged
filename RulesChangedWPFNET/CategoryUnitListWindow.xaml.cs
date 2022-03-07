﻿using System;
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
        }

        private int m_index;
        private MainWindow m_parent;
        public ObservableCollection<DisplayItem> displaySource = new ObservableCollection<DisplayItem>();

        private void mainListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((FrameworkElement)e.OriginalSource).DataContext as DisplayItem;
            if (item != null)
            {
                AttributesModWindow newWindow = new AttributesModWindow((m_parent.dataSets[m_index])[item.BINDING_CODENAME], this);
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
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string textOrig = SearchBox.Text;
            string upper = textOrig.ToUpper();
            string lower = textOrig.ToLower();

            var emFiltered = from item in displaySource
                             let ename = item.BINDING_CODENAME
                             where ename.StartsWith(upper) || ename.StartsWith(lower) || ename.Contains(textOrig)
                             select item;

            mainListView.ItemsSource = emFiltered;
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
    }
}
