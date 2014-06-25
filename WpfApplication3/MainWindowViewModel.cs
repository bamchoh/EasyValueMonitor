using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace EasyValueMonitor
{
    public class DataList : INotifyPropertyChanged
    {
        public int Value
        {
            get { return _value; }

            set
            {
                if (_value != value)
                {
                    _value = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Value"));
                }
            }
        }

        public int Address
        {
            get { return _address; }

            set
            {
                if (_address != value)
                {
                    _address = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Address"));
                }
            }
        }

        public int WriteValue
        {
            get { return _writevalue; }

            set
            {
                _writevalue = value;
                PropertyChanged(this, new PropertyChangedEventArgs("WriteValue"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private int _value;

        private int _address;

        private int _writevalue;

    }

    public class MainWindowViewModel
    {
        public MainWindowViewModel()
        {
            model = new MainWindowModel();
        }

        public DataList DataList
        {
            get
            {
                return model.DataList;
            }
        }

        public void WriteValue(string value)
        {
            DataList.WriteValue = Convert.ToInt16(value);
        }

        private MainWindowModel model;

    }

}
