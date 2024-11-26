using Hardware.Info;

namespace HFTbridge.Agent.Services
{
    public class HardwareMetricsManager
    {
        // main source
        private readonly HardwareInfo _data;

        public string OperatingSystem {get;}
        public string HardwareCPU {get;}
        public string HardwareNC {get;}
        public string HardwareGPU {get;}

        public HardwareMetricsManager()
        {
            _data = new HardwareInfo();
            _data.RefreshOperatingSystem();
            _data.RefreshMemoryStatus();
             //hardwareInfo.RefreshBatteryList();
             //hardwareInfo.RefreshBIOSList();
             //hardwareInfo.RefreshComputerSystemList();
            _data.RefreshCPUList();
             //hardwareInfo.RefreshDriveList();
             //hardwareInfo.RefreshKeyboardList();
            _data.RefreshMemoryList();
             //hardwareInfo.RefreshMonitorList();
             //hardwareInfo.RefreshMotherboardList();
             //hardwareInfo.RefreshMouseList();
             //hardwareInfo.RefreshNetworkAdapterList();
             //hardwareInfo.RefreshPrinterList();
             //hardwareInfo.RefreshSoundDeviceList();
             //hardwareInfo.RefreshVideoControllerList();

            OperatingSystem = _data.OperatingSystem.Name;
            var memorySize = _data.MemoryStatus.TotalPhysical;
            HardwareCPU = _data.CpuList[0].Name;
            HardwareNC = "N/A";
            HardwareGPU = "N/A";
        }
    }
}