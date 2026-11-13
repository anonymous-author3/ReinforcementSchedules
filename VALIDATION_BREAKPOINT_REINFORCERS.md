# Breakpoint and four-output reinforcement validation

Validated on 2026-11-13 with the .NET Framework 4.7.2 WinForms application. Release and separate validation builds compile without warnings or errors.

## Automated checks

- Breakpoint: 5,055 assertions, including exact Decimal ceiling, arithmetic progression, six inputs, independent chambers, presentation reset, delivery omission/magnitude/manual separation, delay cancellation, TO and ICI retraction, deadline/chart boundaries and JSON compatibility.
- Four reinforcement outputs: 58 assertions, including aliases, categorical probabilities, mappings, runtime commands, independent-time schedules, Yoked replay, cloning, version 10 JSON and sparse RTDB restoration.
- WinForms UI: 37 checks for Breakpoint mode, preview/overflow, decimal input, multiple inputs, delay, Random selections, Lag copies and independent-time settings. Screenshots were visually inspected.
- Existing regression suites: Classic chains 46; variable schedules 31,608; Lag runtime 50,192; storage 92; reference runtime 40; reference parser 38; cloud references 8; Lag chart 27.
- Interval suite: 599 integration/boundary assertions plus exhaustive FI whole-second conversions and VI domain validation (650,243 means / 6,502,430 generated intervals).

An independent review reproduced two interaction bugs and confirmed their fixes: delay expiration at the component deadline no longer reextends an input during ICI, and timeout completion no longer overrides an active delayed-reinforcement retraction. A late response is rejected before it alters the cumulative chart. Noncanonical Breakpoint names and malformed value arrays reject before execution.

A storage assertion was updated to compare normalized JSON data instead of property order. Structural comparison confirmed that the downloaded setup retained every value; JSON key order is not part of the setup semantics. Reference tests now expect format version 10. Legacy setup import and historical Pellet/Liquid references remain tested.

## Limits

All execution tests use simulated transport or fake HTTP. No physical chamber, pump, feeder, lever movement, real Firebase account, or reinforcer consumption was validated. The bundled Arduino sketch remains unchanged and supports only one feeder and one pump. Feeder 2 / Pump 2 require compatible firmware and confirmed wiring before real use. See `DUAL_REINFORCERS.md`.

No commit, push or remote publication was performed as part of this validation. User-owned session data were not edited.

## Re-running the new suites

Build the main project to `bin\BreakpointValidation\` and run `BreakpointTests/BreakpointTests.vbproj`. Build the main project to `bin\ReinforcerValidation\` and run `ReinforcerTests/ReinforcerTests.vbproj`. Both suites use the friend assembly name already allowed by the application, and do not open a real serial port.

## Explicit input deletion and restoration

42 additional WinForms and persistence checks pass. Delete Input confirms before removing configuration and retains physical input numbers. Deleted rows remain reserved, including at the tail. Without a selected deleted row, Add Input appends the next physical number; selecting a deleted row explicitly restores only that slot. At six positions, select a deleted row to add again. Drafts and other inputs are preserved, and Lag maintains its selected physical inputs and independent feedback.

Run InputDeletionTests/InputDeletionTests.ps1 using Windows PowerShell 5.1 (powershell.exe -NoProfile -STA -File ...), not PowerShell 7: the harness loads the .NET Framework WinForms executable. The main script includes the Lag tests. An optional AssemblyPath argument selects a separate validation build; the default is bin/Release/ReinforcementSchedules.exe.
