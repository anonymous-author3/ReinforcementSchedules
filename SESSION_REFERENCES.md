# Session history, yoking and live Lag views

## Configure references

In Setup, add/load the program, enter the subject for the box, then click **Session references: continue Lag / Yoked...**. Select the target component. Changes are staged until **Apply references**; Cancel leaves the prior configuration intact.

- **Load prior Lag history**: choose a previous session and its source component. When RTDB is connected the picker lists account-scoped sessions and marks the newest exact subject match as a suggestion. It does not choose one automatically. **Choose local file** remains available. In local-only mode an Open File dialog accepts raw TXT, its matching summary TXT, or a Lag sequence CSV.
- **Load Yoked reference**: choose an exact, completed reference from a newly recorded raw/summary session. Different subject names require an explicit confirmation in the picker. The displayed UID/component remains attached to the setup and per-box snapshot.
- History and Yoked are mutually exclusive. Clearing references and applying the dialog resets the selection. Saved setup XML v5 embeds the reference snapshot; running boxes get deep copies, so later edits cannot change their reference mid-session.
- Old XML versions 1–4 remain readable. Old applications reject v5. Setup snapshots are capped at 4 MB; session import files are capped at 32 MB. A setup exceeding its limit is rejected before replacing the old file.

## Prior Lag history

The last N complete sequences from the selected source component seed the new comparison window, where N is the target Lag value. Passing and failing sequences both count. Incomplete sequences never seed history. The new session's completed-sequence counter, bars and transition counts start at zero. Source sequence length and physical input numbers must match the target component. Histories are isolated by chamber and component and remain in memory across repeated presentations in the same session.

## Independent sequence limit

The Lag component editor has **Max sequences / presentation (0 = off)**.

- A positive value counts all complete sequences, regardless of whether reinforcement was delivered. For example, 50 ends the presentation after 50 complete sequences. This takes precedence over MaxRefs.
- Zero disables the sequence limit and retains the existing duration/MaxRefs criteria.
- Duration is always a fallback. A final sequence's programmed reinforcement delay or failure timeout is allowed to finish before a sequence-limit transition, unless duration expires first. Duration can cancel a pending delivery; that cancellation is recorded explicitly.
- The limit resets for each component presentation. The Lag comparison history does not reset between presentations.

## Lag Yoked

Each new complete sequence advances exactly one reference trial. Reference trial outcomes, not the new response pattern or Lag criterion, determine reinforcement. Every recorded Pellet/Liquid command and its order are copied, including magnitudes greater than one and omitted trials. The new completing input receives the attribution.

Current probability, magnitude, delay, failure timeout, MaxRefs and Max sequences settings do not alter a Yoked pattern. Reference timing is not copied for Lag. The configured duration remains a fallback. After the last reference trial, the whole session enters post-session and accepts no more responses, even if other components were configured. The reference never wraps and never falls back to normal Lag. The criterion may still be logged descriptively, separately from the actual Yoked outcome.

## Classic/T Yoked

Reference reinforcement batches become available at their recorded milliseconds from the start of the source component presentation. A response on the same physical input collects the next available batch for that input. One response collects one batch, with its original quantity, type and within-batch order. Uncollected due batches stay available. They are not automatically emitted in a burst merely because the UI timer is late.

Normal ratio/interval/T/independent-time contingencies are disabled for that Yoked component. Probability and delay are not reapplied. MaxRefs does not truncate the reference. Configured duration remains a fallback. Consuming the final reference batch ends the session.

Current supported scope is exactly one source presentation of the selected Classic/T component. Repeated source presentations, including those with no reinforcers, are explicitly rejected rather than flattened or silently skipped. FT/VT events with no response input cannot be used as response-contingent references. Manual reinforcement commands make a reference ineligible for Yoked. Manual reinforcer controls are disabled while a Yoked component is running.

## Exactness and recorded data

Every new raw/summary file has `ReferenceFormat,2`. Each `DeliveredReinforcer` row records session time in milliseconds, component, global presentation, physical input (0 for independent time), concrete Pellet/Liquid type, Lag trial number (0 outside a trial), source, and batch number. There is one row per successfully issued command. Each `LagOutcome` records trial number, actual command count, and Delivered/Omitted/Cancelled status. The ordinary Rn/RT events remain for compatibility, but they are not an exact quantity/type record.

**These are command timestamps, not sensor-confirmed food/liquid delivery times.** The firmware does not return a delivery acknowledgment or a dispenser timestamp. Windows/serial scheduling and feeder operation introduce physical timing limits.

Normal session closure records `ReferenceClosed,Completed`. Active/truncated sessions, error/disposal closures, missing outcomes, malformed metadata, inconsistent counts and partially delivered/cancelled batches cannot be used for Yoked. A cancelled pending delay with an explicit zero delivery count in an otherwise completed session is a recorded nonreinforced trial. Historical Pass/Fail does not establish actual reinforcement: old raw/CSV files can seed Lag history but cannot serve as exact Yoked references.

The ordinary summary now includes every delivery command and Lag trial outcome. The additional `_LagSequences.csv` remains the ordered sequence/frequency report; its Pass/Fail column still means criterion, not reinforcement. Use raw/summary for exact Yoked replay.

RTDB reference browsing is read-only. Future session uploads write a small session index in the existing atomic upload. Older sessions are discovered with shallow ID reads and small metadata reads, not by downloading every chart. The selected raw/summary are downloaded only after selection; a changed account/database invalidates the selection.

## Live Lag views

Lag uses two tabs instead of the cumulative display. Classic/T retain the original cumulative graph, including Yoked reinforcement markers.

- **Sequence frequencies**: one bar per distinct new complete sequence, incremented on every occurrence including failures. The display shows up to 40 categories at a time with horizontal scrolling; the underlying counts and CSV are not truncated. Order is first observed. Counts are separate per component/chamber and retained between presentations.
- **Response transitions**: an empirical directed diagram. Arrows show P(next input | current input) and observed numerator/denominator. Nodes show marginal response proportions and counts. Selecting a node emphasizes its outgoing arrows. A node with no outgoing observations has no defined conditional distribution; no zero-division or invented probabilities are shown.
- Toggle off: transitions are counted only inside completed sequences. No transitions join different trials; partial sequences, delay, timeout and prior-session history are excluded.
- Toggle on: all responses on configured Lag inputs are included, including partial sequences, between-trial transitions, delay and timeout responses. ICI, inactive inputs, other components and links across presentation boundaries are excluded. Both modes retain separate data so switching does not discard observations. Frequency bars remain complete-sequence counts in both modes.
- These are descriptive observed probabilities, not fitted predictions or evidence that behavior is a Markov process.

When a session ends in Lag, chart export saves the selected view as the primary image and separate `_LagBars.png` and `_LagTransitions.png` images. The transition image identifies the selected mode.

## Validation and safety

All verification uses synthetic sessions, fake HTTP or UI harnesses. No test starts a real COM connection, drives an experimental chamber, or writes to a real RTDB. The regular Debug/Release executables are not replaced; the candidate is built into `bin/ReferenceValidation`.

Suites: ReferenceTests (parser, fake cloud, integrated runtime and staged Apply/Cancel UI), LagChartTests (two transition modes, rendering/export, scrolling, 50,000 sequences and isolation), existing LagRuntimeTests/LagIndependentTests/LagEditorTests, IntervalTests and StorageTests. See test output for the final counts and executable hash.

Before an animal session, validate the final executable on an empty chamber, including physical input numbering, output polarity, feeder quantity and timing. The software tests do not establish physical apparatus performance.
