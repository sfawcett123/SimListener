using SimListener;
using System.Diagnostics;

Connect cnx = new();

KeyValuePair<string, string> Data = new("PLANE LATITUDE", "");
List<string> Test = new() { Data.Key };

bool requested = false;

foreach (var i in Enumerable.Range(0, 60000))
{
    
    if (!cnx.Connected)
    {
        requested = false;
        cnx.ConnectToSim();
        Console.WriteLine($"Connection is {cnx.Connected}");
    }

    if (cnx.Connected)
    {
        if (! requested)
        {
            cnx.AddRequests(Test);
            requested = true;
        }

        foreach (var req in cnx.AircraftData())
        {
            if (req.Key == Data.Key && req.Value != "")
            {
                Console.WriteLine($"Output -> {req.Key} = {req.Value}");
            }
        }
    }
    Thread.Sleep(1000);
}
