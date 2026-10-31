# Security Log

## 1.7.0 — elFinder client 2.1.70

The demo project ships the elFinder client **2.1.70** (via `libman.json`). Upstream strongly recommends not running 2.1.69 or earlier on public servers, so keep the client up to date.

The connector applies the following protections by default:

- **Extract**: ZIP entries that resolve outside the destination directory (Zip-Slip) are skipped and reported back in the `warning` field; extracted files obey the volume's `UploadAllow` / `UploadDeny` / `UploadOrder` rules.
- **Archive**: the requested archive name is sanitized and both the archive and its targets must live inside the volume root.
- **put with `encoding=scheme`** (saving content fetched from a URL): disabled by default to prevent SSRF. To opt in, add trusted hosts to the connector:

  ```csharp
  var connector = new Connector(driver);
  connector.AllowedPutSchemeHosts.Add("cdn.example.com"); // http/https only
  connector.MaxPutSchemeBytes = 5 * 1024 * 1024;          // optional, default 10 MB
  ```

- **file**: HTML, XHTML, SVG, XML and JavaScript files are served as `application/octet-stream` attachments (never rendered inline from the connector's origin), and `X-Content-Type-Options: nosniff` is set on every file response.
- **resize / crop / rotate**: numeric parameters are validated and invalid input returns `errCmdParams`.
- **Options.disabled**: now also lists `netunmount` and `restore`, which are not implemented by the .NET backend.


## Applying to another web project (checklist)

Use this section when porting the 1.7.0 hardening to a web project that lives in a different repository/solution and references a copy of `elFinder.NetCore` via `ProjectReference`. Source of truth: `https://github.com/utilsites/elFinder.NetCore` (branch `master`, 1.7.0).

### A. Core library copy (`elFinder.NetCore` folder in the target solution)

1. Check the copy for local modifications first (`git diff -- <copy-folder>`); if there are none, overwrite these files from the source repo:
   - `Connector.cs`
   - `Drivers/FileSystem/FileSystemDriver.cs`
   - `Models/Commands/AddResponseModel.cs`
   - `Models/Commands/Open/Options.cs`
   - `elFinder.NetCore.csproj` — only if the target framework matches (`net10.0`); otherwise keep the local csproj and just bump `<Version>` to `1.7.0`.
2. Alternative: apply the git patch `elfinder-core-hardening.patch` from inside the copy folder (`git apply --check` first, then `git apply`).
3. Build the solution.

### B. Web project — client assets (`libman.json`)

1. Change the elFinder library entry to `"library": "elfinder@2.1.70"` (provider `cdnjs`, destination usually `wwwroot/lib/elfinder`).
2. Delete the existing `wwwroot/lib/elfinder` folder and run **Restore Client-Side Libraries** (or `libman restore`).
3. Verify the headers of `wwwroot/lib/elfinder/js/elfinder.full.js` / `elfinder.min.js` / `css/elfinder.full.css` read `Version 2.1.70`.
4. cdnjs does not ship `sounds/`; if removal sound is wanted, copy `sounds/rm.wav` from the source repo (`elFinder.NetCore.Web/wwwroot/lib/elfinder/sounds/`).

### C. Web project — elFinder client initialization (every view/page that calls `.elfinder(options)`)

Replace the legacy command-filtering pattern, which is a no-op because `elFinder.prototype._options.commands` is `['*']`:

```javascript
// OLD (does nothing)
var myCommands = elFinder.prototype._options.commands;
$.each(disabled, function (i, cmd) {
    (idx = $.inArray(cmd, myCommands)) !== -1 && myCommands.splice(idx, 1);
});
```

with:

```javascript
// NEW
var disabled = ['callback', 'chmod', 'editor', 'netmount', 'netunmount', 'ping', 'help', 'restore']; // not implemented by elFinder.NetCore
var myCommands = $.grep(Object.keys(elFinder.prototype.commands), function (cmd) {
    return $.inArray(cmd, disabled) === -1;
});
```

Keep passing `commands: myCommands` in the options. Keep any project-specific entries in `disabled` (e.g. `extract`, `archive`, `resize`, `select`).

### D. Web project — connector wiring (controller/page that does `new Connector(driver)`)

- No change required: remote fetch for `put` with `encoding=scheme` is now disabled by default.
- Only if the project needs to save content fetched from a URL (e.g. online editors), opt in:

  ```csharp
  connector.AllowedPutSchemeHosts.Add("trusted-host.example.com");
  // connector.MaxPutSchemeBytes = 5 * 1024 * 1024; // optional
  ```

### E. Smoke tests (against the running web project; `<base>` = connector URL)

1. `GET <base>?cmd=open&init=1&tree=1` → `options.disabled` contains `netunmount` and `restore`.
2. `POST <base>` with `cmd=put&target=<hash>&content=http://127.0.0.1/&encoding=scheme` → `{"error":"errAccess"}`.
3. `GET <base>?cmd=resize&target=<hash>&mode=resize&width=abc&height=10` → `{"error":["errCmdParams","resize"]}`.
4. `GET <base>?cmd=archive&target=<dir>&targets[]=<hash>&name=..%5C..%5Cevil&type=application/zip` → archive created **inside** the volume root as `evil.zip`.
5. Upload a zip containing `../../evil.txt` and `ok.txt`, then `cmd=extract` → response has `warning` for the traversal entry, only `ok.txt` extracted.
6. `GET <base>?cmd=file&target=<hash-of-.html-or-.svg>` → `Content-Type: application/octet-stream`, `Content-Disposition: attachment`, `X-Content-Type-Options: nosniff`; a `.txt` is still served inline.
7. Browse / upload / mkdir / rename / duplicate / rm / quicklook work normally in the UI.