using BlitzInfo.Utils;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Timers;
using System.Windows;
using BlitzInfo.Model.Entities;
using Quobject.SocketIoClientDotNet.Client;
using Socket = Quobject.SocketIoClientDotNet.Client.Socket;

namespace BlitzInfo.Model
{

    #region Adattípus-osztályok

    public class Process
    {
        public enum ProcessKind { GEOCODING_QUERY, BLITZ_QUERY, UPLOAD_LOGS, DISTANCE_PROCESS, COUNT_PROCESS, LOG_DOWNLOAD, OVERALL_COUNT, STATION_STAT }
        public int ProcessID { get; set; }
        public ProcessKind kind { get; set; }

        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }
            Process p = obj as Process;
            if ((System.Object)p == null)
            {
                return false;
            }
            return (ProcessID == p.ProcessID) && (kind == p.kind);
        }
        public override int GetHashCode()
        {
            return ProcessID ^ (int)kind;
        }
    }
    public class Log
    {
        [JsonProperty("debug_clientid")]
        public string clientId { get; set; }
        [JsonProperty("debug_timestamp")]
        public DateTime timestamp { get; set; }
        [JsonProperty("debug_messagekind")]
        public string kind { get; set; }
        [JsonProperty("debug_messageheader")]
        public string header { get; set; }
        [JsonProperty("debug_message")]
        public string message { get; set; }
        [JsonProperty("debug_processors")]
        public string processor { get; set; }
    }
   

    public class BlitzDistance
    {
        public DateTime time { get; set; }
        public double distance { get; set; }
        public string dtype { get; set; }
    }

    #endregion

    public class BlitzModel
    {

        #region Statikus adattagok

        private static String STROKE_INIT = "strokesInit";
        private static String LOGS = "log";
        private static String LOGS_INIT = "logInit";
        private static String STROKES = "strokes";
        private static String ALERTS = "alerts";

        private static Newtonsoft.Json.JsonSerializerSettings JSON_SERIALIZER = new JsonSerializerSettings
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Local
        };

        #endregion

        #region Privát adattagok

        private Timer _periodic;
        private Timer _logsender_timer;
        private Timer _chart_timer;
        private Timer _logdown_timer;
        private Timer _countTimer;

        private List<Stroke> _lastBlitzes;
        private List<Log> _log;
        private List<BlitzDistance> _distances;
        private List<StrokeTenmin> _counts;
        private List<OverallStatEntry> _overallstats;
        private List<StationStatEntry> _overallStationStats;
        private HashSet<Process> _processes;
        private Queue<Process.ProcessKind> _processqueue;

        private Random _randomgen;

        public event EventHandler OnAddressChanged;
        public event EventHandler<BlitzEventArgs> PushToLog;
        public event EventHandler<BadgeEventArgs> ShowBadge;
        public event EventHandler OnDistancesGot;
        public event EventHandler OnDistancesQueryStart;
        public event EventHandler OnCountGot;
        public event EventHandler OnCountQueryStart;
        public event EventHandler OverallStatQueryStart;
        public event EventHandler OverallStatQueryResult;
        public event EventHandler OnStationsOverallStatQueryStart;
        public event EventHandler OnStationsOverallStatQueryResult;
        public event EventHandler<ProcessEventArgs> OnProceed;
        public event EventHandler<NoSettlemEventArgs> onAdressNotExisting;
        public event EventHandler<SingleStrokeReceivedEventArgs> onSingleStrokeReceived;
        public event EventHandler<MultipleStrokeReceivedEventArgs> onMultipleStrokesReceived;
        public event EventHandler<SingleServerLogEventArgs> OnSingleServerLogReceived;
        public event EventHandler<MultipleServerLogEventArgs> OnMultipleServerLogReceived;
        public event EventHandler onSocketRestartEvent;

        private string _ram;
        private string _json;
        private string _jsonLoc;
        private LatLon _userLatLon;
        private string _address;
        private RegistryKey _key;
        private string _guid;
        private string _queriedAddress;
        private string _countrycode;
        private DateTime _lastgetlogs;
        private Int16 _distanceTreshold;
        private Int16 _distanceTresholdNear;
        private Int16 _soundTreshold;
        private string _liveDataUrl;
        private string _restUrl;

        private enum ProcessForce { FORCE_STOP, FORCE_START, NO_FORCE }

        private Socket _socket;

        #endregion

        #region Konstruktor

        public BlitzModel()
        {
            registry();

            List<String> Processors = new List<string>();
            ManagementClass mgt = new ManagementClass("Win32_Processor");
            ManagementObjectCollection procs = mgt.GetInstances();
            foreach (ManagementObject item in procs)
            {
                Processors.Add(item.Properties["Name"].Value.ToString());
            }
            _randomgen = new Random();
            _lastgetlogs = DateTime.Now;

            _lastBlitzes = new List<Stroke>();
            _log = new List<Log>();
            _distances = new List<BlitzDistance>();
            _counts = new List<StrokeTenmin>();
            _processes = new HashSet<Process>();
            _processqueue = new Queue<Process.ProcessKind>();
            _overallstats = new List<OverallStatEntry>();
            _userLatLon = new LatLon();

            var commandLineArgs = Environment.GetCommandLineArgs();
            var isLocal = commandLineArgs.Any(x => x.ToLowerInvariant() == "local");
            var isNonHttps = commandLineArgs.Any(x => x.ToLowerInvariant() == "non-https");

            if (isLocal)
            {
                _liveDataUrl = "http://localhost:5001";
                _restUrl = "http://localhost:5000";
            }
            else if (isNonHttps)
            {
                _liveDataUrl = "http://api.blitzinfo.ehog.hu";
                _restUrl = "http://api.blitzinfo.ehog.hu/rest";
            }
            else
            {
                _liveDataUrl = "https://api.blitzinfo.ehog.hu";
                _restUrl = "https://api.blitzinfo.ehog.hu/rest";
            }
      
            _periodic = new System.Timers.Timer();
            _periodic.Interval = (1000 * 120) - 1;
            _periodic.Elapsed += update_periodic;
            _periodic.Start();

            _logsender_timer = new System.Timers.Timer();
            _logsender_timer.Interval = (1000 * 300) + 3;
            _logsender_timer.Elapsed += send_logs;
            _logsender_timer.Start();

            _logdown_timer = new System.Timers.Timer();
            _logdown_timer.Interval = 1000 * 30;
            _logdown_timer.Elapsed += LogDownTick;
            _logdown_timer.Start();

            _countTimer = new System.Timers.Timer();
            _countTimer.Interval = (1000 * 600) - 5;
            _countTimer.Elapsed += countTick;
            _countTimer.Start();


            OnAddressChanged = delegate { };
            PushToLog = delegate { };
            ShowBadge = delegate { };
            OnDistancesGot = delegate { };
            OnDistancesQueryStart = delegate { };
            OnCountGot = delegate { };
            OnCountQueryStart = delegate { };
            OnProceed = delegate { };
            OverallStatQueryStart = delegate { };
            OverallStatQueryResult = delegate { };
            OnStationsOverallStatQueryStart = delegate { };
            OnStationsOverallStatQueryResult = delegate { };

            OnProceed += NextInQueue;

            PushToLog += addToLog;
            countProcess();
            PushToLog(this, new BlitzEventArgs("Program elindítva", "Program", BlitzEventArgs.EventMood.COMMAND));

        }


        #endregion

        #region Processz-kezelés

        private void NextInQueue(object sender, ProcessEventArgs e)
        {
            if (e.processevent == ProcessEventArgs.ProcessEvent.PROCESS_FINISHED)
            {
                if (_processqueue.Count != 0)
                {
                    Process.ProcessKind elem = _processqueue.Dequeue();
                    if (elem == Process.ProcessKind.COUNT_PROCESS)
                    {
                        countProcess(true);
                    }
                    else if (elem == Process.ProcessKind.OVERALL_COUNT)
                    {
                        OverallStatUpdate(true);
                    }
                    else if (elem == Process.ProcessKind.UPLOAD_LOGS)
                    {
                    }
                }
            }
        }

        private void addToQueue(Process.ProcessKind process)
        {
            if (_processqueue.Count < 5)
            {
                _processqueue.Enqueue(process);
            }
            else
            {
                PushToLog(this, new BlitzEventArgs("A várakozási sor tele van, a folyamat (" + processKindToText(process) + ") nem indítható.", "Program", BlitzEventArgs.EventMood.ERR));
            }
        }


        private void ProcessManagement(Process process, ProcessForce pforce = ProcessForce.NO_FORCE)
        {
            if (pforce == ProcessForce.FORCE_START)
            {
                _processes.Add(process);
                OnProceed(this, new ProcessEventArgs { count = _processes.Count + _processqueue.Count, processevent = ProcessEventArgs.ProcessEvent.PROCESS_STARTED, processkind = process.kind });
            }
            else if (pforce == ProcessForce.FORCE_STOP)
            {
                _processes.Remove(process);
                OnProceed(this, new ProcessEventArgs { count = _processes.Count + _processqueue.Count, processevent = ProcessEventArgs.ProcessEvent.PROCESS_FINISHED, processkind = process.kind });
            }
            else
            {
                if (_processes.Contains(process))
                {
                    _processes.Remove(process);
                    OnProceed(this, new ProcessEventArgs { count = _processes.Count + _processqueue.Count, processevent = ProcessEventArgs.ProcessEvent.PROCESS_FINISHED, processkind = process.kind });
                }
                else
                {
                    _processes.Add(process);
                    OnProceed(this, new ProcessEventArgs { count = _processes.Count + _processqueue.Count, processevent = ProcessEventArgs.ProcessEvent.PROCESS_STARTED, processkind = process.kind });
                }
            }
        }

        public bool HasKindOfProcess(Process.ProcessKind kind)
        {
            foreach (Process process in _processes)
            {
                if (process.kind == kind)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Rendszerfüggvények

        public async void Disconnect()
        {
            _socket.Disconnect();
        }

        public void Prepare()
        {
            _socket?.Disconnect();
            onSocketRestartEvent?.Invoke(this, EventArgs.Empty);
            try
            {
                InitObject initObject = new InitObject
                {
                    LatLon = new[] { _userLatLon.Longitude, _userLatLon.Latitude },
                    DistanceTreshold = DistanceTreshold,
                    DeviceType = ".NET CLIENT",
                    Directions = DirectionsArray ?? new int[] { },
                };

                var json = JObject.FromObject(initObject);
                _socket = IO.Socket(_liveDataUrl);
                _socket.On(Socket.EVENT_CONNECT, () =>
                {
                    _socket.Emit(STROKE_INIT, json);
                    _socket.Emit(LOGS_INIT, "");
                    ShowBadge?.Invoke(this,new BadgeEventArgs(BadgeEventArgs.BadgeType.CONNECTED));
                     PushToLog(this, new BlitzEventArgs("Kapcsolódva Socket.IO-hoz", "Socket.IO-hozzáférés", BlitzEventArgs.EventMood.OK));
                });

                _socket.On(Socket.EVENT_ERROR, OnSocketIoError);
                _socket.On(Socket.EVENT_CONNECT_ERROR, OnSocketIoError);
                _socket.On(Socket.EVENT_CONNECT_TIMEOUT, OnSocketIoError);
                _socket.On(STROKES, OnReceiveSingleStroke);
                _socket.On(STROKE_INIT, OnReceiveMultipleStrokes);
                _socket.On(LOGS, OnReceiveLogs);
                _socket.On(LOGS_INIT, OnReceiveLogsInit);
                _socket.On(ALERTS, OnReceiveAlerts);

            }

            catch (Exception e)
            {
                PushToLog?.Invoke(this, new BlitzEventArgs("Kapcsolódás Socket.IO-hoz sikertelen", "Socket.IO-hozzáférés", BlitzEventArgs.EventMood.ERR));
            }

        }

        private void OnReceiveAlerts(object obj)
        {
            MetHuAlerts log = JsonConvert.DeserializeObject<MetHuAlerts>(obj.ToString(), JSON_SERIALIZER);
        }

        private void OnSocketIoError(object obj)
        {
            ShowBadge?.Invoke(this, new BadgeEventArgs(BadgeEventArgs.BadgeType.CONNECTION_ERROR));
            PushToLog?.Invoke(this, new BlitzEventArgs("Socket.IO kapcsolati hiba", "Socket.IO-hozzáférés", BlitzEventArgs.EventMood.ERR));
        }

        private void OnReceiveSingleStroke(object strokeData)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate ()
            {
                Stroke stroke = Stroke.GetBlitzFromJsonArray(strokeData);
                stroke.Bearing = LatLon.Bearing(_userLatLon, stroke.LatLon);
                stroke.Distance = LatLon.Distance(stroke.LatLon, _userLatLon);
                onSingleStrokeReceived?.Invoke(this, new SingleStrokeReceivedEventArgs { Stroke = stroke });
            });
        }

        private void OnReceiveLogs(object logData)
        {
            string logString = logData.ToString();
            ServerLog log = JsonConvert.DeserializeObject<ServerLog>(logData.ToString(),JSON_SERIALIZER);

            Application.Current.Dispatcher.Invoke((Action)delegate ()
            {
                OnSingleServerLogReceived?.Invoke(this, new SingleServerLogEventArgs { ServerLog = log });
            });
        }

        private void OnReceiveLogsInit(object logData)
        {
            string logString = logData.ToString();
            List<ServerLog> logs = JsonConvert.DeserializeObject<List<ServerLog>>(logData.ToString(), JSON_SERIALIZER);
            Application.Current.Dispatcher.Invoke((Action)delegate ()
            {
                OnMultipleServerLogReceived?.Invoke(this, new MultipleServerLogEventArgs { ServerLogs = logs });
            });
        }
        private void OnReceiveMultipleStrokes(object strokesData)
        {
            List<Stroke> blitzes = Stroke.GetMultipleBlitzesFromJsonArray(strokesData);
            Application.Current.Dispatcher.Invoke((Action)delegate ()
            {
                blitzes.ForEach(stroke =>
                {
                    stroke.Bearing = LatLon.Bearing(_userLatLon, stroke.LatLon);
                    stroke.Distance = LatLon.Distance(stroke.LatLon, _userLatLon);
                });
                onMultipleStrokesReceived?.Invoke(this, new MultipleStrokeReceivedEventArgs { Strokes = blitzes });
            });
        }


        private void registry()
        {
            if (Microsoft.Win32.Registry.CurrentUser.OpenSubKey("BlitzInfo", true) == null)
            {
                _key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("BlitzInfo");
                _guid = Convert.ToString(System.Guid.NewGuid());
                _key.SetValue("client_id", _guid);
                _key.SetValue("address", "Budapest");
                _key.SetValue("soundTreshold", _soundTreshold);
                _queriedAddress = "Budapest";

            }
            else
            {
                _key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("BlitzInfo", true);
                _guid = (string)_key.GetValue("client_id");
                try
                {
                    _soundTreshold = Convert.ToInt16(_key.GetValue("soundTreshold"));
                }
                catch (Exception)
                {
                    _soundTreshold = UtilValues.SOUND_TRESHOLD;
                }


                if (_key.GetValue("address") == null)
                {
                    _key.SetValue("address", "Budapest");
                    _queriedAddress = "Budapest";
                }
                else
                {
                    _queriedAddress = Convert.ToString(_key.GetValue("address"));
                }
            }
            _key.Close();
        }
        private void addressRegistry(string address)
        {
            _queriedAddress = address;
            if (Microsoft.Win32.Registry.CurrentUser.OpenSubKey("BlitzInfo", true) == null)
            {
                _key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("BlitzInfo");
                _guid = Convert.ToString(System.Guid.NewGuid());
                _key.SetValue("client_id", _guid);
                _key.SetValue("address", address);
            }
            else
            {
                _key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("BlitzInfo", true);
                _key.SetValue("address", address);
            }
            _key.Close();
        }

        #endregion

        #region Esemény-lekezelések

        private void LogDownTick(object sender, ElapsedEventArgs e)
        {
            //GetServerLog();
        }


        private void countTick(object sender, ElapsedEventArgs e)
        {
            countProcess();
        }

        private async void send_logs(object sender, ElapsedEventArgs e)
        {
        }

        private void addToLog(object sender, BlitzEventArgs e)
        {
            Log log = new Log { clientId = _guid, timestamp = e.timestamp, message = e.msg, header = e.msgheader, kind = moodtoString(e.mood) };
            if (e.saved == true)
            {
                _log.Add(log);
            }
        }

        public async void ManualLog(BlitzEventArgs new_event)
        {
            PushToLog(this, new_event);
        }

        private async void update_periodic(object sender, ElapsedEventArgs e)
        {
            OverallStatUpdate();
            StationsOverallStatUpdate();
        }

        #endregion

        #region Adatlekérések és adatküldés

        public async void ForwardGeocodingQuery(string searchString)
        {
            if (!HasKindOfProcess(Process.ProcessKind.GEOCODING_QUERY))
            {
                WebClient geocodingWebClient = new WebClient();
                geocodingWebClient.Encoding = Encoding.UTF8;
                geocodingWebClient.Headers.Add("user-agent", "ehog BlitzInfo (.NET) - ehog@ehog.hu");
                geocodingWebClient.Headers.Add("accept-language", "hu-HU");

                int processId = _randomgen.Next();
                ProcessManagement(new Process { ProcessID = processId, kind = Process.ProcessKind.GEOCODING_QUERY });
                try
                {
                    PushToLog(this, new BlitzEventArgs("Lekérés... (" + searchString + ')', "Google Maps címlekérés", BlitzEventArgs.EventMood.AWAIT));
                    geocodingWebClient.CancelAsync();
                    string geocodingAddress = String.Format("https://maps.googleapis.com/maps/api/geocode/json?address={0}",
                            WebUtility.UrlEncode(searchString));
                    string geocodingJson = await geocodingWebClient.DownloadStringTaskAsync(geocodingAddress);
                    var geocodingJsonObject = JObject.Parse(geocodingJson);

                    if (geocodingJsonObject["results"].Count() != 0)
                    {
                        var firstResult = geocodingJsonObject["results"].First;
                        _address = (string)firstResult["formatted_address"];
                        addressRegistry(searchString);
                        _userLatLon = new LatLon
                        {
                            Latitude = (float) firstResult["geometry"]["location"]["lat"],
                            Longitude = (float) firstResult["geometry"]["location"]["lng"],
                        };

                        foreach (var addressComponent in firstResult["address_components"])
                        {
                            Boolean isCountry = addressComponent["types"].Children().ToList().FirstOrDefault(x => (string) x == "country") != null;
                            if (isCountry)
                            {
                                _countrycode = ((string)addressComponent["short_name"]).ToLower();
                            }
                        }
                        if (OnAddressChanged != null)
                        {
                            OnAddressChanged(this, EventArgs.Empty);
                            PushToLog(this, new BlitzEventArgs("Új cím (" + _address + ')', "Google Maps címlekérés", BlitzEventArgs.EventMood.OK));
                        }
                        Prepare();


                    }
                    else
                    {
                        PushToLog(this, new BlitzEventArgs("A hely nem található. (" + searchString + ')', "Google Maps címlekérés", BlitzEventArgs.EventMood.ERR));
                        onAdressNotExisting?.Invoke(this, new NoSettlemEventArgs { ProbedAddress = searchString });
                    }
                }
                catch (NotSupportedException exc)
                {
                    PushToLog(this, new BlitzEventArgs("Kapcsolati hiba. (" + searchString + ')', "Google Maps címlekérés", BlitzEventArgs.EventMood.ERR));
                }
                catch (WebException exc)
                {
                    PushToLog(this, new BlitzEventArgs("A kapcsolat megszakadt vagy nincs. (" + searchString + ')', "Google Maps címlekérés", BlitzEventArgs.EventMood.ERR));
                }
                catch (Exception exc)
                {
                    PushToLog(this, new BlitzEventArgs("Ismeretlen hiba: " + exc.ToString(), "Ismeretlen hiba", BlitzEventArgs.EventMood.ERR));
                }
                finally
                {
                    ProcessManagement(new Process { ProcessID = processId, kind = Process.ProcessKind.GEOCODING_QUERY });
                }
            }
            else
            {
                PushToLog(this, new BlitzEventArgs("A címlekérés nem lehetséges, mert már folyamatban van.", "Google Maps címlekérés", BlitzEventArgs.EventMood.ERR));
            }
        }

        public async void countProcess(bool isInQueue = false)
        {
            if (!HasKindOfProcess(Process.ProcessKind.COUNT_PROCESS))
            {
                int processId = _randomgen.Next();
                ProcessManagement(new Process { ProcessID = processId, kind = Process.ProcessKind.COUNT_PROCESS });
                OnCountQueryStart(this, EventArgs.Empty);
                bool ok = true;
                string json;
                try
                {
                    WebClient _client_count = new WebClient(); 
                    _client_count.Headers.Add("user-agent", "ehog BlitzInfo (.NET) - ehog@ehog.hu");
                    PushToLog(this, new BlitzEventArgs("Adatlekérés elindult", "Villám darabszám-grafikon", BlitzEventArgs.EventMood.AWAIT));
                    json = await _client_count.DownloadStringTaskAsync($"{_restUrl}/stats/tenmin/48");
                    _counts = StrokeTenmin.GetMultipleTenminDataFromJsonArray(json);
                    _counts.Reverse();

                }
                catch (NotSupportedException exc)
                {
                    PushToLog(this, new BlitzEventArgs("Kapcsolati hiba.", "Villám darabszám-grafikon", BlitzEventArgs.EventMood.ERR));
                    ok = false;
                }

                catch (WebException exc)
                {
                    PushToLog(this, new BlitzEventArgs("A kapcsolat megszakadt vagy nincs.", "Villám darabszám-grafikon", BlitzEventArgs.EventMood.ERR));
                    ok = false;
                }
                finally
                {
                    if (_counts.Count == 0)
                    {
                        PushToLog(this, new BlitzEventArgs("A JSON nem tartalmaz elemeket. Talán frissül? Válaszd a villámok darabszáma fület, és nyomj F5-t, amíg nincs adat!", "Villám darabszám-grafikon", BlitzEventArgs.EventMood.ERR));
                        ok = false;
                    }
                    if (ok)
                    {
                        OnCountGot(this, EventArgs.Empty);
                        PushToLog(this, new BlitzEventArgs("Adatlekérés befejezve.", "Villám darabszám-grafikon", BlitzEventArgs.EventMood.OK));
                    }
                    ProcessManagement(new Process { ProcessID = processId, kind = Process.ProcessKind.COUNT_PROCESS });
                }
            }
            else
            {
                if (!isInQueue)
                {
                    PushToLog(this, new BlitzEventArgs("A darabszám-grafikon adatainak lekérése már folyamatban, várakozási sorhoz adás...", "Villám darabszám-grafikon", BlitzEventArgs.EventMood.ERR));
                }
                addToQueue(Process.ProcessKind.COUNT_PROCESS);
            }
        }


        public async void StationsOverallStatUpdate(bool isInQueue = false)
        {
            if (!HasKindOfProcess(Process.ProcessKind.STATION_STAT))
            {
                int processId = _randomgen.Next();
                ProcessManagement(new Process { ProcessID = processId, kind = Process.ProcessKind.STATION_STAT });
                OnStationsOverallStatQueryStart(this, EventArgs.Empty);
                bool ok = true;
                string json;
                try
                {
                    WebClient overallStatsWebClient = new WebClient();
                    overallStatsWebClient.Encoding = Encoding.UTF8;
                    overallStatsWebClient.Headers.Add("user-agent", "ehog BlitzInfo (.NET) - ehog@ehog.hu");
                    PushToLog(this, new BlitzEventArgs("Adatlekérés elindult", "Összes állomásadat", BlitzEventArgs.EventMood.AWAIT));
                    json = await overallStatsWebClient.DownloadStringTaskAsync($"{_restUrl}/stations");
                    _overallStationStats = StationStatEntry.GetEntriesFromJson(json);

                }
                catch (NotSupportedException exc)
                {
                    PushToLog(this, new BlitzEventArgs("Kapcsolati hiba.", "Összes állomásadat", BlitzEventArgs.EventMood.ERR));
                    ok = false;
                }

                catch (WebException exc)
                {
                    PushToLog(this, new BlitzEventArgs("A kapcsolat megszakadt vagy nincs.", "Összes állomásadat", BlitzEventArgs.EventMood.ERR));
                    ok = false;
                }
                finally
                {
                    if (_overallstats.Count == 0)
                    {
                        PushToLog(this, new BlitzEventArgs("Nincsenek adatok", "Összes állomásadat", BlitzEventArgs.EventMood.ERR));
                        ok = false;
                    }
                    if (ok)
                    {
                        OnStationsOverallStatQueryResult(this, EventArgs.Empty);
                        PushToLog(this, new BlitzEventArgs("Adatlekérés befejezve.", "Összes állomásadat", BlitzEventArgs.EventMood.OK));
                    }
                    ProcessManagement(new Process { ProcessID = processId, kind = Process.ProcessKind.STATION_STAT });
                }

            }
        }

        public async void OverallStatUpdate(bool isInQueue = false)
        {
            if (!HasKindOfProcess(Process.ProcessKind.OVERALL_COUNT))
            {
                int processId = _randomgen.Next();
                ProcessManagement(new Process { ProcessID = processId, kind = Process.ProcessKind.OVERALL_COUNT });
                OverallStatQueryStart(this, EventArgs.Empty);
                bool ok = true;
                string json;
                try
                {
                    WebClient overallStatsWebClient = new WebClient();
                    overallStatsWebClient.Headers.Add("user-agent", "ehog BlitzInfo (.NET) - ehog@ehog.hu");
                    PushToLog(this, new BlitzEventArgs("Adatlekérés elindult", "Hosszútávú statisztika", BlitzEventArgs.EventMood.AWAIT));
                    json = await overallStatsWebClient.DownloadStringTaskAsync($"{_restUrl}/stats/overall");
                    _overallstats = OverallStatEntry.GetMultipleStatEntryFromJsonArray(json);

                }
                catch (NotSupportedException exc)
                {
                    PushToLog(this, new BlitzEventArgs("Kapcsolati hiba.", "Hosszútávú statisztika", BlitzEventArgs.EventMood.ERR));
                    ok = false;
                }

                catch (WebException exc)
                {
                    PushToLog(this, new BlitzEventArgs("A kapcsolat megszakadt vagy nincs.", "Hosszútávú statisztika", BlitzEventArgs.EventMood.ERR));
                    ok = false;
                }
                finally
                {
                    if (_overallstats.Count == 0)
                    {
                        PushToLog(this, new BlitzEventArgs("A hosszútávú statisztika üres.", "Hosszútávú statisztika", BlitzEventArgs.EventMood.ERR));
                        ok = false;
                    }
                    if (ok)
                    {
                        OverallStatQueryResult(this, EventArgs.Empty);
                        PushToLog(this, new BlitzEventArgs("Adatlekérés befejezve.", "Hosszútávú statisztika", BlitzEventArgs.EventMood.OK));
                    }
                    ProcessManagement(new Process { ProcessID = processId, kind = Process.ProcessKind.OVERALL_COUNT });
                }
            }
            else
            {
                if (!isInQueue)
                {
                    PushToLog(this, new BlitzEventArgs("A hosszútávú statisztika letöltése már folyamatban, várakozási sorhoz adás...", "Hosszútávú statisztika", BlitzEventArgs.EventMood.ERR));
                }
                addToQueue(Process.ProcessKind.OVERALL_COUNT);
            }
        }



        #endregion

        #region Segédfüggvények

        public string moodtoString(BlitzEventArgs.EventMood mood)
        {
            if (mood == BlitzEventArgs.EventMood.AWAIT) return "AWAIT";
            else if (mood == BlitzEventArgs.EventMood.ERR) return "ERR";
            else if (mood == BlitzEventArgs.EventMood.OK) return "OK";
            else if (mood == BlitzEventArgs.EventMood.COMMAND) return "COMM";
            else if (mood == BlitzEventArgs.EventMood.INFORMATION) return "INF";
            else if (mood == BlitzEventArgs.EventMood.SERVER) return "SERV";
            else return "UNKNOWN";
        }

        private string processKindToText(Process.ProcessKind kind)
        {
            if (kind == Process.ProcessKind.BLITZ_QUERY)
            {
                return "villámok lekérése";
            }
            else if (kind == Process.ProcessKind.COUNT_PROCESS)
            {
                return "darabszám-grafikon adatlekérés";
            }
            else if (kind == Process.ProcessKind.DISTANCE_PROCESS)
            {
                return "távolság-grafikon adatlekérés";
            }
            else if (kind == Process.ProcessKind.GEOCODING_QUERY)
            {
                return "Nominatim címfeloldás";
            }
            else if (kind == Process.ProcessKind.UPLOAD_LOGS)
            {
                return "log feltöltés";
            }
            else
            {
                return "ismeretlen";
            }
        }


        #endregion

        #region Tulajdonságok

        public string Address
        {
            get
            {
                return _address;
            }
        }

        public string Country
        {
            get
            {
                return _countrycode.ToUpper();
            }
        }

        public string QueriedAddress
        {
            get
            {
                return _queriedAddress;
            }
        }

        public List<Stroke> GetBlitzes
        {
            get
            {
                return _lastBlitzes;
            }
        }

        public List<BlitzDistance> GetDistances
        {
            get
            {
                return _distances;
            }
        }

        public List<OverallStatEntry> GetOverallStats
        {
            get
            {
                return _overallstats;
            }
        }

        public List<StrokeTenmin> GetCount
        {
            get
            {
                return _counts;
            }
        }

        public List<StationStatEntry> OverallStationStats
        {
            get
            {
                return _overallStationStats;
            }
        }

        public List<Log> LogList
        {
            get
            {
                return _log;
            }
        }

        public Int16 DistanceTreshold
        {
            get
            {
                return _distanceTreshold;

            }
            set { _distanceTreshold = value; }
        }

        public Int16 DistanceTresholdNear
        {
            get
            {
                return _distanceTresholdNear;

            }
            set { _distanceTresholdNear = value; }
        }



        public Int16 SoundTreshold
        {
            get
            {
                return _soundTreshold;

            }
            set
            {
                _soundTreshold = value;
                RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("BlitzInfo", true);
                key.SetValue("soundTreshold", _soundTreshold);
                key.Close();
            }
        }

        public int[] DirectionsArray { get; set; }

        #endregion
    }
}
