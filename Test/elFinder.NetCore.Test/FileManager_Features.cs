using elFinder.NetCore.Test.Automation;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace elFinder.NetCore.Test
{
    [TestFixture]
    public class FileManager_Features
    {
        private FileManagerTest _fileManagerTest;
        private bool _stopped;
        private bool _prerequisite;
        private IDictionary<string, bool> _result;
        private IDictionary<string, List<string>> _dependencies;

        public FileManager_Features()
        {
            _result = new Dictionary<string, bool>();
            _dependencies = new Dictionary<string, List<string>>();
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _stopped = false;
            _prerequisite = false;

            var config = new ConfigurationBuilder()
                .AddJsonFile(Config.AppSettingFile, optional: false)
                .Build();

            var baseUrl = config.GetSection(Config.BaseUrlKey).Value;
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

            _fileManagerTest = new FileManagerTest(webDriver, $"{baseUrl}/file-manager");

            #region Dependencies
            _dependencies[nameof(Rename)] = new List<string>()
            {
                nameof(CreateNewFolder)
            };
            _dependencies[nameof(DeleteFolder)] = new List<string>()
            {
                nameof(Rename)
            };
            #endregion
        }

        [SetUp]
        public void Setup()
        {
            if (_prerequisite && _stopped)
            {
                Assert.Inconclusive("Prerequisite failed");
            }

            var methodName = TestContext.CurrentContext.Test.MethodName;

            if (_dependencies.ContainsKey(methodName))
                foreach (var dep in _dependencies[methodName])
                {
                    if (!_result[dep])
                        Assert.Inconclusive("Dependency failed");
                }

            _prerequisite = false;
        }

        [Test]
        [Order(1)]
        public async Task LoadSuccess()
        {
            _prerequisite = true;
            var success = await _fileManagerTest.LoadSuccessAsync();
            Assert.IsTrue(success);
        }

        [Test]
        [TestCase("Test Folder")]
        [Order(2)]
        public async Task CreateNewFolder(string folderName)
        {
            var success = await _fileManagerTest.CreateNewFolderAsync(folderName);
            Assert.IsTrue(success);
        }

        [Test]
        [TestCase("Test Folder", "Test Folder (Renamed)")]
        [Order(3)]
        public async Task Rename(string oldFolderName, string newFolderName)
        {
            var success = await _fileManagerTest.RenameAsync(oldFolderName, newFolderName);
            Assert.IsTrue(success);
        }

        [Test]
        [TestCase("Test Folder (Renamed)")]
        [Order(1000)]
        public async Task DeleteFolder(string folderName)
        {
            var success = await _fileManagerTest.DeleteFolderAsync(folderName);
            Assert.IsTrue(success);
        }

        [TearDown]
        public void TearDown()
        {
            var methodName = TestContext.CurrentContext.Test.MethodName;
            _result[methodName] = false;

            switch (TestContext.CurrentContext.Result.Outcome.Status)
            {
                case TestStatus.Failed:
                    if (_prerequisite) _stopped = true;
                    break;
                case TestStatus.Passed:
                    _result[methodName] = true;
                    break;
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