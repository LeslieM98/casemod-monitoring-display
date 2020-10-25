

using OpenHardwareMonitor.Hardware;
using System;
using System.Management.Instrumentation;

namespace MonitorPCModule.Transmitter.Hardware
{
    class GPU
    {
        private IHardware gpu { get; }
        public GPU(IComputer computer)
        {
            gpu = FindGPU(computer);
        }

        private IHardware FindGPU(IComputer computer)
        {
            foreach(IHardware hardware in computer.Hardware)
            {
                 if(hardware.HardwareType == HardwareType.GpuNvidia)
                {
                    return hardware;
                }
            }
            throw new InstanceNotFoundException("Could not find Nvidia GPU");
        }

        public float Temperature()
        {
            float? val = new float?();
            gpu.Update();
            foreach(ISensor sensor in gpu.Sensors)
            {
                if (sensor.SensorType == SensorType.Temperature && sensor.Name.ToLower().Contains("core")){
                    val = sensor.Value;
                    break;
                }
            }

            return val.GetValueOrDefault(0f);
        }
    }
}
