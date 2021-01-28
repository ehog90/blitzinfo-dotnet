using System;
using System.Collections.ObjectModel;

namespace BlitzInfo.ViewModel
{
    public class CountryData : BaseViewModel
    {
        private string _countrycode;
        private int _countryCount;

        public string CountryCode
        {
            get => _countrycode;
            set
            {
                _countrycode = value;
                OnPropertyChanged("CountryCode");
            }
        }

        public int CountryCount
        {
            get => _countryCount;
            set
            {
                _countryCount = value;
                OnPropertyChanged("CountryCount");
            }
        }
    }

    public class BlitzCountryStatItem : BaseViewModel
    {
        private int _allcount;
        private ObservableCollection<CountryData> _countryData;
        private DateTime _time;

        public DateTime Time
        {
            get => _time;
            set
            {
                _time = value;
                OnPropertyChanged("Time");
            }
        }

        public ObservableCollection<CountryData> Countries
        {
            get => _countryData;
            set
            {
                _countryData = value;
                OnPropertyChanged("CountryData");
            }
        }

        public int AllCount
        {
            get => _allcount;
            set
            {
                _allcount = value;
                OnPropertyChanged("AllCount");
            }
        }
    }
}