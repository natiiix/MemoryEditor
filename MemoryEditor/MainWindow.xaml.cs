using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MemoryEditor
{
    public partial class MainWindow : Window
    {
        private const int LISTVIEW_MAX_VALUE_COUNT = 1000;

        private IOrderedEnumerable<ProcessInfo> ProcList = null;
        private List<AddressValue> AddrVals = new List<AddressValue>();

        private DispatcherTimer TimerRefreshValues = new DispatcherTimer()
        {
            IsEnabled = false,
            Interval = new TimeSpan(0, 0, 0, 0, 500)
        };

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TimerRefreshValues.Tick += TimerRefreshValues_Tick;

            RefreshProcessList();
            RefreshSelectedProcess();
        }

        private void TimerRefreshValues_Tick(object sender, EventArgs e)
        {
            RefreshValues(true);
            UpdateValueCount();
        }

        private void RefreshProcessList()
        {
            // Get a list of running processes and order them by their name
            ProcList = Process.GetProcesses().Select(x => new ProcessInfo(x)).OrderBy(x => x.ProcessName);
            // Display the process list
            DisplayProcessList();
        }

        private void DisplayProcessList()
        {
            string filter = TextBoxProcessFilter.Text;
            ListViewProcessList.ItemsSource = ProcList.Where(x => x.FitsFilter(filter));
        }

        private void ButtonRefreshProcessList_Click(object sender, RoutedEventArgs e)
        {
            RefreshProcessList();
        }

        private void ListBoxProcessList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshSelectedProcess();
        }

        private void TextBoxProcessFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            DisplayProcessList();
        }

        private void RefreshSelectedProcess()
        {
            // The process controls are enabled only if a process is selected
            GridProcessControl.IsEnabled = ListViewProcessList.SelectedIndex >= 0;

            // Update the UI
            AddrVals.Clear();
            UpdateListViewAddressesItems();

            ButtonScanFilter.IsEnabled = false;
            GridValueControl.IsEnabled = false;

            // Display a placeholder string
            LabelValueCount.Content = "number of matching values";

            // Prevent values from being refreshed because the original process is no longer selected
            TimerRefreshValues.Stop();
        }

        private void ButtonScan_Click(object sender, RoutedEventArgs e)
        {
            // If the scan value is a valid integer
            if (int.TryParse(TextBoxScanValue.Text, out int scanValue))
            {
                // Get PID of the selected process
                int pid = (ListViewProcessList.SelectedItem as ProcessInfo).Id;

                // Get pointers to the specified value in memory of the selected process
                List<IntPtr> ptrs = ProcessMemory.Find(pid, scanValue);

                // Clear the list
                AddrVals.Clear();
                // Populate the list with new address/value objects that contain vital information about the matching values
                ptrs.ForEach(x => AddrVals.Add(new AddressValue(pid, x)));

                // Update the UI
                UpdateListViewAddressesItems();
                UpdateValueCount();

                // Start refreshing the values
                TimerRefreshValues.Start();
            }
            else
            {
                ShowErrorBox("Invalid scan value!");
            }
        }

        private void ButtonScanFilter_Click(object sender, RoutedEventArgs e)
        {
            // If the filter value is a valid integer
            if (int.TryParse(TextBoxScanValue.Text, out int filterValue))
            {
                // Get the current values
                // The property changed event must not be raised when filtering out values
                RefreshValues(true);

                // Remove values that don't fit the filter
                AddrVals.RemoveAll(x => !x.Filter(filterValue));

                // Update the UI
                UpdateListViewAddressesItems();
                UpdateValueCount();
            }
            else
            {
                ShowErrorBox("Invalid filter value!");
            }
        }

        private void RefreshValues(bool raisePropertyChanged)
        {
            AddrVals.ForEach(x => x.RefreshValue(raisePropertyChanged));
        }

        private void UpdateListViewAddressesItems()
        {
            // Clear the list view
            ListViewAddresses.ClearValue(ItemsControl.ItemsSourceProperty);

            // Check if there are too many values to display (displaying all of them would freeze the UI)
            if (AddrVals.Count > LISTVIEW_MAX_VALUE_COUNT)
            {
                // Display an error message box
                ShowErrorBox("Too many values to display!" + Environment.NewLine +
                    "Displaying first " + LISTVIEW_MAX_VALUE_COUNT.ToString() + " values.");

                // Only display the first N values
                ListViewAddresses.ItemsSource = AddrVals.GetRange(0, LISTVIEW_MAX_VALUE_COUNT);
            }
            // The number of values is not too high
            else
            {
                // Display all the values
                ListViewAddresses.ItemsSource = AddrVals;
            }
        }

        private void UpdateValueCount()
        {
            // Get the number of values that have changed
            int nHasChanged = AddrVals.Count(x => x.HasChanged);

            // Changes to UI properties must be done on the UI thread (not the event thread)
            ButtonScanFilter.IsEnabled = (nHasChanged > 0);
            LabelValueCount.Content = string.Format("{0} matching values found ({1} have changed)", AddrVals.Count, nHasChanged);
        }

        private static void ShowErrorBox(string errorMessage)
        {
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ListViewProcessList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshSelectedProcess();
        }

        private void ListViewAddresses_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Automatically change the size of each column to fill the list view
            double workingWidth = ListViewAddresses.ActualWidth - SystemParameters.VerticalScrollBarWidth - 10;

            GridView gView = ListViewAddresses.View as GridView;

            gView.Columns[0].Width = workingWidth * 0.67;
            gView.Columns[1].Width = workingWidth * 0.33;
        }

        private void ListViewAddresses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Enable value control buttons
            GridValueControl.IsEnabled = (ListViewAddresses.SelectedIndex >= 0);
        }

        private void ButtonValueEdit_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected value
            AddressValue selectedValue = ListViewAddresses.SelectedItem as AddressValue;

            // Ask the user to enter the new value (use the old value as an initial value in the input dialog)
            WindowInputDialog dialog = new WindowInputDialog(selectedValue.Value.ToString());
            dialog.ShowDialog();

            // Get the desired new value
            string strValue = dialog.InputValue;

            // If the input value is a valid integer
            if (int.TryParse(strValue, out int editValue))
            {
                // Re-write the value with the speicified value
                selectedValue.Edit(editValue);
            }
            else
            {
                ShowErrorBox("Invalid input value!");
            }
        }

        private void ButtonValueCopy_Click(object sender, RoutedEventArgs e)
        {
            // Copy the selected value to the scan value text box
            TextBoxScanValue.Text = (ListViewAddresses.SelectedItem as AddressValue).Value.ToString();
        }
    }
}