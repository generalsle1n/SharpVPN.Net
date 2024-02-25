using System.Diagnostics;
using System.Dynamic;
using System.Net;
using SharpPcap;
using SharpPcap.LibPcap;
using SharpVPN.Net.GW.Network;
using SharpVPN.Net.GW.Network.Entities;

namespace SharpVPN.Net.GW;

public class VirtualInterface : INetworkInterface
{
    public string Name { get; init; }
    public IPAddress IPAddress { get; set; }
    private LibPcapLiveDevice _interface { get; set; }
    private const string LinuxModuleName = "dummy";
    private const string ModProbeName = "modprobe";
    private const string LSModName = "lsmod";
    private const string IPIntProgram = "ip";
    private const string IPIntAddLink = "link add";
    private const string IPIntSetDev = "link set dev";
    private const string IPIntUp = "up";
    private const string IPIntDeleteLink = "link delete";
    private const string IPIntType = "type";
    private const string IPAddrAdd = "addr add";
    private const string IPAddrDevice = "dev";
    public void DisableInterface()
    {
        UnloadModule();
        DeleteInterface();
    }

    public void EnableInterface(Gateway Gateway)
    {
        if (!ModuleIsLoaded())
        {
            LoadModule();
        }
        CreateInterface();

        _interface = LibPcapLiveDeviceList.New().Where(x => x.Name.Equals(Name)).First();
        _interface.OnPacketArrival += Gateway.PacketProcessor;
        _interface.Open();
        _interface.StartCapture();
    }
    private bool ModuleIsLoaded()
    {
        Process Proc = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = LSModName,
                RedirectStandardOutput = true
            }
        };

        Proc.Start();
        Proc.WaitForExit();

        string Content = Proc.StandardOutput.ReadToEnd().ToLower();
        if (Content.Contains(ModProbeName))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void LoadModule()
    {
        Process Proc = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = ModProbeName,
                Arguments = LinuxModuleName
            }
        };
        Proc.Start();
        Proc.WaitForExit();
    }
    private void UnloadModule()
    {
        Process Proc = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = ModProbeName,
                Arguments = $"-r {LinuxModuleName}"
            }
        };
        Proc.Start();
        Proc.WaitForExit();
    }
    public void SetIPAddress(IPAddress IP, string Subnet)
    {
        Process Proc = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = IPIntProgram,
                Arguments = $"{IPAddrAdd} {IPAddress}{Subnet} {IPAddrDevice} {Name}"
            }
        };
        Proc.Start();
        Proc.WaitForExit();
    }
    private void CreateInterface()
    {
        Process Proc = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = IPIntProgram,
                Arguments = $"{IPIntAddLink} {Name} {IPIntType} {LinuxModuleName}"
            }
        };
        Proc.Start();
        Proc.WaitForExit();

        Proc = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = IPIntProgram,
                Arguments = $"{IPIntSetDev} {Name} {IPIntUp}"
            }
        };
        Proc.Start();
        Proc.WaitForExit();
    }
    private void DeleteInterface()
    {
        Process Proc = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = IPIntProgram,
                Arguments = $"{IPIntDeleteLink} {Name} {IPIntType} {LinuxModuleName}"
            }
        };
        Proc.Start();
        Proc.WaitForExit();
    }
}
