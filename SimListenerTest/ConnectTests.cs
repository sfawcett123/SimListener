using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimListener;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SimListener.Tests
{
    [TestClass()]
    public class ConnectTests
    {
        [TestMethod()]
        public void AddRequestsTest1()
        {
            Connect cnx = new();

            ErrorCodes answer = cnx.AddRequest("SHOULD FAIL");
            Assert.AreEqual(ErrorCodes.INVALID_DATA_REQUEST, answer);
        }

        [TestMethod()]
        public void AddRequestsTest2()
        {
            Connect cnx = new();

            foreach (string name in SimVars.Names)
            {
                ErrorCodes answer = cnx.AddRequest(name);
                Assert.AreEqual(ErrorCodes.OK, answer);
            }
        }

        [TestMethod()]
        public void AddRequestsTest3()
        {
            Connect cnx = new();

            string answer = cnx.AddRequests(SimVars.Names.ToList<string>());

            Assert.AreEqual("OK", answer);
        }

        [TestMethod()]
        public void AddRequestsTest4()
        {
            Connect cnx = new();

            List<string> Test = new() { "WONT WORK" };

            string answer = cnx.AddRequests(Test);

            Assert.AreEqual("WONT WORK", answer);
        }

        [TestMethod()]
        public void AddRequestsTest5()
        {
            Connect cnx = new();

            KeyValuePair<string, string> Data = new KeyValuePair<string, string>("PLANE ALTITUDE", "");

            List<string> Test = new() { Data.Key };

            string answer = cnx.AddRequests(Test);

            foreach (var req in cnx.AircraftData())
            {
                Console.WriteLine($"OUTPUT -> {req}");
            }

            Assert.IsTrue(cnx.AircraftData().Contains(Data));
        }

        [TestMethod()]
        public void ConnectedTest1()
        {
            Connect cnx = new();
            

            Process[] processes = Process.GetProcessesByName("FlightSimulator");
            if (processes.Length == 0)
            {
                Console.Write( "Flight Simulator is not Running");
                Assert.IsTrue(true);
                return;
            }

            cnx.ConnectToSim();

            Console.WriteLine($"Connection is {cnx.Connected}");

            if (!cnx.Connected)
            {
                // Sim Connect has not connected.
                Assert.IsTrue(false);
                return;
            }

            KeyValuePair<string, string> Data = new KeyValuePair<string, string>("PLANE LATITUDE", "");
            List<string> Test = new() { Data.Key };
            string answer = cnx.AddRequests(Test);

            foreach (var i in Enumerable.Range(0, 60 ))
            {
                foreach (var req in cnx.AircraftData())
                {
                    if( req.Key == Data.Key && req.Value != "" )
                    {
                        // If we find data we can pass the test
                        Console.WriteLine($"Output -> {req}");
                        Assert.IsTrue( true );
                        return;
                    }
                }
                Thread.Sleep(1000);
            }

            // If after a minute we find no data connection isn't working
            Assert.IsTrue(false);
        }
    }
}