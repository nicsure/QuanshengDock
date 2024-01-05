using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanshengDock.Audio
{
    public static class AudioDevices
    {
        private static readonly Dictionary<string, string> fullDeviceNames = new();

        static AudioDevices()
        {
            UpdateFullDeviceNames();
        }

        private static void UpdateFullDeviceNames()
        {
            fullDeviceNames.Clear();
            using MMDeviceEnumerator enumerator = new();
            foreach (var device in enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active))
                fullDeviceNames[device.FriendlyName.Length > 31 ? device.FriendlyName[..31] : device.FriendlyName] = device.FriendlyName;
        }

        public static string[] GetAudioInDevices()
        {
            string[] audioInDevices = new string[WaveIn.DeviceCount];
            for (int i = 0; i < WaveIn.DeviceCount; i++)
                audioInDevices[i] = fullDeviceNames.TryGetValue(WaveIn.GetCapabilities(i).ProductName, out string? s) ? s : WaveIn.GetCapabilities(i).ProductName;
            return audioInDevices;
        }

        public static string[] GetAudioOutDevices()
        {
            string[] audioOutDevices = new string[WaveOut.DeviceCount];
            for (int i = 0; i < WaveOut.DeviceCount; i++)
                audioOutDevices[i] = fullDeviceNames.TryGetValue(WaveOut.GetCapabilities(i).ProductName, out string? s) ? s : WaveOut.GetCapabilities(i).ProductName;
            return audioOutDevices;
        }

    }
}
