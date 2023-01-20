using SimListener;
using System.Diagnostics;

Connect cnx = new();

KeyValuePair<string, string> Data = new("PLANE LATITUDE", "");
List<string> Test = new() { Data.Key };

foreach (var i in Enumerable.Range(0, 60000))
{
    
    if (!cnx.Connected)
    {
        cnx.ConnectToSim();
        Console.WriteLine($"Connection is {cnx.Connected}");
    }

    if (cnx.Connected)
    {
        cnx.AddRequests(Test);

        foreach (var req in cnx.AircraftData())
        {
            if (req.Key == Data.Key && req.Value != "")
            {
                Console.WriteLine($"Output -> {req}");
            }
        }
    }
    Thread.Sleep(1000);
}
