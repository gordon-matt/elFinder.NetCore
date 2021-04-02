using elFinder.NetCore.Test.Automation;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Threading.Tasks;

namespace elFinder.NetCore.Test
{
    [TestFixture]
    public class FileManager_Features
    {
        private FileManagerTest _fileManagerTest;
        private bool _stopped;
        private bool _mustPassCurrent;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _stopped = false;
            _mustPassCurrent = false;

            var config = new ConfigurationBuilder()
                .AddJsonFile(Config.AppSettingFile, optional: false)
                .Build();

            var host = config.GetSection(Config.HostKey).Value;
            var driver = config.GetSection(Config.DriverKey).Value;
            var driverDir = config.GetSection(Config.DriverDirectoryKey).Value;

            IWebDriver webDriver;
            switch (driver)
            {
                case Driver.Chrome:
                    webDriver = new ChromeDriver(driverDir, new ChromeOptions
                    {
                        AcceptInsecureCertificates = true
                    });
                    break;
                default:
                    throw new NotImplementedException();
            }

            _fileManagerTest = new FileManagerTest(webDriver, $"{host}/file-manager");
        }

        [SetUp]
        public void Setup()
        {
            if (_mustPassCurrent && _stopped)
            {
                Assert.Inconclusive("Previous test failed");
            }

            _mustPassCurrent = false;
        }

        [Test]
        [Order(1)]
        public async Task LoadSuccess()
        {
            _mustPassCurrent = true;
            var success = await _fileManagerTest.LoadSuccessAsync();
            Assert.IsTrue(success);
        }

        [Test]
        [Order(2)]
        public async Task CreateNewFolder()
        {
            var success = await _fileManagerTest.CreateNewFolder();
            Assert.IsTrue(success);
        }

        [TearDown]
        public void TearDown()
        {
            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed && _mustPassCurrent)
            {
                _stopped = true;
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (_fileManagerTest != null)
                _fileManagerTest.Dispose();
        }
    }
}