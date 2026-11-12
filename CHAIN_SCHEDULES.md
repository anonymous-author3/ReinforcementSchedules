# Classic schedule chains

Classic inputs optionally use an ordered chain of FR, VR, FI, VI, FT and VT links. Only completion of the final link requests the configured reinforcer. A response can complete at most one link. FI/VI wait for a response after their interval; FT/VT advance without a response. Chains restart after terminal reinforcement is resolved, unless component duration or delivered-reinforcer limits end the component first. Component transitions discard unfinished links.

The Classic editor offers Chain..., + Link, Remove, Up and Down. Each link has a criterion, optional light/tone/houselight signals and variable-series count where applicable. FT/VT alone allow input retraction. Use simple schedule preserves a time-based or signaled first link as a one-link chain instead of silently changing its contingency. The setup summary displays all links.

Variable links use independent Fleshler-Hoffman bags without replacement; see VARIABLE_SCHEDULES.md. Series reset at the next component presentation. Chains are not available in T, Lag or Yoked components. New Classic delays are represented by FT/VT links; imported legacy delay fields retain their original behavior and are explicitly displayed. Legacy delays may block other inputs and are not automatically converted.

Shared output demands are combined with OR. A continuously requested shared output can mask an intermittent cue; the editor warns about conflicts. Intermittency values are ON and OFF phase durations, with a 0.05-second minimum; zero means continuous. A 20-ms software update loop and Windows/USB latency are not hardware timing guarantees. Timeout suppresses link cues and lever restoration until every active timeout ends.

JSON v8 stores six optional per-input chain definitions, validates every link field and retains readable formatting. Previous JSON/XML/TXT configurations remain importable. ChainStart, ChainLinkStart, ChainLinkComplete, ChainComplete, ChainReinforcementResolved and ChainCancelled events identify the input, link and component presentation as applicable.

Validation: ChainTests exercises progression, stale callbacks, deadlines, MaxRefs, retraction, legacy delays, FH bags, output ownership and persistence. Storage, variable, interval, Lag and reference regressions were also run. Independent simulated checks cover simultaneous inputs and overlapping timeouts. No physical chamber or real RTDB writes were used during these tests. Physical timing and hardware operation still require a bench test before experimental use.
