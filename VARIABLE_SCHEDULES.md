# Variable schedule series

VI, VR and VT now use a configurable Fleshler-Hoffman series (1-1000 values, default 10). The input editor shows Series values for VI/VR; the independent time schedule section shows it for VT. This is separate from component Iterations.

Each series is sampled without replacement, refilled when exhausted, and reset at the next component presentation. Each input and chamber has independent state. VI/VT retain fractional seconds until one millisecond rounding step. Unsupported timer values are rejected before a session, rather than clamped.

VR uses the same FH real-valued progression, quantized to positive integers with total mean*count preserved. Quantization starts at floor(value) with minimum one; units are added/removed where the deviation is largest until the exact total is reached. At low means repeated integer requirements are unavoidable (VR1 is all ones). A partial cycle need not average to the programmed mean.

JSON format v8 (series fields introduced in v7) stores VariableValueCount per input and TimeVariableValueCount per component. Older JSON/XML/TXT imports default to ten values. TXT imports can also read optional trailing series-count settings. New JSON retains readable indentation. Old executables cannot load v8.

Behavior change: VR previously sampled uniformly between half and 1.5 times the mean; it now uses FH. VT previously rounded FH values to whole seconds; it now retains fractions to milliseconds. Identify the software version when comparing old and new sessions. FT remains independent of responding; VI still requires a response after the interval. No changes to hardware, firmware or real RTDB data.

Validation: VariableTests covers all counts 1-1000 across eight means, independent FH integral oracle, positive exact-mean ratio quantization, sampling/refill, JSON/XML/TXT, clone isolation and UI. IntervalTests retains exhaustive FI/VI10 domain checks. Windows/USB timing and physical reinforcers have not been validated.
