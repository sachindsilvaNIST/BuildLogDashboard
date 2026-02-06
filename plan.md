# App Update Dialog Implementation Plan

## Overview
Replace inline DataGrid editing with modal dialog windows for adding and editing App Updates.

## Changes

### 1. Create `AppUpdateDialog.axaml` + `AppUpdateDialog.axaml.cs`
- New modal window (following AlertDialog/ExportPreviewWindow pattern)
- Fields: App Name, Path, Version, Changes (each as Label + TextBox)
- Footer: **Cancel** (red `#D13438`, same as PDF preview) + **Confirm** or **Modify** (green `#107C10`, same as PDF preview)
- On Confirm/Modify: show Apple-style loading spinner ("Adding App..." or "Modifying App...") for 1 second, then close
- Properties: `IsConfirmed`, `AppName`, `Path`, `Version`, `Changes`
- Two modes: **Add** (empty fields, "Confirm" button) and **Edit** (pre-filled fields, "Modify" button)
- Window: `CenterOwner`, `CanResize=False`, no taskbar, borderless like AlertDialog

### 2. Update `MainWindowViewModel.cs`
- `AddAppUpdate()` → open `AppUpdateDialog` in Add mode, on confirm add the new AppUpdate to collection
- Add `EditAppUpdate(AppUpdate)` command → open `AppUpdateDialog` in Edit mode with pre-filled data, on confirm update the existing object
- Keep `RemoveAppUpdate` as-is (trash button stays)

### 3. Update `MainWindow.axaml` App Updates DataGrid
- Make DataGrid `IsReadOnly="True"` (no inline editing)
- Revert Changes column back to simple `DataGridTextColumn` (remove embedded TextBox+Button template)
- Add separate locked delete column back (trash icon, `Width="40"`, `CanUserResize=False`)
- Add `DoubleTapped` event on DataGrid → calls EditAppUpdate with selected row

### 4. Update `MainWindow.axaml.cs`
- Add `OnAppUpdateDoubleTapped` event handler that invokes the ViewModel's EditAppUpdate command

## Files to create:
- `Views/AppUpdateDialog.axaml`
- `Views/AppUpdateDialog.axaml.cs`

## Files to modify:
- `Views/MainWindow.axaml` (DataGrid changes)
- `Views/MainWindow.axaml.cs` (double-tap handler)
- `ViewModels/MainWindowViewModel.cs` (dialog-based Add/Edit commands)
