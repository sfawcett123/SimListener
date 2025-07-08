using SimListener;

namespace SimListenerTest
{
    [TestClass()]
    public class StaticTests
    {
        [DataTestMethod]
        [DataRow("PLANE ALTITUDE")]
        [DataRow("PLANE ALTITUDE:1")]
        [DataRow("PLANE ALTITUDE:2")]
        public void CheckValidate(string validRequest)
        {
            // Arrange
            // Act & Assert
            Assert.IsTrue(Connect.ValidateRequest(validRequest), "The request should be valid.");
        }

        [DataTestMethod]
        [DataRow("PLANE THINGS")]
        [DataRow("PLANE ALTITUDE:-1")]
        [DataRow("PLANE ALTITUDE:0")]
        [DataRow("PLANE ALTITUDE:11")]
        public void CheckFailsValidate(string validRequest)
        {
            // Arrange
            // Act & Assert
            Assert.IsFalse(Connect.ValidateRequest(validRequest), "The request should not be valid.");
        }
    }
}
