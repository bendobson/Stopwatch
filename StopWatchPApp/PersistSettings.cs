using System;
using System.IO.IsolatedStorage;

namespace StopWatchPApp
{
    public class PersistSettings <T>
    {
        string name;
        T value;
        T initialValue;

        public PersistSettings(string name, T value)
        {
            this.name = name;
            this.initialValue = value;
        }

        public T Value
        {
            get
            {
                // Try to get the value from Isolated Storage
                if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue(
                        this.name, out this.value))
                {
                    // Value is not set
                    IsolatedStorageSettings.ApplicationSettings[this.name] = this.initialValue;
                }

                return this.value;
            }

            set
            {
                // Save the value to Isolated Storage
                IsolatedStorageSettings.ApplicationSettings[this.name] = value;
                this.value = value;
            }
        }

    }
}
