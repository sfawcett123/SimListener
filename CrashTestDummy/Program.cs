using SimListener;
using System.Diagnostics;
using System;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Logging;

class Example
{
    static Connect cnx = new(0, 2000);

    static void Main()
    {
        // This is a test program to connect to the simulator and receive data.
        Console.WriteLine("Starting Example Program.");
              
        cnx.SetLogLevel(LogLevel.Debug);

        Console.WriteLine("Adding SimConnected Event...");
        cnx.SimConnected += c_SimConnected;

        Console.WriteLine("Adding SimDataRecieved Event...");
        cnx.SimDataRecieved += c_SimDataRecieved;

        Console.WriteLine("In Loop Forever. ");
        for (; ; );
    }

    static void c_SimConnected(object? sender, EventArgs e)
    {
        Console.WriteLine($"Simulator Connected.");
        if (sender == null) return;

        List<string> Data = new List<string>() { "PLANE ALTITUDE", "PLANE LATITUDE" , "TURB ENG N1", "TURB ENG N2" };
        cnx.AddRequests(Data);
    }

    static void c_SimDataRecieved(object? sender, SimulatorData e)
    {
        Console.WriteLine($"Simulator Data Recieved. {e.TimeReached} ");
        foreach (var item in e.AircraftData)
        {
            foreach (var kvp in item)
            {
                Console.WriteLine($"Connected is {cnx.Connected} : {kvp.Key}: {kvp.Value}");
            }
        }

    }
}









