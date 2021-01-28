using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Timers;
using System.Windows;
using BlitzInfo.Model.Entities;
using BlitzInfo.Utils;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quobject.SocketIoClientDotNet.Client;

namespace BlitzInfo.Model
{
    #region Adattípus-osztályok

    public class Process
    {
        public enum ProcessKind
        {
            GEOCODING_QUERY,
            BLITZ_QUERY,
            UPLOAD_LOGS,
            DISTANCE_PROCESS,
            COUNT_PROCESS,
            LOG_DOWNLOAD,
            OVERALL_COUNT,
            STATION_STAT
        }

        public int ProcessID { get; set; }
        public ProcessKind kind { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var p = obj as Process;
            if (p == null) return false;
            return ProcessID == p.ProcessID && kind == p.kind;
        }

        public override int GetHashCode()
        {
            return ProcessID ^ (int) kind;
        }
    }

    public class Log
    {
        [JsonProperty("debug_clientid")] public string clientId { get; set; }

        [JsonProperty("debug_timestamp")] public DateTime timestamp { get; set; }

        [JsonProperty("debug_messagekind")] public string kind { get; set; }

        [JsonProperty("debug_messageheader")] public string header { get; set; }

        [JsonProperty("debug_message")] public string message { get; set; }

        [JsonProperty("debug_processors")] public string processor { get; set; }
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
        private string _envFlag => Environment.GetCommandLineArgs().Length > 1
            ? Environment.GetCommandLineArgs()[1].ToUpperInvariant()
            : "DEFAULT";

        private string _liveDataUrl
        {
            get
            {
                if (_envFlag == "DEFAULT")
                {
                    return "http://api.blitzinfo.ehog.hu";
                }

                if (_envFlag == "LOCAL")
                {
                    return "http://localhost:5001";
                }
                if (_envFlag == "VPN")
                {
                    return "http://blitzinfo-api.ehog";
                }
                if (_envFlag == "VPN-NOPROXY")
                {
                    return "http://blitzinfo-api.ehog:5001";
                }

                return "http://api.blitzinfo.ehog.hu";
            }
        }

        private string _restUrl
        {
            get
            {
                if (_envFlag == "DEFAULT")
                {
                    return "https://api.blitzinfo.ehog.hu/rest";
                }

                if (_envFlag == "LOCAL")
                {
                    return "http://localhost:5000";
                }
                if (_envFlag == "VPN")
                {
                    return "http://blitzinfo-api.ehog/rest";
                }
                if (_envFlag == "VPN-NOPROXY")
                {
                    return "http://blitzinfo-api.ehog:5000";
                }

                return "https://api.blitzinfo.ehog.hu/rest";
            }
        }


        #region Konstruktor

        public BlitzModel()
        {
            registry();

            var Processors = new List<string>();
            var mgt = new ManagementClass("Win32_Processor");
            var procs = mgt.GetInstances();
            foreach (var o in procs)
            {
                var item = (ManagementObject) o;
                Processors.Add(item.Properties["Name"].Value.ToString());
            }

            _randomgen = new Random();
            _lastgetlogs = DateTime.Now;

            GetBlitzes = new List<Stroke>();
            LogList = new List<Log>();
            GetDistances = new List<BlitzDistance>();
            GetCount = new List<StrokeTenmin>();
            _processes = new HashSet<Process>();
            _processqueue = new Queue<Process.ProcessKind>();
            GetOverallStats = new List<OverallStatEntry>();
            _userLatLon = new LatLon();

            _periodic = new Timer();
            _periodic.Interval = 1000 * 120 - 1;
            _periodic.Elapsed += update_periodic;
            _periodic.Start();

            _logsender_timer = new Timer();
            _logsender_timer.Interval = 1000 * 300 + 3;
            _logsender_timer.Elapsed += send_logs;
            _logsender_timer.Start();

            _logdown_timer = new Timer();
            _logdown_timer.Interval = 1000 * 30;
            _logdown_timer.Elapsed += LogDownTick;
            _logdown_timer.Start();

            _countTimer = new Timer();
            _countTimer.Interval = 1000 * 600 - 5;
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

        #region Statikus adattagok

        private static readonly string STROKE_INIT = "strokesInit";
        private static readonly string LOGS = "log";
        private static readonly string LOGS_INIT = "logInit";
        private static readonly string STROKES = "strokes";
        private static readonly string ALERTS = "alerts";
        private static string GOOGLE_MAPS_API_KEY = "AIzaSyBG4pD7EOyUeKV3o-GYwNydrHH6EvFw8oE";

        private static readonly JsonSerializerSettings JSON_SERIALIZER = new JsonSerializerSettings
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Local
        };

        #endregion

        #region Privát adattagok

        private readonly Timer _periodic;
        private readonly Timer _logsender_timer;
        private Timer _chart_timer;
        private readonly Timer _logdown_timer;
        private readonly Timer _countTimer;
        private readonly HashSet<Process> _processes;
        private readonly Queue<Process.ProcessKind> _processqueue;

        private readonly Random _randomgen;

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
        private RegistryKey _key;
        private string _guid;
        private string _countrycode;
        private DateTime _lastgetlogs;
        private short _soundTreshold;

        private enum ProcessForce
        {
            FORCE_STOP,
            FORCE_START,
            NO_FORCE
        }

        private Socket _socket;

        #endregion

        #region Processz-kezelés

        private void NextInQueue(object sender, ProcessEventArgs e)
        {
            if (e.processevent == ProcessEventArgs.ProcessEvent.PROCESS_FINISHED)
                if (_processqueue.Count != 0)
                {
                    var elem = _processqueue.Dequeue();
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

        private void addToQueue(Process.ProcessKind process)
        {
            if (_processqueue.Count < 5)
                _processqueue.Enqueue(process);
            else
                PushToLog(this,
                    new BlitzEventArgs(
                        "A várakozási sor tele van, a folyamat (" + processKindToText(process) + ") nem indítható.",
                        "Program", BlitzEventArgs.EventMood.ERR));
        }


        private void ProcessManagement(Process process, ProcessForce pforce = ProcessForce.NO_FORCE)
        {
            if (pforce == ProcessForce.FORCE_START)
            {
                _processes.Add(process);
                OnProceed(this,
                    new ProcessEventArgs
                    {
                        count = _processes.Count + _processqueue.Count,
                        processevent = ProcessEventArgs.ProcessEvent.PROCESS_STARTED, processkind = process.kind
                    });
            }
            else if (pforce == ProcessForce.FORCE_STOP)
            {
                _processes.Remove(process);
                OnProceed(this,
                    new ProcessEventArgs
                    {
                        count = _processes.Count + _processqueue.Count,
                        processevent = ProcessEventArgs.ProcessEvent.PROCESS_FINISHED, processkind = process.kind
                    });
            }
            else
            {
                if (_processes.Contains(process))
                {
                    _processes.Remove(process);
                    OnProceed(this,
                        new ProcessEventArgs
                        {
                            count = _processes.Count + _processqueue.Count,
                            processevent = ProcessEventArgs.ProcessEvent.PROCESS_FINISHED, processkind = process.kind
                        });
                }
                else
                {
                    _processes.Add(process);
                    OnProceed(this,
                        new ProcessEventArgs
                        {
                            count = _processes.Count + _processqueue.Count,
                            processevent = ProcessEventArgs.ProcessEvent.PROCESS_STARTED, processkind = process.kind
                        });
                }
            }
        }

        public bool HasKindOfProcess(Process.ProcessKind kind)
        {
            foreach (var process in _processes)
                if (process.kind == kind)
                    return true;
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
                var initObject = new InitObject
                {
                    LatLon = new[] {_userLatLon.Longitude, _userLatLon.Latitude},
                    DistanceTreshold = DistanceTreshold,
                    DeviceType = ".NET CLIENT",
                    Directions = DirectionsArray ?? new int[] { }
                };

                var json = JObject.FromObject(initObject);
                _socket = IO.Socket(_liveDataUrl);
                _socket.On(Socket.EVENT_CONNECT, () =>
                {
                    _socket.Emit(STROKE_INIT, json);
                    _socket.Emit(LOGS_INIT, "");
                    ShowBadge?.Invoke(this, new BadgeEventArgs(BadgeEventArgs.BadgeType.CONNECTED));
                    PushToLog(this,
                        new BlitzEventArgs("Kapcsolódva Socket.IO-hoz", "Socket.IO-hozzáférés",
                            BlitzEventArgs.EventMood.OK));
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
                PushToLog?.Invoke(this,
                    new BlitzEventArgs("Kapcsolódás Socket.IO-hoz sikertelen", "Socket.IO-hozzáférés",
                        BlitzEventArgs.EventMood.ERR));
            }
        }

        private void OnReceiveAlerts(object obj)
        {
            var log = JsonConvert.DeserializeObject<MetHuAlerts>(obj.ToString(), JSON_SERIALIZER);
        }

        private void OnSocketIoError(object obj)
        {
            ShowBadge?.Invoke(this, new BadgeEventArgs(BadgeEventArgs.BadgeType.CONNECTION_ERROR));
            PushToLog?.Invoke(this,
                new BlitzEventArgs("Socket.IO kapcsolati hiba", "Socket.IO-hozzáférés", BlitzEventArgs.EventMood.ERR));
        }

        private void OnReceiveSingleStroke(object strokeData)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                var stroke = Stroke.GetBlitzFromJsonArray(strokeData);
                stroke.Bearing = LatLon.Bearing(_userLatLon, stroke.LatLon);
                stroke.Distance = LatLon.Distance(stroke.LatLon, _userLatLon);
                onSingleStrokeReceived?.Invoke(this, new SingleStrokeReceivedEventArgs {Stroke = stroke});
            });
        }

        private void OnReceiveLogs(object logData)
        {
            var logString = logData.ToString();
            var log = JsonConvert.DeserializeObject<ServerLog>(logData.ToString(), JSON_SERIALIZER);

            Application.Current.Dispatcher.Invoke(delegate
            {
                OnSingleServerLogReceived?.Invoke(this, new SingleServerLogEventArgs {ServerLog = log});
            });
        }

        private void OnReceiveLogsInit(object logData)
        {
            var logString = logData.ToString();
            var logs = JsonConvert.DeserializeObject<List<ServerLog>>(logData.ToString(), JSON_SERIALIZER);
            Application.Current.Dispatcher.Invoke(delegate
            {
                OnMultipleServerLogReceived?.Invoke(this, new MultipleServerLogEventArgs {ServerLogs = logs});
            });
        }

        private void OnReceiveMultipleStrokes(object strokesData)
        {
            var blitzes = Stroke.GetMultipleBlitzesFromJsonArray(strokesData);
            Application.Current.Dispatcher.Invoke(delegate
            {
                blitzes.ForEach(stroke =>
                {
                    stroke.Bearing = LatLon.Bearing(_userLatLon, stroke.LatLon);
                    stroke.Distance = LatLon.Distance(stroke.LatLon, _userLatLon);
                });
                onMultipleStrokesReceived?.Invoke(this, new MultipleStrokeReceivedEventArgs {Strokes = blitzes});
            });
        }


        private void registry()
        {
            if (Registry.CurrentUser.OpenSubKey("BlitzInfo", true) == null)
            {
                _key = Registry.CurrentUser.CreateSubKey("BlitzInfo");
                _guid = Convert.ToString(Guid.NewGuid());
                _key.SetValue("client_id", _guid);
                _key.SetValue("address", "Budapest");
                _key.SetValue("soundTreshold", _soundTreshold);
                QueriedAddress = "Budapest";
            }
            else
            {
                _key = Registry.CurrentUser.OpenSubKey("BlitzInfo", true);
                _guid = (string) _key.GetValue("client_id");
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
                    QueriedAddress = "Budapest";
                }
                else
                {
                    QueriedAddress = Convert.ToString(_key.GetValue("address"));
                }
            }

            _key.Close();
        }

        private void addressRegistry(string address)
        {
            QueriedAddress = address;
            if (Registry.CurrentUser.OpenSubKey("BlitzInfo", true) == null)
            {
                _key = Registry.CurrentUser.CreateSubKey("BlitzInfo");
                _guid = Convert.ToString(Guid.NewGuid());
                _key.SetValue("client_id", _guid);
                _key.SetValue("address", address);
            }
            else
            {
                _key = Registry.CurrentUser.OpenSubKey("BlitzInfo", true);
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
            var log = new Log
            {
                clientId = _guid, timestamp = e.timestamp, message = e.msg, header = e.msgheader,
                kind = moodtoString(e.mood)
            };
            if (e.saved) LogList.Add(log);
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
            var messageHeader = "OSMA Nominatim címlekérés";
            if (!HasKindOfProcess(Process.ProcessKind.GEOCODING_QUERY))
            {
                var geocodingWebClient = new WebClient();
                geocodingWebClient.Encoding = Encoding.UTF8;
                geocodingWebClient.Headers.Add("user-agent", "ehog BlitzInfo (.NET) - ehog@ehog.hu");
                geocodingWebClient.Headers.Add("accept-language", "hu-HU");

                var processId = _randomgen.Next();
                ProcessManagement(new Process {ProcessID = processId, kind = Process.ProcessKind.GEOCODING_QUERY});
                try
                {
                    PushToLog(this,
                        new BlitzEventArgs("Lekérés... (" + searchString + ')', messageHeader,
                            BlitzEventArgs.EventMood.AWAIT));
                    geocodingWebClient.CancelAsync();
                    var geocodingAddress =
                        $"https://nominatim.openstreetmap.org/search.php?q={WebUtility.UrlEncode(searchString)}&polygon=0&geojson=1&format=json&limit=1&addressdetails=1";

                    var geocodingJson = await geocodingWebClient.DownloadStringTaskAsync(geocodingAddress);
                    var geocodingJsonObject = JArray.Parse(geocodingJson);

                    if (geocodingJsonObject.Count() != 0)
                    {
                        var firstResult = geocodingJsonObject.First;
                        Address = (string) firstResult["display_name"];
                        addressRegistry(searchString);
                        _userLatLon = new LatLon
                        {
                            Latitude = (float) Convert.ToDouble(firstResult["lat"]),
                            Longitude = (float) Convert.ToDouble(firstResult["lon"])
                        };
                        var isCountry = firstResult["address"]["country_code"] != null;
                        if (isCountry) _countrycode = ((string) firstResult["address"]["country_code"]).ToLower();
                        if (OnAddressChanged != null)
                        {
                            OnAddressChanged(this, EventArgs.Empty);
                            PushToLog(this,
                                new BlitzEventArgs("Új cím (" + Address + ')', messageHeader,
                                    BlitzEventArgs.EventMood.OK));
                        }

                        Prepare();
                    }
                    else
                    {
                        PushToLog(this,
                            new BlitzEventArgs("A hely nem található. (" + searchString + ')', messageHeader,
                                BlitzEventArgs.EventMood.ERR));
                        onAdressNotExisting?.Invoke(this, new NoSettlemEventArgs {ProbedAddress = searchString});
                    }
                }
                catch (NotSupportedException exc)
                {
                    PushToLog(this,
                        new BlitzEventArgs("Kapcsolati hiba. (" + searchString + ')', messageHeader,
                            BlitzEventArgs.EventMood.ERR));
                }
                catch (WebException exc)
                {
                    PushToLog(this,
                        new BlitzEventArgs("A kapcsolat megszakadt vagy nincs. (" + searchString + ')',
                            messageHeader, BlitzEventArgs.EventMood.ERR));
                }
                catch (Exception exc)
                {
                    PushToLog(this,
                        new BlitzEventArgs("Ismeretlen hiba: " + exc, "Ismeretlen hiba", BlitzEventArgs.EventMood.ERR));
                }
                finally
                {
                    ProcessManagement(new Process {ProcessID = processId, kind = Process.ProcessKind.GEOCODING_QUERY});
                }
            }
            else
            {
                PushToLog(this,
                    new BlitzEventArgs("A címlekérés nem lehetséges, mert már folyamatban van.",
                        messageHeader, BlitzEventArgs.EventMood.ERR));
            }
        }

        public async void countProcess(bool isInQueue = false)
        {
            if (!HasKindOfProcess(Process.ProcessKind.COUNT_PROCESS))
            {
                var processId = _randomgen.Next();
                ProcessManagement(new Process {ProcessID = processId, kind = Process.ProcessKind.COUNT_PROCESS});
                OnCountQueryStart(this, EventArgs.Empty);
                var ok = true;
                string json;
                try
                {
                    var _client_count = new WebClient();
                    _client_count.Headers.Add("user-agent", "ehog BlitzInfo (.NET) - ehog@ehog.hu");
                    PushToLog(this,
                        new BlitzEventArgs("Adatlekérés elindult", "Villám darabszám-grafikon",
                            BlitzEventArgs.EventMood.AWAIT));
                    json = await _client_count.DownloadStringTaskAsync($"{_restUrl}/stats/tenmin/48");
                    GetCount = StrokeTenmin.GetMultipleTenminDataFromJsonArray(json);
                    GetCount.Reverse();
                }
                catch (NotSupportedException exc)
                {
                    PushToLog(this,
                        new BlitzEventArgs("Kapcsolati hiba.", "Villám darabszám-grafikon",
                            BlitzEventArgs.EventMood.ERR));
                    ok = false;
                }

                catch (WebException exc)
                {
                    PushToLog(this,
                        new BlitzEventArgs("A kapcsolat megszakadt vagy nincs.", "Villám darabszám-grafikon",
                            BlitzEventArgs.EventMood.ERR));
                    ok = false;
                }
                finally
                {
                    if (GetCount.Count == 0)
                    {
                        PushToLog(this,
                            new BlitzEventArgs(
                                "A JSON nem tartalmaz elemeket. Talán frissül? Válaszd a villámok darabszáma fület, és nyomj F5-t, amíg nincs adat!",
                                "Villám darabszám-grafikon", BlitzEventArgs.EventMood.ERR));
                        ok = false;
                    }

                    if (ok)
                    {
                        OnCountGot(this, EventArgs.Empty);
                        PushToLog(this,
                            new BlitzEventArgs("Adatlekérés befejezve.", "Villám darabszám-grafikon",
                                BlitzEventArgs.EventMood.OK));
                    }

                    ProcessManagement(new Process {ProcessID = processId, kind = Process.ProcessKind.COUNT_PROCESS});
                }
            }
            else
            {
                if (!isInQueue)
                    PushToLog(this,
                        new BlitzEventArgs(
                            "A darabszám-grafikon adatainak lekérése már folyamatban, várakozási sorhoz adás...",
                            "Villám darabszám-grafikon", BlitzEventArgs.EventMood.ERR));
                addToQueue(Process.ProcessKind.COUNT_PROCESS);
            }
        }


        public async void StationsOverallStatUpdate(bool isInQueue = false)
        {
            if (!HasKindOfProcess(Process.ProcessKind.STATION_STAT))
            {
                var processId = _randomgen.Next();
                ProcessManagement(new Process {ProcessID = processId, kind = Process.ProcessKind.STATION_STAT});
                OnStationsOverallStatQueryStart(this, EventArgs.Empty);
                var ok = true;
                string json;
                try
                {
                    var overallStatsWebClient = new WebClient();
                    overallStatsWebClient.Encoding = Encoding.UTF8;
                    overallStatsWebClient.Headers.Add("user-agent", "ehog BlitzInfo (.NET) - ehog@ehog.hu");
                    PushToLog(this,
                        new BlitzEventArgs("Adatlekérés elindult", "Összes állomásadat",
                            BlitzEventArgs.EventMood.AWAIT));
                    json = await overallStatsWebClient.DownloadStringTaskAsync($"{_restUrl}/stations");
                    OverallStationStats = StationStatEntry.GetEntriesFromJson(json);
                }
                catch (NotSupportedException exc)
                {
                    PushToLog(this,
                        new BlitzEventArgs("Kapcsolati hiba.", "Összes állomásadat", BlitzEventArgs.EventMood.ERR));
                    ok = false;
                }

                catch (WebException exc)
                {
                    PushToLog(this,
                        new BlitzEventArgs("A kapcsolat megszakadt vagy nincs.", "Összes állomásadat",
                            BlitzEventArgs.EventMood.ERR));
                    ok = false;
                }
                finally
                {
                    if (GetOverallStats.Count == 0)
                    {
                        PushToLog(this,
                            new BlitzEventArgs("Nincsenek adatok", "Összes állomásadat", BlitzEventArgs.EventMood.ERR));
                        ok = false;
                    }

                    if (ok)
                    {
                        OnStationsOverallStatQueryResult(this, EventArgs.Empty);
                        PushToLog(this,
                            new BlitzEventArgs("Adatlekérés befejezve.", "Összes állomásadat",
                                BlitzEventArgs.EventMood.OK));
                    }

                    ProcessManagement(new Process {ProcessID = processId, kind = Process.ProcessKind.STATION_STAT});
                }
            }
        }

        public async void OverallStatUpdate(bool isInQueue = false)
        {
            if (!HasKindOfProcess(Process.ProcessKind.OVERALL_COUNT))
            {
                var processId = _randomgen.Next();
                ProcessManagement(new Process {ProcessID = processId, kind = Process.ProcessKind.OVERALL_COUNT});
                OverallStatQueryStart(this, EventArgs.Empty);
                var ok = true;
                string json;
                try
                {
                    var overallStatsWebClient = new WebClient();
                    overallStatsWebClient.Headers.Add("user-agent", "ehog BlitzInfo (.NET) - ehog@ehog.hu");
                    PushToLog(this,
                        new BlitzEventArgs("Adatlekérés elindult", "Hosszútávú statisztika",
                            BlitzEventArgs.EventMood.AWAIT));
                    json = await overallStatsWebClient.DownloadStringTaskAsync($"{_restUrl}/stats/overall");
                    GetOverallStats = OverallStatEntry.GetMultipleStatEntryFromJsonArray(json);
                }
                catch (NotSupportedException exc)
                {
                    PushToLog(this,
                        new BlitzEventArgs("Kapcsolati hiba.", "Hosszútávú statisztika", BlitzEventArgs.EventMood.ERR));
                    ok = false;
                }

                catch (WebException exc)
                {
                    PushToLog(this,
                        new BlitzEventArgs("A kapcsolat megszakadt vagy nincs.", "Hosszútávú statisztika",
                            BlitzEventArgs.EventMood.ERR));
                    ok = false;
                }
                finally
                {
                    if (GetOverallStats.Count == 0)
                    {
                        PushToLog(this,
                            new BlitzEventArgs("A hosszútávú statisztika üres.", "Hosszútávú statisztika",
                                BlitzEventArgs.EventMood.ERR));
                        ok = false;
                    }

                    if (ok)
                    {
                        OverallStatQueryResult(this, EventArgs.Empty);
                        PushToLog(this,
                            new BlitzEventArgs("Adatlekérés befejezve.", "Hosszútávú statisztika",
                                BlitzEventArgs.EventMood.OK));
                    }

                    ProcessManagement(new Process {ProcessID = processId, kind = Process.ProcessKind.OVERALL_COUNT});
                }
            }
            else
            {
                if (!isInQueue)
                    PushToLog(this,
                        new BlitzEventArgs(
                            "A hosszútávú statisztika letöltése már folyamatban, várakozási sorhoz adás...",
                            "Hosszútávú statisztika", BlitzEventArgs.EventMood.ERR));
                addToQueue(Process.ProcessKind.OVERALL_COUNT);
            }
        }

        #endregion

        #region Segédfüggvények

        public string moodtoString(BlitzEventArgs.EventMood mood)
        {
            if (mood == BlitzEventArgs.EventMood.AWAIT) return "AWAIT";
            if (mood == BlitzEventArgs.EventMood.ERR) return "ERR";
            if (mood == BlitzEventArgs.EventMood.OK) return "OK";
            if (mood == BlitzEventArgs.EventMood.COMMAND) return "COMM";
            if (mood == BlitzEventArgs.EventMood.INFORMATION) return "INF";
            if (mood == BlitzEventArgs.EventMood.SERVER) return "SERV";
            return "UNKNOWN";
        }

        private string processKindToText(Process.ProcessKind kind)
        {
            if (kind == Process.ProcessKind.BLITZ_QUERY)
                return "villámok lekérése";
            if (kind == Process.ProcessKind.COUNT_PROCESS)
                return "darabszám-grafikon adatlekérés";
            if (kind == Process.ProcessKind.DISTANCE_PROCESS)
                return "távolság-grafikon adatlekérés";
            if (kind == Process.ProcessKind.GEOCODING_QUERY)
                return "Nominatim címfeloldás";
            if (kind == Process.ProcessKind.UPLOAD_LOGS)
                return "log feltöltés";
            return "ismeretlen";
        }

        #endregion

        #region Tulajdonságok

        public string Address { get; private set; }

        public string Country => _countrycode.ToUpper();

        public string QueriedAddress { get; private set; }

        public List<Stroke> GetBlitzes { get; }

        public List<BlitzDistance> GetDistances { get; }

        public List<OverallStatEntry> GetOverallStats { get; private set; }

        public List<StrokeTenmin> GetCount { get; private set; }

        public List<StationStatEntry> OverallStationStats { get; private set; }

        public List<Log> LogList { get; }

        public short DistanceTreshold { get; set; }

        public short DistanceTresholdNear { get; set; }


        public short SoundTreshold
        {
            get => _soundTreshold;
            set
            {
                _soundTreshold = value;
                var key = Registry.CurrentUser.OpenSubKey("BlitzInfo", true);
                key.SetValue("soundTreshold", _soundTreshold);
                key.Close();
            }
        }

        public int[] DirectionsArray { get; set; }

        #endregion
    }
}