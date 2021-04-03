## Instructions
1. Download a compatible Chrome driver from: [Downloads - ChromeDriver - WebDriver for Chrome (chromium.org)](https://chromedriver.chromium.org/downloads)
2. Place the **chromedriver.exe** inside the **elFinder.NetCore.Test.Automation** project. *Note: if you place the exe file somewhere else, please update the "DriverDirectory" value inside **appsettings.json** in the test project.*
3. Update the "BaseUrl" value inside **appsettings.json** to your startup web url.
4. Run the demo website.
5. In VisualStudio Solution Explorer, right click on the **elFinder.NetCore.Test** project, choose "Run Tests". That's it.

## Note
Current test cases:
1. Load elFinder file manager.
2. Create a new folder
3. Rename folder
4. Delete folder

Current supported drivers:
+ Chrome driver

## About

These tests use NUnit and Selenium.NET to automate the process.
+ About NUnit: [NUnit.org](https://nunit.org/)
+ About Selenium: [SeleniumHQ Browser Automation](https://www.selenium.dev/)
+ Download Selenium: [Downloads (selenium.dev)](https://www.selenium.dev/downloads/)
