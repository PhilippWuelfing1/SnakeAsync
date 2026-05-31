using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Linq;

namespace SnakeAsync
{
    public class ViewModel : INotifyPropertyChanged
    {
        public ViewModel()
        {

        }

        private string _name = "John Doe";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string PersonName
        {
            get { return _name; }
            set
            {
                _name = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged(PersonName);
            }
        }

    }
}
