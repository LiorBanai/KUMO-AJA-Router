using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kumo.Routing.API
{
    public interface IKumoApi
    {
        event EventHandler<Dictionary<int, List<int>>> MatrixChanged;
        event EventHandler<int> TemperatureChanged;
        event EventHandler<List<KumoText>> TextChanged;
        event EventHandler<List<KumoColor>> ColorChanged;
        event EventHandler<List<KumoLock>> LockedChanged;
        event EventHandler SignalSwitchingModeChanged;
        event EventHandler<bool> ConnectionStateChanged;
        bool Connected {get; }
        Task<bool> Login(string pw);
        Task<string> GetDeviceInformation();
        Task <Dictionary<int, List<int>>> GetMatrix();

        /// <summary>
        /// get value of which source is connected to destination destNum
        /// </summary>
        Task<int> GetDestinationStatus(int destNum);

        Task<string> GetNetworkCommand();

        /// <summary>
        /// get network State
        /// </summary>
        /// <returns>
        /// Options:1-Uninitialized , 2-Running DHCP Query , 3-Committing IP Info,4-IP Configured,5-Error Configuring IP,6-Redirect needed,7=Sent Redirect
        /// </returns>
        Task<string> GetNetworkState();
        Task<string> GetControlPanelMode();
        Task<string> GetKumoProductId();
        Task<string> GetNumberOfSources();
        Task<string> GetAuthentication();
        Task<string> GetIpConfig();
        Task<string> GetIpAddress();
        Task<string> GetIPAddress_1();
        Task<string> GetIPAddress_2();
        Task<string> GetIPAddress_3();
        Task<string> GetSubnetMask();
        Task<string> GetSubnetMask_1();
        Task<string> GetSubnetMask_2();
        Task<string> GetSubnetMask_3();
        Task<string> GetDefaultGateway();
        Task<string> GetDefaultGateway_1();
        Task<string> GetDefaultGateway_2();
        Task<string> GetDefaultGateway_3();
        Task<string> GetSysName();
        Task<string> GetMacAddress();
        Task<string> GetEmergencyIpAddress();
        Task<string> GetSuppressPsAlarm();
        Task<string> GetSuppressReferenceAlarm();
        Task<string> GetSuppressTemperatureAlarm();
        Task<string> GetTemperatureAlarmSetPoint();
        Task<string> GetDisplayIntensity();
        Task<string> GetHighTallyIntensity();
        Task<string> GetLowTallyIntensity();
        Task<string> GetPsledIntensity();
        Task<string> GetSWVersion();
        Task<string> GetSafeBootMode();
        Task<string> GetSerialNumber();
        Task<string> GetFactorySettings();
        Task<string> GetLedIdentify();


        Task SetLedIdentify(bool status);
        Task<bool> GetLedIdentifyStatus();
        Task<string> GetAlarmLed();
        Task<string> GetDetectReferenceFormat();
        Task<string> GetTemperature();
        Task<string> GetPsAlarm();
        Task<string> GetReferenceAlarm();
        Task<string> GetTemperatureAlarm();
        Task<bool> IsPortNumLocked(int destinationPortNum);
        Task<string> GetHexColorByPortNum(int pNum);
        Task<string> SetLockValueByPortNum(int pNum, bool isLocked);

        /// <summary>
        ///  get the text that appears on the top line of the source X button
        /// </summary>
        /// <param name="sourcePortNumber"></param>
        /// <returns></returns> 
        Task<string> GetSourceXLine1(int sourcePortNumber);
        /// <summary>
        ///  get the text that appears on the bottom line of the source X button
        /// </summary>
        /// <param name="sourcePortNumber"></param>
        /// <returns></returns> 
        Task<string> GetSourceXLine2(int sourcePortNumber);
        /// <summary>
        /// get the text that appears on the top line of the destination X button
        /// </summary>
        /// <param name="destinationPortNumber"></param>
        /// <returns></returns>
        Task<string> GetDestinationXLine1(int destinationPortNumber);
        /// <summary>
        /// get the text that appears on the bottom line of the destination X button
        /// </summary>
        /// <param name="destinationPortNumber"></param>
        /// <returns></returns>
        Task<string> GetDestinationXLine2(int destinationPortNumber);

        /// <summary>
        /// set value of which source is connected to destination destNum
        /// </summary>
        Task<string> SetDestinationStatus(int destNum, int portNum);

        /// <summary>
        ///  set the text that appears on the top line of the source X button
        /// </summary>
        /// <param name="sourcePortNumber"></param>
        /// <param name="text"></param>
        Task SetSourceXLine1(int sourcePortNumber, string text);

        /// <summary>
        /// set the text that appears on the top line of the destination X button
        /// </summary>
        /// <param name="destinationPortNumber"></param>
        /// <param name="text"></param>
        Task SetDestinationXLine1(int destinationPortNumber, string text);

        void StartPollingEvents();
        void StopPollingEvents();
        bool IsEventsPoolingActive { get; }
        Task ForcePolling();
    }
}
