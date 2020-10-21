# Casemod Temperature Monitor
This is a small and rudimentary project for a display within your computer case that displays CPU and GPU temperature.

## Usage
### General
This Application works by periodically fetching temperatures from the system and sending them to an android device via network.
The android device listens for udp on port 8080 .

### Setup
You need to create a `configuration.json` in the same directory as the executable:
```json
// configuration.json
{
  "device_hostname": "hostname_of_the_android_device",
  "port": 8080,
  "update_in_ms": 1000
}

```


## Pictures
![Finished build](./images/finished.jpg)

## Notices
#### Third party libraries
- MonitorPCModule\MonitorPCModule\external\OpenHardwareMonitorLib.dll : [Website](https://openhardwaremonitor.org/), [Sourcecode](https://github.com/openhardwaremonitor/openhardwaremonitor)

#### License 
This project is licensed under the [MIT license](https://opensource.org/licenses/MIT). 
In addition to that, if you redistribute or modify this code in compiled or non compiled state, you have to link to the origin of the code meaning this repository.
