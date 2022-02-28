using System;
using System.Collections;
using System.Collections.Generic;
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
        public CategoryUnitListWindow(Dictionary<string, Hashtable> dpL, MainWindow parent)
        {
            InitializeComponent();
            displayList = dpL;
            m_parent = parent;
        }

        public Dictionary<string, Hashtable> displayList;
        private MainWindow m_parent;

        private void mainListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((FrameworkElement)e.OriginalSource).DataContext as DisplayItem;
            if (item != null)
            {
                AttributesModWindow newWindow = new AttributesModWindow(displayList[item.BINDING_CODENAME], this);
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
            foreach (KeyValuePair<string, Hashtable> item in displayList)
            {
                var dpName = item.Value["Name"] as string;
                if (dpName == null)
                    dpName = "";
                mainListView.Items.Add(new DisplayItem(item.Key, dpName));
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
    }
}
