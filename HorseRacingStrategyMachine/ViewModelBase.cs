using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HorseRacingStrategyMachine
{
    public partial class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void RunOnUi(Action action)
        {
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(action);
            }
            else
            {
                action.Invoke();
            }
        }
    }
}
