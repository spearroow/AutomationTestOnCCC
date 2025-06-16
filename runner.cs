using NUnit.Framework;

namespace WebTestSuite.Tests
{
    [TestFixture]
    public class TestRunner
    {
        [OneTimeSetUp]
        public void GlobalSetup()
        {
            Console.WriteLine("Web Test Suite Execution Started");
            Console.WriteLine($"Test Execution Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Environment: {Environment.OSVersion}");
            Console.WriteLine($".NET Version: {Environment.Version}");
        }

        [OneTimeTearDown]
        public void GlobalTeardown()
        {
            Console.WriteLine("Web Test Suite Execution Completed");
            Console.WriteLine($"Completion Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
        }

        [Test]
        [Description("Test suite health check")]
        public void TestSuite_HealthCheck()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Environment.Version, Is.Not.Null, "Runtime should be available");
                Assert.That(Directory.GetCurrentDirectory(), Does.Exist, "Working directory should exist");
                Assert.That(File.Exists("appsettings.json"), Is.True, "Configuration file should exist");
            });

            Console.WriteLine("Test suite health check passed successfully");
        }
    }
}