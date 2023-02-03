using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;

namespace Kumo.Routing.API
{
    public class KumoApi : IKumoApi
    {
        private KumoConnectionSettings Settings { get; }
        private ILogger Logger { get; }
        public event EventHandler<Dictionary<int, List<int>>> MatrixChanged;
        public event EventHandler<int> TemperatureChanged;
        public event EventHandler<List<KumoText>> TextChanged;
        public event EventHandler<List<KumoColor>> ColorChanged;
        public event EventHandler<List<KumoLock>> LockedChanged;
        public event EventHandler SignalSwitchingModeChanged;
        public event EventHandler<bool> ConnectionStateChanged;
        private string _cookieToken;
        private DateTime LastPooling { get; set; }
        private readonly RestClient _client;

        private int NumberOfPorts { get; set; }
        private int ConnectId { get; set; }
        private SemaphoreSlim lockSlim;
        public bool IsEventsPoolingActive { get; private set; }
        public bool Connected { get; private set; }
        Task onReportTask;

        private CancellationTokenSource cts;
        private CancellationToken cancelToken;

        private KumoApi()
        {
            ConnectId = -1;
            cts = new CancellationTokenSource();
            cancelToken = cts.Token;
            LastPooling = DateTime.MinValue;
            lockSlim = new SemaphoreSlim(1);
        }

        /// <summary>
        /// initialize router address
        /// </summary>
        public KumoApi(KumoSettings settings, ILogger logger) : this()
        {
            Settings = settings.ConnectionSettings;
            Logger = logger;
            var options = new RestClientOptions(Settings.KumoIP)
            {
                ThrowOnAnyError = true,
                MaxTimeout = Settings.Timeout
            };
            _client = new RestClient(options);
        }

        /// <summary>
        /// initialize router address
        /// </summary>
        public KumoApi(KumoSettings settings, ILogger<KumoApi> logger) : this(settings, (ILogger)logger)
        {

        }
        /// <summary>
        /// execute the api by command name
        /// </summary>
        private async Task<string> GetCommand(string str, bool getByValueName = false, [CallerMemberName] string memberName = "")
        {
            try
            {
                var request = new RestRequest("/config?action=get&paramid=" + str, Method.Get);
                if (!string.IsNullOrEmpty(_cookieToken))
                {
                    request.AddHeader("Cookie", _cookieToken);

                }
                LogRequest(request, memberName);
                RestResponse restResponse = await _client.ExecuteAsync(request);
                LogResponse(restResponse, memberName);
                var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(restResponse.Content);
                if (getByValueName)
                {
                    return values["value_name"];
                }
                else
                {
                    return values["value"];
                }

            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "get command failed: {cmd}", str);
                return string.Empty;
            }
        }

        ///// <summary>
        ///// check if user is authorized
        ///// </summary>
        ///// <returns></returns>
        //public bool IsAuthorized()
        //{
        //    var request = new RestRequest("/authenticator/login", Method.Post);
        //    request.AddHeader("content-type", "application/x-www-form-urlencoded");
        //    request.AddHeader("Accept", "*/*");
        //    request.AddParameter("password_provided", "", ParameterType.GetOrPost);
        //    RestResponse restResponse = _client.ExecutePostAsync(request);
        //    if (restResponse.Content.Contains("success"))
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LogRequest(RestRequest request, [CallerMemberName] string memberName = "")
        {
            Logger?.LogInformation("Request from {method}: {login} Parameters:{parameters}", memberName, request.Resource, string.Join(",", request.Parameters));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LogResponse(RestResponse restResponse, [CallerMemberName] string memberName = "")
        {
            if (restResponse.StatusCode == HttpStatusCode.OK)
            {
                Logger?.LogInformation("response for {method}: {login} ", memberName, restResponse.Content);
            }
            else
            {
                Logger?.LogError("response for {method}: {login} ", memberName, restResponse.Content);
            }
        }
        public async Task<bool> Login(string pw)
        {
            try
            {
                if (string.IsNullOrEmpty(pw))
                {
                    pw = "";
                }
                var request = new RestRequest("/authenticator/login", Method.Post);
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                request.AddHeader("Accept", "*/*");
                request.AddParameter("password_provided", pw, ParameterType.GetOrPost);
                LogRequest(request);
                RestResponse restResponse = await _client.ExecutePostAsync(request);
                LogResponse(restResponse);
                if (restResponse.Content != null && restResponse.StatusCode == HttpStatusCode.OK)
                {
                    foreach (HeaderParameter header in restResponse.Headers)
                    {
                        Logger?.LogInformation("Header: {key}:{value}", header.Name, header.Value);
                    }
                    var cookie = restResponse.Headers.FirstOrDefault(x => x.Name == "Set-Cookie");
                    this._cookieToken = cookie.Value.ToString();
                    if (string.IsNullOrEmpty(_cookieToken) || _cookieToken.Contains("invalid"))
                    {
                        Connected = false;
                        ConnectionStateChanged?.Invoke(this,false);
                        return false;
                    }
                    NumberOfPorts = Convert.ToInt32(await GetNumberOfSources());
                    Connected = true;
                    ConnectionStateChanged?.Invoke(this, true);
                    return true;
                }

                Connected = false;
                ConnectionStateChanged?.Invoke(this, false);
                return false;
            }
            catch (Exception ex)
            {
                ConnectionStateChanged?.Invoke(this, false);
                Logger?.LogError(ex, "login failed");
                return false;
            }
        }

        public async Task<string> GetDeviceInformation()
        {
            try
            {
                var request = new RestRequest("browse.json", Method.Get);

                if (!string.IsNullOrEmpty(_cookieToken))
                {
                    request.AddHeader("Cookie", _cookieToken);
                }
                request.RequestFormat = DataFormat.Json;
                var response = await _client.ExecuteAsync(request);
                List<KumoDeviceInformation> info = JsonConvert.DeserializeObject<List<KumoDeviceInformation>>(response.Content);
                return info.First().Description;
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Get Info failed; {msg}",ex.Message);
                return "";
            }
        }

        private async Task<string> SetCommand(string str, string value)
        {
            try
            {
                var request = new RestRequest("/config?action=set&paramid=" + str + "&value=" + value, Method.Get);
                if (!string.IsNullOrEmpty(_cookieToken))
                {
                    request.AddHeader("Cookie", _cookieToken);
                }
                request.RequestFormat = DataFormat.Json;
                var response = await _client.ExecuteAsync(request);
                string Content = response.Content;
                return Content;
            }
            catch (Exception e)
            {
                Logger?.LogError(e, "Error executing command. {Message}", e.Message);
                return $"Error: {e.Message}";
            }


        }

        /// <summary>
        /// get the text that appears on the top line of the destination X button
        /// </summary>
        /// <param name="destinationPortNumber"></param>
        /// <returns></returns>
        public Task<string> GetDestinationXLine1(int destinationPortNumber) => GetCommand($"eParamID_XPT_Destination{destinationPortNumber}_Line_1");

        /// <summary>
        /// set the text that appears on the top line of the destination X button
        /// </summary>
        /// <param name="destinationPortNumber"></param>
        /// <param name="text"></param>
        public Task SetDestinationXLine1(int destinationPortNumber, string text) => SetCommand($"eParamID_XPT_Destination{destinationPortNumber}_Line_1", text);


        /// <summary>
        ///  get the text that appears on the bottom line of the destination X button
        /// </summary>
        /// <param name="destinationPortNumber"></param>
        /// <returns></returns>
        public Task<string> GetDestinationXLine2(int destinationPortNumber) => GetCommand($"eParamID_XPT_Destination{destinationPortNumber}_Line_2");


        /// <summary>
        /// set the text that appears on the bottom line of the destination X button
        /// </summary>
        /// <param name="destinationPortNumber"></param>
        /// <param name="text"></param>
        public Task SetDestinationXLine2(int destinationPortNumber, string text) => SetCommand($"eParamID_XPT_Destination{destinationPortNumber}_Line_2", text);


        /// <summary>
        ///  get the text that appears on the top line of the source X button
        /// </summary>
        /// <param name="sourcePortNumber"></param>
        /// <returns></returns> 
        public Task<string> GetSourceXLine1(int sourcePortNumber) => GetCommand($"eParamID_XPT_Source{sourcePortNumber}_Line_1");


        /// <summary>
        ///  set the text that appears on the top line of the source X button
        /// </summary>
        /// <param name="sourcePortNumber"></param>
        public Task SetSourceXLine1(int sourcePortNumber, string text) => SetCommand($"eParamID_XPT_Source{sourcePortNumber}_Line_1", text);


        /// <summary>
        /// Defines the text that appears on the bottom line of the source X button
        /// </summary>
        /// <param name="sourcePortNumber"></param>
        public Task<string> GetSourceXLine2(int sourcePortNumber) => GetCommand($"eParamID_XPT_Source{sourcePortNumber}_Line_2");


        /// <summary>
        ///  set the text that appears on the bottom line of the source X button
        /// </summary>
        /// <param name="sourcePortNumber"></param>
        public Task<string> SetSourceXLine2(int sourcePortNumber, string text) => SetCommand($"eParamID_XPT_Source{sourcePortNumber}_Line_2", text);

        /// <summary>
        /// get value of which source is connected to destination destNum
        /// </summary>
        public async Task<int> GetDestinationStatus(int destNum) => Convert.ToInt32(await GetCommand("eParamID_XPT_Destination" + destNum + "_Status"));


        /// <summary>
        /// set value of which source is connected to destination destNum
        /// </summary>
        public Task<string> SetDestinationStatus(int destNum, int portNum) => SetCommand("eParamID_XPT_Destination" + destNum + "_Status", Convert.ToString(portNum));

        /// <summary>
        /// get network Command
        /// </summary>
        /// <returns>
        /// Options:1-No Command , 2-Run DHCP Query , 3-Commit New IP Info
        /// </returns>
        public Task<string> GetNetworkCommand() => GetCommand("eParamID_NetworkCommand", true);

        /// <summary>
        /// get network State
        /// </summary>
        /// <returns>
        /// Options:1-Uninitialized , 2-Running DHCP Query , 3-Committing IP Info,4-IP Configured,5-Error Configuring IP,6-Redirect needed,7=Sent Redirect
        /// </returns>
        public Task<string> GetNetworkState() => GetCommand("eParamID_NetworkState", true);

        public Task<string> GetControlPanelMode() => GetCommand("eParamID_Control_Panel_Mode");

        /// <summary>
        /// The product id of this KUMO unit
        /// </summary>
        public Task<string> GetKumoProductId() => GetCommand("eParamID_KumoProductID", true);

        /// <summary>
        /// Defines the number of sources
        /// </summary>
        /// <returns></returns>
        public Task<string> GetNumberOfSources() => GetCommand("eParamID_NumberOfSources");

        public Task<string> GetAuthentication() => GetCommand("eParamID_Authentication", true);

        public Task<string> GetIpConfig() => GetCommand("eParamID_IPConfig", true);

        public async Task<string> GetIpAddress() => ToAddr(Convert.ToInt32(await GetCommand("eParamID_IPAddress")));

        public async Task<string> GetIPAddress_1() => ToAddr(Convert.ToInt32(await GetCommand("eParamID_IPAddress_1")));
        public async Task<string> GetIPAddress_2() => ToAddr(Convert.ToInt32(await GetCommand("eParamID_IPAddress_2")));
        public async Task<string> GetIPAddress_3() => ToAddr(Convert.ToInt32(await GetCommand("eParamID_IPAddress_3")));
        public async Task<string> GetSubnetMask() => ToAddr(Convert.ToInt32(await GetCommand("eParamID_SubnetMask")));
        public async Task<string> GetSubnetMask_1() => ToAddr(Convert.ToInt32(await GetCommand("eParamID_SubnetMask_1")));
        public async Task<string> GetSubnetMask_2() => ToAddr(Convert.ToInt32(await GetCommand("eParamID_SubnetMask_2")));
        public async Task<string> GetSubnetMask_3() => ToAddr(Convert.ToInt32(await GetCommand("eParamID_SubnetMask_3")));
        public async Task<string> GetDefaultGateway() => ToAddr(Convert.ToInt32(await GetCommand("eParamID_DefaultGateway")));
        public async Task<string> GetDefaultGateway_1() => ToAddr(Convert.ToInt32(await GetCommand("eParamID_DefaultGateway_1")));
        public async Task<string> GetDefaultGateway_2() => ToAddr(Convert.ToInt32(await GetCommand("eParamID_DefaultGateway_2")));
        public async Task<string> GetDefaultGateway_3() => ToAddr(Convert.ToInt32(await GetCommand("eParamID_DefaultGateway_3")));

        public Task<string> GetSysName() => GetCommand("eParamID_SysName", true);

        public Task<string> GetMacAddress() => GetCommand("eParamID_MACAddress");

        public async Task<string> GetEmergencyIpAddress() => ToAddr(Convert.ToInt32(await GetCommand("eParamID_EmergencyIPAddress")));

        public Task<string> GetSuppressPsAlarm() => GetCommand("eParamID_SuppressPSAlarm", true);

        public Task<string> GetSuppressReferenceAlarm() => GetCommand("eParamID_SuppressReferenceAlarm", true);

        public Task<string> GetSuppressTemperatureAlarm() => GetCommand("eParamID_SuppressTemperatureAlarm", true);

        public Task<string> GetTemperatureAlarmSetPoint() => GetCommand("eParamID_TemperatureAlarmSetpoint", true);

        public Task<string> GetDisplayIntensity() => GetCommand("eParamID_DisplayIntensity");

        public Task<string> GetHighTallyIntensity() => GetCommand("eParamID_HighTallyIntensity");

        public Task<string> GetLowTallyIntensity() => GetCommand("eParamID_LowTallyIntensity");

        public Task<string> GetPsledIntensity() => GetCommand("eParamID_PSLEDIntensity");

        public Task<string> GetSWVersion() => GetCommand("eParamID_SWVersion");
        public Task<string> GetSafeBootMode() => GetCommand("eParamID_SafebootMode");
        public Task<string> GetSerialNumber() => GetCommand("eParamID_SerialNumber");
        public Task<string> GetFactorySettings() => GetCommand("eParamID_FactorySettings", true);
        public Task<string> GetLedIdentify() => GetCommand("eParamID_LED_Identify", true);

        public async Task<bool> GetLedIdentifyStatus() => ConvertNumberToBool(await GetCommand("eParamID_LED_Identify", true));

        public Task SetLedIdentify(bool status) => SetCommand("eParamID_LED_Identify", ConvertBoolToNumber(status));

        public Task<string> GetAlarmLed() => GetCommand("eParamID_AlarmLed");

        public Task<string> GetDetectReferenceFormat() => GetCommand("eParamID_DetectReferenceFormat");

        public Task<string> GetTemperature() => GetCommand("eParamID_Temperature");

        public Task<string> GetPsAlarm() => GetCommand("eParamID_PSAlarm", true);

        public Task<string> GetReferenceAlarm() => GetCommand("eParamID_ReferenceAlarm", true);

        public Task<string> GetTemperatureAlarm() => GetCommand("eParamID_TemperatureAlarm", true);

        public async Task<string> GetHexColorByPortNum(int pNum) => GetColorByClass(await GetCommand("eParamID_Button_Settings_" + pNum, true));

        public async Task<bool> IsPortNumLocked(int destinationPortNum) => await GetCommand("eParamID_XPT_Destination" + destinationPortNum + "_Locked") == "1";

        public Task<string> SetLockValueByPortNum(int pNum, bool isLocked) => SetCommand("eParamID_XPT_Destination" + pNum + "_Locked", isLocked ? "1" : "0");

        private string ToAddr(int address) => new IPAddress(BitConverter.GetBytes(address)).ToString();

        private string ConvertBoolToNumber(bool status) => status == true ? "1" : "0";

        private bool ConvertNumberToBool(string status) => status == "Blink" ? true : false;

        /// <summary>
        /// define which each source is connected to  specific target 
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<int, List<int>>> GetMatrix()
        {
            Dictionary<int, List<int>> dic = new Dictionary<int, List<int>>();
            for (int i = 1; i <= NumberOfPorts; i++)
            {
                dic[i] = new List<int>();
            }
            for (int i = 1; i <= NumberOfPorts; i++)
            {

                var dest = await GetCommand("eParamID_XPT_Destination" + i + "_Status");
                int idx = Convert.ToInt32(dest);
                if (idx <= 0)
                {
                    continue;
                }

                dic[idx].Add(i);

            }

            return dic;
        }
        /// <summary>
        /// convert dictionary<int,int> to dictionary<int,list<int>>
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private Dictionary<int, List<int>> ConvertDictionary(Dictionary<int, int> data)
        {
            if (data.Count == 0) return new Dictionary<int, List<int>>();

            Dictionary<int, List<int>> dataDicrionary = new Dictionary<int, List<int>>();
            List<int> list = new List<int>();

            foreach (KeyValuePair<int, int> value in data)
            {
                if (!dataDicrionary.ContainsKey(value.Value))
                {
                    dataDicrionary[value.Value] = new List<int> { value.Key };
                }
                else
                {
                    list = dataDicrionary[value.Value];
                    list.Add(value.Key);
                    dataDicrionary[value.Value] = list;
                }
            }

            return dataDicrionary;
        }

        private async Task<int> GetConId()
        {
            var request = new RestRequest("/config?action=connect", Method.Get);
            if (!string.IsNullOrEmpty(_cookieToken))
            {
                request.AddHeader("Cookie", _cookieToken);
            }
            LogRequest(request);
            RestResponse restResponse = await _client.ExecuteAsync(request);
            LogResponse(restResponse);
            if (restResponse.StatusCode == HttpStatusCode.OK)
            {
                KumoConnectionIdData connectionData = JsonConvert.DeserializeObject<KumoConnectionIdData>(restResponse.Content);
                return connectionData.ConnectionId;
            }

            return -1;
        }


        private async Task<KumoEvent> GetEvents(int connectId)
        {


            var request =
                new RestRequest("/config?action=wait_for_config_events&connectionid=" + connectId.ToString(),
                    Method.Get);
            if (!string.IsNullOrEmpty(_cookieToken))
            {
                request.AddHeader("Cookie", _cookieToken);
                request.AddHeader("Accept", "*/*");
            }

            RestResponse res = await _client.ExecuteAsync(request);
            if (res.StatusCode == HttpStatusCode.OK)
            {
                string data = res.Content;
                List<KumoEventData> eventData = JsonConvert.DeserializeObject<List<KumoEventData>>(data);
                if (eventData.Any())
                {
                    Logger?.LogInformation("polling event Data: {data}", data);
                    int tempvValue = -1;
                    List<KumoText> kumoPortTextList = new List<KumoText>();
                    List<KumoColor> kumoColorList = new List<KumoColor>();
                    List<KumoLock> kumoLockList = new List<KumoLock>();
                    Dictionary<int, List<int>> sourcePortMap = new Dictionary<int, List<int>>();
                    if (eventData.Exists(d => d.ParamID.Equals("eParamID_SignalSwitching")))
                    {
                        //Mode was changed (number of connections (4,8,16)). Need to recreate the UI
                        NumberOfPorts = Convert.ToInt32(await GetNumberOfSources());
                        SignalSwitchingModeChanged?.Invoke(this, EventArgs.Empty);
                        return KumoEvent.Empty;

                    }
                    //get Temperature
                    if (eventData.Exists(d => d.ParamID.Equals("eParamID_Temperature")))
                    {
                        KumoEventData tempDic = eventData.First(x => x.ParamID.Equals("eParamID_Temperature"));
                        tempvValue = tempDic.NumericValue;
                    }
                    sourcePortMap = GetChangedMatrix(eventData);
                    kumoPortTextList = GetChangedText(eventData);
                    kumoColorList = GetChangedColor(eventData);
                    kumoLockList = GetLockedStatus(eventData);
                    return new KumoEvent(tempvValue, sourcePortMap, kumoPortTextList, kumoColorList, kumoLockList);
                }

                return KumoEvent.Empty;

            }
            else
            {
                Logger?.LogError("Error getting events: Status code {StatusCode}.", res.StatusCode);
                return KumoEvent.Empty;
            }
        }

        private Dictionary<int, List<int>> GetChangedMatrix(List<KumoEventData> values)
        {
            var statusList = values.Where(x => (x.ParamID.Contains("_Status") && x.ParamID.Contains("Destination"))).ToList();
            Dictionary<int, int> desPortMap = new Dictionary<int, int>();

            for (int i = 0; i < statusList.Count; i++)
            {
                int source = statusList[i].NumericValue;
                int des = Convert.ToInt32(Regex.Match(statusList[i].ParamID, @"\d+").Value);
                desPortMap[des] = source;
            }
            Dictionary<int, List<int>> sourcePortMap = ConvertDictionary(desPortMap);
            return sourcePortMap;
        }

        private List<KumoLock> GetLockedStatus(List<KumoEventData> values)
        {
            List<KumoLock> KumoLockList = new List<KumoLock>();
            var dataList = values.Where(x => (x.ParamID.Contains("_Locked"))).ToList();
            string indexOf = "_Destination";

            if (dataList.Any())
            {
                KumoLockList = new List<KumoLock>();
                KumoLock kL = new KumoLock();
                for (int i = 0; i < dataList.Count; i++)
                {
                    string paramId = dataList[i].ParamID;
                    bool status = dataList[i].NameValue.Equals("Locked", StringComparison.OrdinalIgnoreCase);
                    var portNum = Convert.ToInt32(Regex.Match(paramId, @"\d+").Value);
                    kL.isLocked = status;
                    kL.portNum = portNum;
                    KumoLockList.Add(kL);
                }

            }
            return KumoLockList;
        }

        private List<KumoColor> GetChangedColor(List<KumoEventData> values)
        {
            List<KumoColor> kumoColorList = new List<KumoColor>();
            var dataList = values.Where(x => (x.ParamID.Contains("Button_Settings_"))).ToList();
            if (dataList.Any())
            {
                kumoColorList = new List<KumoColor>();
                KumoColor kC = new KumoColor();
                for (int i = 0; i < dataList.Count; i++)
                {
                    string paramId = dataList[i].ParamID;
                    string className = dataList[i].NameValue;
                    int portNum = Convert.ToInt32(Regex.Match(dataList[i].ParamID, @"\d+").Value);
                    if (portNum > NumberOfPorts)
                    {
                        kC.pT = portType.Destination;
                        portNum = portNum - NumberOfPorts;
                    }
                    else
                    {
                        kC.pT = portType.Source;
                    }

                    kC.ColorHex = GetColorByClass(className);
                    kC.portNum = portNum;
                    kumoColorList.Add(kC);
                }

            }
            return kumoColorList;
        }

        private List<KumoText> GetChangedText(List<KumoEventData> values)
        {
            List<KumoText> kumoPortTextList = new List<KumoText>();
            var textsList = values.Where(x => (x.ParamID.Contains("_XPT_") && x.ParamID.Contains("_Line_"))).ToList();
            if (textsList.Any())
            {
                kumoPortTextList = new List<KumoText>();
                KumoText kPT = null;
                for (int i = 0; i < textsList.Count; i++)
                {
                    kPT = new KumoText();
                    string paramId = textsList[i].ParamID;
                    if (paramId.EndsWith("Line_1", StringComparison.InvariantCultureIgnoreCase))
                    {
                        kPT.Line1Text = textsList[i].NameValue;
                    }
                    else if (paramId.EndsWith("Line_2", StringComparison.InvariantCultureIgnoreCase))
                    {
                        kPT.Line2Text = textsList[i].NameValue;
                    }

                    portType p = paramId.Contains("Source") ? portType.Source : portType.Destination;
                    var portNum = Convert.ToInt32(Regex.Match(paramId, @"\d+").Value);
                    kPT.portNum = portNum;
                    kPT.pT = p;
                    kumoPortTextList.Add(kPT);
                }


            }
            return kumoPortTextList;
        }

        private static bool IsNumber(string str, out int number) => int.TryParse(str, out number);

        private void EventOn()
        {
            if (IsEventsPoolingActive)
            {
                Logger?.LogWarning("already listening to events.");
                return;
            }

            IsEventsPoolingActive = true;
            if (onReportTask == null || cts.IsCancellationRequested)
            {
                onReportTask = Task.Run(async () =>
                {
                    while (!cts.IsCancellationRequested)
                    {
                        try
                        {
                            await PollEvents();
                            await Task.Delay(Settings.PollInterval, cancelToken);
                        }
                        catch (TaskCanceledException e)
                        {
                            Logger?.LogInformation(e, "Cancelling events reading dut to cancel request");
                        }
                        catch (Exception e)
                        {
                            Logger?.LogError(e, "Cancelling events reading");
                        }
                    }
                }, cancelToken);
            }
        }

        public async Task ForcePolling()
        {
            LastPooling = DateTime.MinValue;
            await PollEvents();
        }
        private async Task PollEvents()
        {

            try
            {
                await lockSlim.WaitAsync(cancelToken);
                if (ConnectId == -1)
                {
                    Logger?.LogInformation("Connect id is not set");
                    ConnectId = await GetConId();
                    Logger?.LogInformation("Current Connect id is {id}", ConnectId);

                }

                KumoEvent kE = await GetEvents(ConnectId);
                if (kE.portMap.Any())
                {
                    MatrixChanged?.Invoke(this, kE.portMap);
                }

                if (kE.temperatureValue > -1)
                {
                    TemperatureChanged?.Invoke(this, kE.temperatureValue);
                }


                if (kE.textValues.Any())
                {
                    TextChanged?.Invoke(this, kE.textValues.ToList());
                }

                if (kE.colorValues.Any())
                {
                    ColorChanged?.Invoke(this, kE.colorValues.ToList());
                }

                if (kE.lockValue.Any())
                {
                    LockedChanged?.Invoke(this, kE.lockValue.ToList());
                }

                LastPooling = DateTime.Now;
                if (!Connected)
                {
                    ConnectionStateChanged?.Invoke(this, true);
                }
                Connected = true;
            }
            
            catch (System.Net.Http.HttpRequestException ne)
            {
                Logger?.LogError(ne, "Error getting events {message} due to Network Exception", ne.Message);
                if (ne.Message.Contains("Request failed with status code ExpectationFailed"))
                {
                    if (!Connected)
                    {
                        ConnectionStateChanged?.Invoke(this, true);
                        Connected = true;
                    }
                    return;
                }
                if (Connected)
                {
                    ConnectionStateChanged?.Invoke(this, false);
                }
                Connected = false;
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error getting events {message}", ex.Message);
            }
            finally
            {
                lockSlim.Release();
            }
        }

        public void StartPollingEvents()
        {
            cts = new CancellationTokenSource();
            cancelToken = cts.Token;
            EventOn();
        }

        public void StopPollingEvents()
        {

            if (IsEventsPoolingActive && !cts.IsCancellationRequested)
            {
                cts.Cancel();
                onReportTask = null;
                IsEventsPoolingActive = false;
                ConnectId = -1;
            }

        }

        string GetColorByClass(string className)
        {
            switch (className)
            {
                case string c when c.Contains("color_1"): return "#cb7676";
                case string c when c.Contains("color_2"): return "#e6a52e";
                case string c when c.Contains("color_3"): return "#d9cb7e";
                case string c when c.Contains("color_4"): return "#87b4c8";
                case string c when c.Contains("color_5"): return "#64c896";
                case string c when c.Contains("color_6"): return "#ade68e";
                case string c when c.Contains("color_7"): return "#7888cb";
                case string c when c.Contains("color_8"): return "#9b8ce1";
                case string c when c.Contains("color_9"): return "#c84b91";
                default: return "#87b4c8";
            }
        }


        public void Dispose()
        {
            StopPollingEvents();
        }
    }
}
