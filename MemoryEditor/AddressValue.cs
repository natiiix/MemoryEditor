using System;
using System.ComponentModel;

namespace MemoryEditor
{
    public class AddressValue : INotifyPropertyChanged
    {
        private readonly int ProcessId;
        private readonly IntPtr Address;

        public string AddressString { get; }
        public int Value { get; private set; }
        public bool HasChanged { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public AddressValue(int pid, IntPtr ptr)
        {
            ProcessId = pid;
            Address = ptr;
            AddressString = Address.ToHexString();

            Value = GetValue();
            HasChanged = false;
        }

        private int GetValue()
        {
            return ProcessMemory.Read(ProcessId, Address);
        }

        public void RefreshValue()
        {
            int oldValue = Value;
            Value = GetValue();

            // If the value has changed
            if (Value != oldValue)
            {
                // If this is the first time the value has changed
                if (!HasChanged)
                {
                    HasChanged = true;

                    // Notify the list view so that it knows to change the background color of the row
                    RaisePropertyChanged("HasChanged");
                }

                // Tell the list view that it needs to update the value
                RaisePropertyChanged("Value");
            }
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool Filter(int filterValue)
        {
            // Reset the "has changed" property
            HasChanged = false;

            // Return boolean value indicating whether the current value is equal to the filter value
            return Value == filterValue;
        }

        public bool Edit(int editValue)
        {
            // Change the value of this variable to the specified value
            return ProcessMemory.Write(ProcessId, Address, editValue);
        }
    }
}