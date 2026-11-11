# Lag schedules

See `SESSION_REFERENCES.md` for prior-session history, independent sequence limits, Yoked replay, the live frequency chart and transition-mode selector.

## Configure a component

1. In **Add component**, choose **Lag Schedules** as the third schedule mode, before adding any Classic/T input.
2. Check at least two physical inputs. Input 1 corresponds to Arduino pin 2, through input 6 / pin 7. Unchecked inputs are N/A. Nonconsecutive selections retain their actual physical input numbers.
3. Set **Responses / sequence** and **Lag (previous sequences)**. The count next to the inputs updates immediately as inputs or sequence length change: participating inputs raised to responses per sequence. Large counts use approximate scientific notation without overflow. Lag 0 accepts every complete sequence.
4. Choose **Omit reinforcer** or a **Time Out** duration for repeated sequences. With Time Out selected, check **Tono** to keep the tone on for that same duration. It is off by default and unavailable when omitting the reinforcer.
5. Configure the shared reinforcer and optional reinforcement delay. Use the input selector inside **Response feedback** to configure every active physical input's outputs and duration independently. Inactive inputs are omitted from this selector; temporarily unchecking and rechecking an input preserves its settings.
6. **Apply Lag inputs** previews the configuration and can be repeated after edits. **Submit** reapplies the latest controls and adds the component. The schedule family is locked after inputs are added to avoid mixing a shared Lag contingency with independent Classic/T schedules.

A response-feedback duration of **0** keeps that input's selected outputs on from its response until the complete sequence ends, whether it passes or fails. A component/session end cancels any incomplete sequence and its feedback. Positive durations remain timed pulses and restart on each response on the same input. Outputs shared with another active pulse, component stimulus, or delay signal stay on until their final owner releases them. Feedback from the final response of a length-1 sequence is released immediately, so it is not a guaranteed visible pulse.

COD and independent FT/VT are disabled in a Lag component. Response-feedback Time Out is excluded; the dedicated failure Time Out controls the consequence for a repeated sequence.

## Sequence rules

- Each complete sequence is compared with up to the last `Lag` complete sequences **before** the current sequence enters the history.
- Both passing and failing complete sequences enter the moving history. Earlier sequences expire once outside that window.
- Example: length 4 and Lag 5 compare `1122` with the previous five complete sequences. A cycle of six distinct sequences can continue to pass.
- Each component has its own history, retained when that component returns within the same session. Separate sessions and boxes start independent histories unless a prior-session history is explicitly selected.
- An incomplete sequence is discarded when its component or session ends, but all individual responses and the discarded partial sequence remain in raw data.
- During reinforcement delay and failure Time Out, responses are logged but do not form a new sequence. Time Out does not restart when responses occur.
- Time Out retracts inputs and switches off chamber stimuli, except its optional **Tono**. The continuous tone uses the existing Arduino T/t command mapping (1500 Hz in the bundled firmware), not the 250 ms tone pulse. It ends after the configured TO duration, or is cancelled immediately when the component/session ends. A component tone is restored without an off pulse when TO ends. The component's duration timer continues during Time Out. Physical timing depends on Windows/serial scheduling; no firmware update is required.
- A qualifying sequence uses the existing delivery probability/magnitude/delay. Passing the Lag criterion is distinct from an actual delivered reinforcer. Delivered reinforcers remain attributed to the physical input completing the sequence.
- Input changes in the same Arduino frame are processed in ascending physical-input order. The existing firmware does not provide timestamps within a frame.

## Data and saved configurations

Raw responses retain their physical input numbers. Additional records:

- `LagSequence`: sequence number, sequence, Pass/Fail, number of preceding sequences compared, configured Lag.
- `LagPartialDiscarded`: incomplete sequence and reason.
- `LagTOStart`, `LagTOEnd`, `LagTOInterrupted:...`.
- `LagTOResponseN`, `LagDelayResponseN`, `LagInactiveResponseN`, `LagBoundaryResponseN`: physical input N, recorded without adding to the sequence.

### Additional sequence report

Every session configured with at least one Lag component saves an additional UTF-8 CSV in `C:\Data\Summary`, beside its ordinary summary, using the same session filename stem and the suffix `_LagSequences.csv`. The traditional summary and raw data remain unchanged. This new file stays local; RTDB uploads are unchanged.

The report contains:

- `Sequence` rows for **all completed sequences**, both passing and failing, in their original recorded order. Fields include timestamp in milliseconds, component number/name, presentation number within that component, the component's cumulative sequence number, physical input sequence, Pass/Fail criterion, number of preceding sequences compared, and configured Lag. Every row identifies the session UID, chamber, subject, and session name.
- `Partial` rows in the same ordered list, marked `Incomplete` with the reason they were discarded. They are **never counted** as completed sequences or included in frequencies.
- `FrequencyAll` rows for the whole session, then `FrequencyComponent` rows for each Lag component, sorted from highest to lowest frequency. Ties sort by sequence text. Counts include all complete occurrences and separate Pass and Fail totals. Component sections keep distinct conditions apart, including repeated presentations and different sequence lengths.
- `Totals` for completed, passing, failing and discarded partial sequences. Sessions with no completed sequences still get a valid zero-total report.

`Pass` means the sequence met the Lag criterion; it does not claim a reinforcer was actually delivered. Responses during delay, timeout, inactive inputs or component boundaries remain in raw data and do not become complete sequences. Components without Lag do not contribute sequences.

The `Sequence` column stores the exact digit string. **Import that column as Text in Excel**, especially for long sequences; opening CSV directly can make Excel round numerical strings longer than 15 digits. The companion `SequenceText` column starts with `Inputs: ` and preserves all digits when opening directly. Text fields resembling spreadsheet formulas are prefixed with an apostrophe for safe display; the original raw data is not modified.

The report is reconstructed from the flushed raw log on a separate worker thread, so finishing one chamber does not block acquisition/timers in other chambers. The hub shows `saving Lag report` until it is done. A normal application close allows the worker to finish. Reports are published only after they are complete, and an existing report is not overwritten. Normal finish, session error and control disposal all request export. Forced process termination or power loss can prevent export, but the auto-flushed raw log remains the recovery source. If export fails, the hub exposes the error and, when writable, an `_LagReportError.txt` note is placed beside the raw file; raw data and the traditional summary are retained.

The summary includes completed, passing and failing sequence totals, plus timeout responses per input. Setup XML format is version 5. Versions 1-4 remain readable; absent reference, sequence-limit and timeout-tone fields remain disabled. The optional `LagTimeoutTone` field round-trips in saved XML and is included in configuration snapshots. Supported legacy text protocols without the field still load with the tone off. Older applications do not implement the optional tone field, so use this updated application to run tone-enabled protocols. Applications limited to XML versions 1-4 reject version 5 rather than silently ignoring references.

## Validation

All checks use simulation or fake services, with no experimental hardware or real session data:

- `LagIndependentTests.ps1`: independent moving-window oracle, boundary cases, malformed settings, old XML without Lag fields, v4 round-trip.
- `LagRuntimeTests/LagRuntimeTests.vbproj`: integration tests for response logging, repeated components, partial discard, Time Out/ICI, reinforcement delay and MaxRefs transition, and Classic/T regressions. It also verifies ordered CSV reports, frequencies, partials, repeated/mixed components, sparse inputs, zero sequences, normal/error/disposal closure, export failure recovery, 10,000-response sequences, spreadsheet-safe text, and 50,000 sequences while another simulated chamber remains responsive.
- `LagEditorTests.ps1`: editor mode switching, physical inputs 2/6, per-input feedback, repeated Apply, Submit/Cancel and a rendered screenshot.
- `StorageTests/StorageTests.vbproj`: existing persistence and UI regression suite using fake services.
- `LagTimeoutToneTests.ps1`: hidden editor checks for Tono enablement, repeated Apply, Submit/Cancel, XML v4/v5 without the new field, and legacy text loading after a tone-enabled configuration.
- The runtime suite also verifies continuous TO tone, early/stale ticks, response-independent duration, component tone sharing, ICI ownership, cancellation/error/disposal, sequence/component limits and independent chambers.

Build the application into `bin\LagValidation` and `obj\LagValidation` for these tests. The runtime test project references that isolated build. The ordinary Debug/Release executable folders have not been replaced as part of these test runs. A full original-code/artifact backup was made before changes.

Physical timing, wiring and output polarity must still be checked on an empty chamber before running an experimental session.
