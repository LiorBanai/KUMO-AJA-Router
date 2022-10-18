using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace KumoAJA.API
{
    public class KumoApiSimulator : IKumoApi
    {
        public ILogger Logger { get; }
        public event EventHandler<Dictionary<int, List<int>>> MatrixChanged;
        public event EventHandler<int> TemperatureChanged;
        public event EventHandler<List<KumoText>> TextChanged;
        public event EventHandler<List<KumoColor>> ColorChanged;
        public event EventHandler<List<KumoLock>> LockedChanged; 
        public event EventHandler SignalSwitchingModeChanged;

        public bool IsEventsPoolingActive { get; private set; }
        public Task ForcePolling()
        {
            return Task.CompletedTask;
        }

        public KumoApiSimulator(ILogger logger)
        {
            Logger = logger;
        }
        public async Task<bool> Login(string pw)
        {
            await Task.Delay(1000);
            return true;
        }

        public async Task<Dictionary<int, List<int>>> GetMatrix()
        {
            var m = new Dictionary<int, List<int>>();
            for (int i = 1; i <= 4; i++)
            {
                m.Add(i, new List<int>());
            }
            m[1].AddRange(new List<int>() { 1, 1 });
            m[2].AddRange(new List<int>() { 2, 2,  });
            m[3].AddRange(new List<int>() { 3,3 });
            m[4].AddRange(new List<int>() { 2});
            return m;
        }

        public Task<int> GetDestinationStatus(int destNum)
        {
            return Task.FromResult(1);
        }

        public Task<string> GetNetworkCommand()
        {
            return Task.FromResult("");
        }

        public Task<string> GetNetworkState()
        {
            return Task.FromResult("");
        }

        public Task<string> GetControlPanelMode()
        {
            return Task.FromResult("");
        }

        public Task<string> GetKumoProductId()
        {
            return Task.FromResult("");
        }

        public Task<string> GetNumberOfSources()
        {
            return Task.FromResult("16");
        }

        public Task<string> GetAuthentication()
        {
            return Task.FromResult("");
        }

        public Task<string> GetIpConfig()
        {
            return Task.FromResult("");
        }

        public Task<string> GetIpAddress()
        {
            return Task.FromResult("");
        }

        public Task<string> GetIPAddress_1()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetIPAddress_2()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetIPAddress_3()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetSubnetMask()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetSubnetMask_1()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetSubnetMask_2()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetSubnetMask_3()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetDefaultGateway()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetDefaultGateway_1()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetDefaultGateway_2()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetDefaultGateway_3()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetSysName()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetMacAddress()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetEmergencyIpAddress()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetSuppressPsAlarm()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetSuppressReferenceAlarm()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetSuppressTemperatureAlarm()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetTemperatureAlarmSetPoint()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetDisplayIntensity()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetHighTallyIntensity()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetLowTallyIntensity()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetPsledIntensity()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetSWVersion()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetSafeBootMode()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetSerialNumber()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetFactorySettings()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetLedIdentify()
        {
            return Task.FromResult("LED1");
        }

        public Task SetLedIdentify(bool status)
        {
            throw new NotImplementedException();
        }

        public Task<bool> GetLedIdentifyStatus()
        {
            return Task.FromResult(true);
        }

        public Task<string> GetAlarmLed()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetDetectReferenceFormat()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetTemperature()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetPsAlarm()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetReferenceAlarm()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetTemperatureAlarm()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsPortNumLocked(int destinationPortNum)
        {
            await Task.Delay(0);
            return (destinationPortNum % 2 == 0) ? true : false;
        }

        public async Task<string> GetHexColorByPortNum(int pNum)
        {
            await Task.Delay(0);
            return (pNum % 2 == 0) ? "#c84b91" : "#d9cb7e";
        }

        public Task<string> SetLockValueByPortNum(int pNum, bool isLocked)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetSourceXLine1(int sourcePortNumber)
        {
            return Task.FromResult($"source_{sourcePortNumber}");
        }

        public Task<string> GetDestinationXLine1(int destinationPortNumber)
        {
            return Task.FromResult($"dest_{destinationPortNumber}");
        }

        public Task<string> SetDestinationStatus(int destNum, int portNum)
        {
            return Task.FromResult("");
        }

        public Task SetSourceXLine1(int sourcePortNumber, string text)
        {
            return Task.FromResult("");
        }

        public Task SetDestinationXLine1(int destinationPortNumber, string text)
        {
            return Task.FromResult("");
        }

        public void StartPollingEvents()
        {
            IsEventsPoolingActive = true;
        }

        public void StopPollingEvents()
        {
            IsEventsPoolingActive = false;
        }
    }
}
