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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace MemoryEditor
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateProcessList();
        }

        private void UpdateProcessList()
        {
            ListBoxProcessList.ItemsSource = Process.GetProcesses().Select(x => new ProcessInfo(x)).OrderBy(x => x.ProcessName);
        }

        private void ButtonRefreshProcessList_Click(object sender, RoutedEventArgs e)
        {
            UpdateProcessList();
        }

        private void ListBoxProcessList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MessageBox.Show("Selection Changed: " + ListBoxProcessList.SelectedIndex.ToString());
        }
    }
}