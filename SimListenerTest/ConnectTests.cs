using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimListener;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace SimListener.Tests
{
    [TestClass()]
    public class ConnectTests
    {
        [TestMethod()]
        public void AddRequestsTest1()
        {
            Connect cnx = new Connect();

            ErrorCodes answer = cnx.AddRequest("SHOULD FAIL");
            Assert.AreEqual(ErrorCodes.INVALID_DATA_REQUEST, answer);
        }

        [TestMethod()]
        public void AddRequestsTest2()
        {
            Connect cnx = new Connect();

            foreach (string name in SimVars.Names)
            {
                ErrorCodes answer = cnx.AddRequest(name);
                Assert.AreEqual(ErrorCodes.OK, answer);
            }
        }

        [TestMethod()]
        public void AddRequestsTest3()
        {
            Connect cnx = new Connect();

            string answer = cnx.AddRequests(SimVars.Names.ToList<string>());

            Assert.AreEqual("OK", answer);
        }

        [TestMethod()]
        public void AddRequestsTest4()
        {
            Connect cnx = new Connect();

            List<string> Test = new List<string>() { "WONT WORK" };

            string answer = cnx.AddRequests(Test);

            Assert.AreEqual("WONT WORK", answer);
        }
    }
}