using SimListener;
using System.Diagnostics;
using System;
using System.Threading.Tasks;
using System.Timers;

class Example
{
    static void Main()
    {
        // This is a test program to connect to the simulator and receive data.
        Connect cnx = new(); 
        
        Console.WriteLine("In Loop Forever. ");
        cnx.SimConnected += c_SimConnected;
        cnx.SimDataRecieved += c_SimDataRecieved;
        for (; ; );
    }

    static void c_SimConnected(object? sender, EventArgs e)
    {
        Console.WriteLine($"Simulator Connected.");
        if (sender == null) return;

        Connect cnx = (Connect)sender;
        List<string> Data = new List<string>() { "PLANE ALTITUDE", "PLANE LATITUDE" };
        cnx.AddRequests(Data);
    }

    static void c_SimDataRecieved(object? sender, SimulatorData e)
    {
        Console.WriteLine($"Simulator Data Recieved. {e.TimeReached} ");
        foreach (var item in e.AircraftData)
        {
            foreach (var kvp in item)
            {
                Console.WriteLine($"{kvp.Key}: {kvp.Value}");
            }
        }

    }
}









