using BlitzInfo.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlitzInfo.ViewModel
{
    public class CountryData : BaseViewModel
    {
        private string _countrycode;
        private int _countrycount;
        public string CountryCode
        {
            get
            {
                return _countrycode;
            }
            set
            {
                _countrycode = value;
                OnPropertyChanged("CountryCode");
            }
        }
        public int CountryCount
        {
            get
            {
                return _countrycount;
            }
            set
            {
                _countrycount = value;
                OnPropertyChanged("CountryCount");
            }
        }
    }
    public class BlitzCountryStatItem : BaseViewModel
    {
        private DateTime _time;
        private int _allcount;
        private ObservableCollection<CountryData> _countrydata;
        public DateTime Time
        {
            get
            {
                return _time;
            }
            set
            {
                _time = value;
                OnPropertyChanged("Time");
            }
        }
        public ObservableCollection<CountryData> Countries
        {
            get
            {
                return _countrydata;
            }
            set
            {
                _countrydata = value;
                OnPropertyChanged("CountryData");
            }
        }
        public int AllCount
        {
            get
            {
                return _allcount;
            }
            set
            {
                _allcount = value;
                OnPropertyChanged("AllCount");
            }
        }
    }
}
