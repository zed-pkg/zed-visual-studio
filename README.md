# zed-visual-studio

Visual Studio integration work for Zed Package Manager state, diagnostics, and confirmation-gated recommended actions.

The dedicated repository now contains:

- a .NET 8 process adapter using `ProcessStartInfo.ArgumentList`, timeout/cancellation, injected runners, schema validation, redaction, and unsafe-action rejection;
- a multi-root tool-window projection with error/warning counts;
- Error List projection records with file-specific resources and `.zpkg.toml` fallback entries;
- confirmation-gated command previews carrying exact executable, argv, and working directory;
- xUnit coverage and Windows CI.

```powershell
dotnet test Zed.VisualStudio.sln --configuration Release
```

Remaining native work is the Visual Studio SDK `AsyncPackage`, WPF tool window, solution/workspace listeners, Error List UI wiring, experimental-instance tests, and signed VSIX packaging.
