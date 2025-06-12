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

            cnx.AddRequest("SHOULD FAIL");

        }
    }
}