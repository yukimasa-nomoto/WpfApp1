using System;
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

namespace WpfApp1
{
    /// <summary>
    /// MainWindow2.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow2 : Window
    {
        public class DataGridItems
        {
            public DataGridItems(string items0, string items1, string items2, string items3)
            {
                this.items0 = items0;
                this.items1 = items1;
                this.items2 = items2;
                this.items3 = items3;
            }

            public string items0 { get; set; }
            public string items1 { get; set; }
            public string items2 { get; set; }
            public string items3 { get; set; }
        }

        public MainWindow2()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            List<DataGridItems> items = new List<DataGridItems>();
            items.Add(new DataGridItems("000", "111", "222", "333"));
            items.Add(new DataGridItems("aaa", "bbb", "ccc", "ddd"));
            items.Add(new DataGridItems("AAA", "BBB", "CCC", "DDD"));

            DataGridName.ItemsSource = items;
        }
    }
}
