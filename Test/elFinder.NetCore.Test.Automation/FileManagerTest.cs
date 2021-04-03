using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace elFinder.NetCore.Test.Automation
{
    public class FileManagerTest : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly string _fileManagerUrl;
        private bool disposedValue;

        public FileManagerTest(IWebDriver driver, string fileManagerUrl)
        {
            _driver = driver;
            _fileManagerUrl = fileManagerUrl;
        }

        public Task<bool> LoadSuccessAsync()
        {
            _driver.Navigate().GoToUrl(_fileManagerUrl);

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            var loaded = wait.Until(webDriver =>
            {
                return (webDriver as IJavaScriptExecutor)
                    .ExecuteScript("return document.readyState") as string == "complete";
            });

            try
            {
                var errDialog = _driver.FindElement(
                    By.CssSelector(".elfinder-dialog-error"));

                var isVisible = errDialog.Displayed;

                return Task.FromResult(!isVisible);
            }
            catch (NoSuchElementException)
            {
                return Task.FromResult(true);
            }
        }

        public Task<bool> CreateNewFolderAsync(string folderName)
        {
            var elFinder = ElementVisible(".elfinder-cwd-wrapper");

            var rightClickElfinder = new Actions(_driver).ContextClick(elFinder);
            rightClickElfinder.Perform();
            Wait(1);

            var newFolderOption = ElementsVisible(".elfinder-contextmenu-item span",
                filter: ele => ele.GetAttribute("innerHTML").Contains("New folder")).FirstOrDefault();
            var clickNewFolder = new Actions(_driver).Click(newFolderOption);
            clickNewFolder.Perform();
            Wait(1);

            var inputName = ElementVisible(".elfinder-cwd-file textarea");
            inputName.SendKeys(folderName);
            Wait(1);

            var clickElfinder = new Actions(_driver).Click(elFinder);
            clickElfinder.Perform();
            Wait(1);

            var result = ElementVisible(".toast-success", 3);
            Wait(2);

            return Task.FromResult(true);
        }

        public Task<bool> RenameAsync(string oldFolderName, string newFolderName)
        {
            var testFolder = ElementsVisible(".elfinder-cwd-file", filter:
                ele => ele.GetAttribute("innerHTML").Contains(oldFolderName)).FirstOrDefault();

            var rightClickTestFolder = new Actions(_driver).ContextClick(testFolder);
            rightClickTestFolder.Perform();
            Wait(1);

            var renameOption = ElementsVisible(".elfinder-contextmenu-item span",
                filter: ele => ele.GetAttribute("innerHTML").Contains("Rename")).FirstOrDefault();
            var clickRename = new Actions(_driver).Click(renameOption);
            clickRename.Perform();
            Wait(1);

            var inputName = ElementVisible(".elfinder-cwd-file textarea");
            inputName.SendKeys(newFolderName);
            Wait(1);

            var clickElfinder = new Actions(_driver).Click(testFolder);
            clickElfinder.Perform();
            Wait(1);

            try
            {
                var result = ElementVisible(".elfinder-dialog-error", 1);
                return Task.FromResult(false);
            }
            catch (Exception)
            {
                return Task.FromResult(true);
            }
        }

        public Task<bool> DeleteFolderAsync(string folderName)
        {
            var testFolder = ElementsVisible(".elfinder-cwd-file", filter:
                ele => ele.GetAttribute("innerHTML").Contains(folderName)).FirstOrDefault();

            var rightClickTestFolder = new Actions(_driver).ContextClick(testFolder);
            rightClickTestFolder.Perform();
            Wait(1);

            var renameOption = ElementsVisible(".elfinder-contextmenu-item span",
                filter: ele => ele.GetAttribute("innerHTML").Contains("Delete")).FirstOrDefault();
            var clickRename = new Actions(_driver).Click(renameOption);
            clickRename.Perform();
            Wait(1);

            var acceptBtn = ElementVisible(".elfinder-confirm-accept", 2);
            acceptBtn.Click();
            Wait(1);

            try
            {
                var result = ElementVisible(".elfinder-dialog-error", 1);
                return Task.FromResult(false);
            }
            catch (Exception)
            {
                return Task.FromResult(true);
            }
        }

        private void Wait(double sec)
        {
            var calledTime = DateTime.Now;
            var waitSec = TimeSpan.FromSeconds(sec);
            new WebDriverWait(_driver, TimeSpan.FromSeconds(sec + 1))
                .Until(dv => DateTime.Now - calledTime > waitSec);
        }

        private IWebElement ElementVisible(string cssSelector, double? sec = null)
        {
            return ElementWait(sec).Until(ExpectedConditions.ElementIsVisible(By.CssSelector(cssSelector)));
        }

        private IEnumerable<IWebElement> ElementsVisible(string cssSelector, Func<IWebElement, bool> filter = null, double? sec = null)
        {
            return ElementWait(sec).Until(dv =>
            {
                IEnumerable<IWebElement> elements = dv.FindElements(
                    By.CssSelector(cssSelector)).ToList();
                if (filter != null) elements = elements.Where(filter);
                return elements.Any() ? elements : null;
            });
        }

        private WebDriverWait ElementWait(double? sec = null)
        {
            var wait = new WebDriverWait(_driver, sec.HasValue ? TimeSpan.FromSeconds(sec.Value) : TimeSpan.FromSeconds(5));
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
            return wait;
        }

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null

                _driver.Close();
                _driver.Dispose();

                disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~FileManagerTest()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
