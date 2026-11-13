# Breakpoint schedules

Breakpoint is a separate input mode alongside Classic, T, and Lag. It implements a progressive fixed-ratio requirement, not an inferred inactivity stopping rule.

## Configuration

Select **Breakpoint Schedules**, then **Arithmetic** or **Geometric**. Set the starting ratio (positive integer) and the increment or multiplier. The preview shows the first six requirements.

- Arithmetic: next requirement = previous requirement + whole-number increment (at least 1). Example: start 2, increment 3 gives 2, 5, 8, 11.
- Geometric: next requirement = ceiling(previous integer requirement × multiplier), where the multiplier is greater than 1. The editor supports four decimal places and its tooltip explains that each fractional result rounds **up**, then becomes the basis for the next step. Example: start 2, factor 1.5 gives 2, 3, 5, 8, 12, 18.
- Response feedback, reinforcement magnitude/probability and reinforcement delay remain available. Breakpoint is not a Classic chain; it cannot be combined with Lag or Yoked mode in one component.

## Execution

Each input and chamber has independent state. Reaching the current requirement completes one ratio and invokes the programmed reinforcement delay, if any. Responses during the delay do not count toward a second ratio. A successful scheduled delivery batch advances the requirement once, even if its magnitude contains multiple units. If the whole batch is omitted by delivery probability, the same requirement is presented again. Manual and independent-time deliveries do not advance Breakpoint.

Every component presentation starts over at the configured initial requirement. Partial ratios and pending deliveries are cancelled at component end; they are not carried into the next presentation. The usual duration and maximum-reinforcer limits still apply. There is no new inactivity timer. A response or delayed callback arriving after the component deadline cannot deliver reinforcement or alter the completed record.

Timeout feedback and delayed retraction respect each other. End-of-timeout cannot extend an input still retracted for a pending delay. ICI retraction is preserved if a late delay callback triggers component closure.

## Records and compatibility

Raw events identify Breakpoint start, requirement, completed ratio, delivery outcome and stop/cancellation, together with component, presentation and input. The summary reports completed/reinforced ratios, largest completed/reinforced requirement and the final partial ratio. These are descriptive measures; the largest requirement is not labeled as a measured inactivity breakpoint.

New JSON setups include explicit per-input Breakpoint definitions. Legacy setups normalize this field to an empty six-input array. Saved definitions are deep-copied into each session. Invalid names, missing fields, invalid values and unsupported combinations are rejected before starting.

The response requirement is limited to Int32 (2,147,483,647). Overflow stops the session rather than wrapping, saturating or silently changing the experimental procedure. This numerical limit is not a normal stopping criterion.

## Validation

`BreakpointTests/BreakpointTests.vbproj` exercises progression, decimal ceiling, six inputs, independent chambers, repeated components, omissions, magnitude, manual deliveries, delay/TO/ICI deadline boundaries, JSON round trips and legacy loading. Automated validation uses the application's simulated transport. It does not certify physical lever movement, pump operation or reinforcer consumption.
