using System.ComponentModel;
using SimListener;

namespace SimListenerTest
{
    [TestClass()]
    public class ConnectTests
    {
        [TestMethod()]
        public void CheckInvalidRequest()
        {
            Connect cnx = new();
            Assert.ThrowsExactly<InvalidSimDataRequestException>(() => cnx.AddRequest("SHOULD FAIL"));
        }

        [TestMethod()]
        [Category("SimRunning")]
        public void CheckAddingSingleRequest()
        {
            Connect cnx = new();
            string request = "PLANE ALTITUDE";
            cnx.AddRequest(request);
            Assert.IsTrue(cnx.listRequests().Contains(request), "The request was not added successfully.");
        }

        [TestMethod()]
        [Category("SimRunning")]
        public void CheckAddingListOfRequests()
        {
            Connect cnx = new();
            List<string> requests = new() { "PLANE ALTITUDE", "PLANE LATITUDE" };
            cnx.AddRequests(requests);
            foreach (string request in requests)
            {
                Assert.IsTrue(cnx.listRequests().Contains(request), "The request was not added successfully.");
            }
        }
    }
}