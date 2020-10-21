

using OpenHardwareMonitor.Hardware;
using System;
using System.Management.Instrumentation;

namespace HardwareTemperature
{
    class CPU
    {
        private IHardware cpu { get; }
        public CPU(IComputer computer)
        {
            cpu = FindCPU(computer);
        }

        private IHardware FindCPU(IComputer computer)
        {
            foreach (IHardware hardware in computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.CPU)
                {
                    return hardware;
                }
            }
            throw new InstanceNotFoundException("Could not find CPU");
        }

        public float Temperature()
        {
            float? val = new float?();
            cpu.Update();
            foreach (ISensor sensor in cpu.Sensors)
            {
                if (sensor.SensorType == SensorType.Temperature && sensor.Name.ToLower().Contains("package"))
                {
                    val = sensor.Value;
                    break;
                }
            }

            return val.GetValueOrDefault(0f);
        }
    }
}
