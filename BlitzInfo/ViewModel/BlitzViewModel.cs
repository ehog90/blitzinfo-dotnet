using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Threading;
using BlitzInfo.Model;
using BlitzInfo.Model.Entities;
using BlitzInfo.Utils;
using OxyPlot;
using OxyPlot.Axes;

namespace BlitzInfo.ViewModel
{
    public class DirectionItem
    {
        public bool IsChecked { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
    }

    public class BlitzViewModel : BaseViewModel
    {
        private void CountGot(object sender, EventArgs e)
        {
            // try
            //  {
            Application.Current.Dispatcher.Invoke(delegate
            {
                CountsList.Clear();
                var countsTempList = new List<StrokeTenmin>(_blitzmodel.GetCount);
                countsTempList.Reverse();
                foreach (var datarow in countsTempList)
                {
                    var d = new BlitzCountryStatItem();
                    d.Time = datarow.time;
                    d.AllCount = datarow.count;
                    d.Countries = new ObservableCollection<CountryData>();
                    foreach (var cd in datarow.country_data)
                    {
                        var cdv = new CountryData();
                        cdv.CountryCode = cd.countrycode;
                        cdv.CountryCount = cd.countrycount;
                        d.Countries.Add(cdv);
                    }

                    CountsList.Add(d);
                }

                OnPropertyChanged("CountsList");
                countsTempList.Reverse();

                Counts = new List<DataPoint>();
                countsTempList.ForEach(d => Counts.Add(new DataPoint(DateTimeAxis.ToDouble(d.time), d.count)));
                CountChartYMax = countsTempList.Max(d => d.count);
                CountChartYMax *= 1.1;
                CountChartXMin = DateTimeAxis.ToDouble(countsTempList.First().time);
                CountChartXMax = DateTimeAxis.ToDouble(countsTempList.Last().time);

                CountChartLoadText = "A grafikon utoljára frissítve: " + DateTime.Now.ToString("HH:mm");
                CountChartVisible = Visibility.Visible;

                OnPropertyChanged("CountChartLoadText");
                OnPropertyChanged("CountChartVisible");

                OnPropertyChanged("CountChartXMin");
                OnPropertyChanged("CountChartXMax");
                OnPropertyChanged("CountChartYMax");
                OnPropertyChanged("Counts");
            });


            //  }
            //  catch
            // {
            //   MessageBox.Show("HIBA");
            // }
        }

        private void CountQuery(object sender, EventArgs e)
        {
            CountChartLoadText = "A grafikon frissítése folyamatban...";
            CountChartVisible = Visibility.Hidden;
            OnPropertyChanged("CounChartLoadText");
            OnPropertyChanged("CountChartVisible");
        }

        /// <summary>
        ///     Uploads the manually.
        /// </summary>
        private void UploadManually()
        {
            /* _blitzmodel.ManualLog(new Model.BlitzEventArgs("Log feltöltés indítása F8-cal", "Log feltöltés", Model.BlitzEventArgs.EventMood.COMMAND));
             _blitzmodel.upload_logs();*/
        }

        private void ProcessChanged(object sender, ProcessEventArgs e)
        {
            if (e.count == 0)
                Proceeding = false;
            else
                Proceeding = true;

            OnPropertyChanged("Proceeding");
        }

        private void DistanceQuery(object sender, EventArgs e)
        {
            DistanceChartLoadText = "A grafikon frissítése folyamatban...";
            DistanceChartVisible = Visibility.Hidden;
            OnPropertyChanged("DistanceChartLoadText");
            OnPropertyChanged("DistanceChartVisible");
        }

        private void DistancesGot(object sender, EventArgs e)
        {
            var distances = new List<BlitzDistance>(_blitzmodel.GetDistances);
            var dataWithTypes = distances.GroupBy(m => m.dtype).ToList();
            MinimumDistance = new List<DataPoint>();
            //AverageDistance = new List<DataPoint>();
            dataWithTypes[1].ToList().ForEach(d =>
                MinimumDistance.Add(new DataPoint(DateTimeAxis.ToDouble(d.time), d.distance)));
            // dataWithTypes[0].ToList().ForEach(d => AverageDistance.Add(new DataPoint(DateTimeAxis.ToDouble(d.time), d.Distance)));
            DistanceChartYMax = dataWithTypes[1].Max(d => d.distance);
            DistanceChartYMax *= 1.1;
            DistanceChartXMin = DateTimeAxis.ToDouble(distances.First().time);
            DistanceChartXMax = DateTimeAxis.ToDouble(distances.Last().time);

            DistanceChartLoadText = "A grafikon utoljára frissítve: " + DateTime.Now.ToString("HH:mm");
            DistanceChartVisible = Visibility.Visible;

            OnPropertyChanged("DistanceChartLoadText");
            OnPropertyChanged("DistanceChartVisible");

            OnPropertyChanged("DistanceChartXMin");
            OnPropertyChanged("DistanceChartXMax");
            OnPropertyChanged("DistanceChartYMax");
            OnPropertyChanged("MinimumDistance");
            //   OnPropertyChanged("AverageDistance");
        }

        private void ShowBadge(object sender, BadgeEventArgs e)
        {
            string urisource;
            Application.Current.Dispatcher.Invoke(delegate
            {
                switch (e.badgeType)
                {
                    case BadgeEventArgs.BadgeType.CONNECTED:
                    {
                        urisource = @"pack://application:,,,/Resources/success.png";
                        break;
                    }
                    case BadgeEventArgs.BadgeType.CONNECTION_ERROR:
                    {
                        urisource = @"pack://application:,,,/Resources/error-01.png";
                        break;
                    }
                    default:
                    {
                        urisource = @"pack://application:,,,/Resources/error-01.png";
                        break;
                    }
                }

                NotifyPic = new BitmapImage();
                NotifyPic.BeginInit();
                NotifyPic.UriSource = new Uri(urisource, UriKind.RelativeOrAbsolute);
                NotifyPic.EndInit();
                NotifyPic.Freeze();
                OnPropertyChanged("NotifyPic");
            });
        }

        private void KeyPressed()
        {
            if (TabSelected == 0)
            {
                _blitzmodel.ManualLog(new BlitzEventArgs("Socket.IO Újrakapcsolódás", "Socket.IO-hozzáférés",
                    BlitzEventArgs.EventMood.COMMAND));
                _blitzmodel.ForwardGeocodingQuery(ProbeAddress);
            }
            else if (TabSelected == 1)
            {
                _blitzmodel.ManualLog(new BlitzEventArgs("Darabszám-grafikon manuális frissítése F5-tel",
                    "Villám darabszám-grafikon", BlitzEventArgs.EventMood.COMMAND));
                _blitzmodel.countProcess();
            }
            else if (TabSelected == 2)
            {
                _blitzmodel.ManualLog(new BlitzEventArgs("10 perces statisztikák manuális frissítése F5-tel",
                    "10 perces statisztika", BlitzEventArgs.EventMood.COMMAND));
                _blitzmodel.countProcess();
            }
            else if (TabSelected == 3)
            {
                _blitzmodel.ManualLog(new BlitzEventArgs("Hosszútávú statisztika manuális frissítése F5-tel",
                    "Hosszútávú statisztika", BlitzEventArgs.EventMood.COMMAND));
                _blitzmodel.OverallStatUpdate();
            }
            else if (TabSelected == 4)
            {
                _blitzmodel.ManualLog(new BlitzEventArgs("Összes állomásadat manuális frissítése F5-tel",
                    "Összes állomásadat", BlitzEventArgs.EventMood.COMMAND));
                _blitzmodel.StationsOverallStatUpdate();
            }
            else if (TabSelected == 5)
            {
                _blitzmodel.ManualLog(new BlitzEventArgs("Socket.IO Újrakapcsolódás", "Socket.IO-hozzáférés",
                    BlitzEventArgs.EventMood.COMMAND));
                _blitzmodel.ForwardGeocodingQuery(ProbeAddress);
                ServerLogs.Clear();
            }
        }

        private void AddressChanged(object sender, EventArgs e)
        {
            GeocodedAddress = _blitzmodel.Address;
            AddressFlag = new BitmapImage();
            AddressFlag.BeginInit();

            AddressFlag.UriSource = new Uri(@"pack://siteoforigin:,,,/Resources/" + _blitzmodel.Country + "-01.png",
                UriKind.RelativeOrAbsolute);
            AddressFlag.EndInit();
            AddressFlag.Freeze();

            OnPropertyChanged("AddressFlag");
            OnPropertyChanged("GeocodedAddress");
            OnPropertyChanged("ProbeAddress");
        }

        private void AddToLog(object sender, BlitzEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                var log = new Log
                {
                    timestamp = e.timestamp, header = e.msgheader, message = e.msg,
                    kind = _blitzmodel.moodtoString(e.mood)
                };
                Logs.Insert(0, log);
                OnPropertyChanged("Logs");
            });
        }

        private void ChangeAddress()
        {
            if (ProbeAddress.Length < 2)
            {
                onAddressInputError?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _blitzmodel.ForwardGeocodingQuery(ProbeAddress);
            }

            OnPropertyChanged("ProbeAddress");
        }

        #region Adattagok

        private DispatcherTimer _removeOldDataTimer;

        private readonly BlitzModel _blitzmodel;
        public ObservableCollection<Log> Logs { get; set; }
        public ObservableCollection<ServerLog> ServerLogs { get; }
        public ObservableCollection<Stroke> LiveBlitzes { get; set; }
        public ObservableCollection<OverallStatEntry> OverallStatistics { get; private set; }
        public ObservableCollection<BlitzCountryStatItem> CountsList { get; set; }
        public ObservableCollection<DirectionItem> DirectionItems { get; set; }
        public ObservableCollection<StationStatEntry> OverallStationStats { get; set; }
        public string GeocodedAddress { get; set; }
        public string ProbeAddress { get; set; }
        public DelegateCommand AddressChangeCommand { get; }
        public DelegateCommand KeyPressCommand { get; }
        public DelegateCommand ManualUploadCommand { get; }
        public DelegateCommand TresholdCommitCommand { get; }
        public DelegateCommand AddDirectionCommand { get; }
        public DelegateCommand SoundToggleCommand { get; private set; }
        public BitmapImage AddressFlag { get; private set; }
        public BitmapImage NotifyPic { get; private set; }

        public List<DataPoint> MinimumDistance { get; set; }
        public List<DataPoint> Counts { get; set; }

        public double DistanceChartYMax { get; set; }
        public double DistanceChartXMin { get; set; }
        public double DistanceChartXMax { get; set; }
        public double CountChartYMax { get; set; }
        public double CountChartXMin { get; set; }
        public double CountChartXMax { get; set; }

        public event EventHandler onAddressInputError;

        public string DistanceChartLoadText { get; set; }
        public string CountChartLoadText { get; set; }

        public Visibility DistanceChartVisible { get; set; }
        public Visibility CountChartVisible { get; set; }

        public bool Proceeding { get; set; }
        public int TabSelected { get; set; }
        private short _distanceTreshold;
        private short _soundTreshold;

        private readonly SoundPlayer _notifSoundPlayer;



        public short DistanceTreshold
        {
            get => _distanceTreshold;
            set
            {
                _distanceTreshold = value;
                OnPropertyChanged("DistanceTreshold");
                if (_blitzmodel != null)
                {
                    if (value == 3000)
                        _blitzmodel.DistanceTreshold = -1;
                    else
                        _blitzmodel.DistanceTreshold = value;
                }
            }
        }

        public short SoundTreshold
        {
            get => _soundTreshold;
            set
            {
                _soundTreshold = value;
                OnPropertyChanged("SoundTreshold");
                if (_blitzmodel != null) _blitzmodel.SoundTreshold = value;
            }
        }

        #endregion

        #region Konstruktor

        public BlitzViewModel(BlitzModel model)
        {
            _blitzmodel = model;
            AddressChangeCommand = new DelegateCommand(param => { ChangeAddress(); });
            KeyPressCommand = new DelegateCommand(param => { KeyPressed(); });
            ManualUploadCommand = new DelegateCommand(param => { UploadManually(); });
            TresholdCommitCommand = new DelegateCommand(x => TresholdChanged());
            AddDirectionCommand = new DelegateCommand(x => { DirectionAdded(); });
            _blitzmodel.PushToLog += AddToLog;
            _blitzmodel.OnAddressChanged += AddressChanged;
            _blitzmodel.ShowBadge += ShowBadge;
            _blitzmodel.OnDistancesGot += DistancesGot;
            _blitzmodel.OnDistancesQueryStart += DistanceQuery;
            _blitzmodel.OnProceed += ProcessChanged;
            _blitzmodel.OnSingleServerLogReceived += SingleServerLogReceived;
            _blitzmodel.OnMultipleServerLogReceived += MultipleServerLogReceived;
            _blitzmodel.OnCountQueryStart += CountQuery;
            _blitzmodel.OnCountGot += CountGot;
            _blitzmodel.OverallStatQueryResult += OverallStartQueryResult;
            _blitzmodel.onSingleStrokeReceived += SingleStrokeReceived;
            _blitzmodel.onMultipleStrokesReceived += MultipleStrokesReceived;
            _blitzmodel.onSocketRestartEvent += BlitzSocketRestart;

            _blitzmodel.OnStationsOverallStatQueryResult += StationOverallStatReceived;


            _removeOldDataTimer = new DispatcherTimer(new TimeSpan(0, 0, 30), DispatcherPriority.Normal, RemoveOldData,
                Dispatcher.CurrentDispatcher);

            Logs = new ObservableCollection<Log>(_blitzmodel.LogList);
            LiveBlitzes = new ObservableCollection<Stroke>();
            CountsList = new ObservableCollection<BlitzCountryStatItem>();
            OverallStationStats = new ObservableCollection<StationStatEntry>();
            ServerLogs = new ObservableCollection<ServerLog>();
            OverallStatistics = new ObservableCollection<OverallStatEntry>();
            _blitzmodel.ForwardGeocodingQuery(_blitzmodel.QueriedAddress);
            _blitzmodel.OverallStatUpdate(false);
            _blitzmodel.StationsOverallStatUpdate(false);
            GeocodedAddress = _blitzmodel.Address;
            ProbeAddress = _blitzmodel.QueriedAddress;
            TabSelected = 0;
            DistanceTreshold = 3000;
            SoundTreshold = _blitzmodel.SoundTreshold;

            DistanceChartLoadText = "A grafikon frissítése folyamatban...";
            DistanceChartVisible = Visibility.Hidden;

            CountChartLoadText = "A grafikon frissítése folyamatban...";
            CountChartVisible = Visibility.Hidden;

            DirectionItems = new ObservableCollection<DirectionItem>();
            DirectionItems.Add(new DirectionItem {IsChecked = true, Name = "É-ÉK", Value = 0});
            DirectionItems.Add(new DirectionItem {IsChecked = true, Name = "ÉK-K", Value = 45});
            DirectionItems.Add(new DirectionItem {IsChecked = true, Name = "K-DK", Value = 90});
            DirectionItems.Add(new DirectionItem {IsChecked = true, Name = "DK-D", Value = 135});
            DirectionItems.Add(new DirectionItem {IsChecked = true, Name = "D-DNy", Value = 180});
            DirectionItems.Add(new DirectionItem {IsChecked = true, Name = "DNy-Ny", Value = 225});
            DirectionItems.Add(new DirectionItem {IsChecked = true, Name = "Ny-ÉNy", Value = 270});
            DirectionItems.Add(new DirectionItem {IsChecked = true, Name = "ÉNy-É", Value = 315});

            OnPropertyChanged("Logs");
            OnPropertyChanged("Blitzes");
            OnPropertyChanged("GeocodedAddress");
            OnPropertyChanged("ProbeAddress");
            OnPropertyChanged("DistanceChartLoadText");
            OnPropertyChanged("DistanceChartVisible");
            OnPropertyChanged("TabSelected");
            OnPropertyChanged("DirectionItems");


            Uri notifSoundUri = new Uri(@"pack://application:,,,/Resources/cred.wav");
            StreamResourceInfo notifSoundResourceInfo = Application.GetResourceStream(notifSoundUri);
            _notifSoundPlayer = new SoundPlayer(notifSoundResourceInfo?.Stream);
        }

        private void StationOverallStatReceived(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                OverallStationStats?.Clear();
                foreach (var overallStat in _blitzmodel.OverallStationStats)
                {
                    OverallStationStats?.Add(overallStat);
                }

                OnPropertyChanged("OverallStationStats");
            });
        }

        private void RemoveOldData(object sender, EventArgs e)
        {
            var timeoutedStrokes = LiveBlitzes.Where(x => DateTime.Now.Subtract(x.Time).TotalMinutes > 10).ToList();
            var timeoutedLogs = ServerLogs.Where(x => DateTime.Now.Subtract(x.Time).TotalMinutes > 10).ToList();
            foreach (var stroke in timeoutedStrokes) LiveBlitzes.Remove(stroke);
            foreach (var timeoutedLog in timeoutedLogs) ServerLogs.Remove(timeoutedLog);
            OnPropertyChanged("LiveBlitzes");
            OnPropertyChanged("ServerLogs");
        }

        private void MultipleServerLogReceived(object sender, MultipleServerLogEventArgs e)
        {
            ServerLogs.Clear();
            foreach (var serverLog in e.ServerLogs) ServerLogs.Add(serverLog);
        }

        private void SingleServerLogReceived(object sender, SingleServerLogEventArgs e)
        {
            if (ServerLogs.Count == 10000) ServerLogs.RemoveAt(9999);
            ServerLogs.Insert(0, e.ServerLog);
        }


        private void DirectionAdded()
        {
            throw new NotImplementedException();
        }


        private void TresholdChanged()
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                _blitzmodel.DirectionsArray = DirectionItems.Where(x => x.IsChecked).Select(x => x.Value).ToArray();
                _blitzmodel.Prepare();
            });
        }

        private void BlitzSocketRestart(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                LiveBlitzes.Clear();
                OnPropertyChanged("LiveBlitzes");
            });
        }

        private void SingleStrokeReceived(object sender, SingleStrokeReceivedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                if (e.Stroke != null)
                {
                    LiveBlitzes.Insert(0, e.Stroke);
                    OnPropertyChanged("LiveBlitzes");
                    if (LiveBlitzes.Count > 5000) LiveBlitzes.RemoveAt(5000);

                    if (e.Stroke.Distance < SoundTreshold || SoundTreshold == UtilValues.SOUND_TRESHOLD)
                    {
                        _notifSoundPlayer.Play();
                    }
                }
            });
        }


        private void MultipleStrokesReceived(object sender, MultipleStrokeReceivedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                e.Strokes.OrderByDescending(x => x.Time).ToList().ForEach(stroke => { LiveBlitzes.Add(stroke); });
                OnPropertyChanged("LiveBlitzes");
            });
        }

        private void OverallStartQueryResult(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                OverallStatistics?.Clear();
                foreach (var entry in _blitzmodel.GetOverallStats)
                {
                    OverallStatistics?.Add(entry);
                }

                OnPropertyChanged("OverallStatistics");
            });
        }

        #endregion
    }
}