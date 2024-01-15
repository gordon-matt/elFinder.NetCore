# Run Notes

Migrated to .net 8.
On Visual Studio 2022 works well.

## Note for Brave Browser

Visual Studio Debugger closes on file upload/download when debugging using Brave Browser.
Solution is to run without debug or to disable VS setting "Stop debugger when browser window is closed..."
This is not project problem, it's common for Visual Studio + Brave Browser.
More details: [Github issue Visual Studio Debugger closes on file upload](https://github.com/brave/brave-browser/issues/21364#issuecomment-1892627745)

