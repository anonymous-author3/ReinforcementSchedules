# Four reinforcer outputs

Soft Schedules supports Feeder 1, Feeder 2, Pump 1, Pump 2, and Random for each schedule input and the independent time schedule. For Random, select outputs and set whole-number percentages totaling 100. A zero percentage excludes an output. Delivery probability remains a separate decision: after a delivery is accepted, the output is drawn for each delivered unit in the programmed magnitude.

## Compatibility and command map

- Historical `Pellet` means Feeder 1; `Liquid` means Pump 1. Old Random macros retain their PelletP split between output 1 only.
- Default pulse letters: Feeder 1 `R`, Pump 1 `W`, Feeder 2 `U`, Pump 2 `V`. They can be changed under Arduino commands. Uppercase and lowercase are different.
- Old 30-letter maps retain all their original letters. The two new slots use U/V if free, otherwise unused letters. Inspect those slots before using new hardware.
- JSON setup version 10 stores the complete command map and optional four-output probability distributions. Old JSON/XML/TXT imports remain supported. No historical file is rewritten automatically.
- Delivered-command records and Yoked references preserve the specific output identity. A log entry confirms a command was sent, not that hardware physically dispensed or an animal consumed a reinforcer.

## Firmware prerequisite

The bundled `newProgram2026_6inputs_4lights.ino` still controls one feeder (step A0/direction A1) and one pump (A2). It does **not** implement U/V or second-output wiring. To use Feeder 2 or Pump 2 physically, the chamber firmware must implement the configured pulse commands, define verified pins/driver connections, and ensure X/all-off stops all outputs. No pins for second outputs have been assumed, no firmware has been flashed, and no physical validation has been performed. Do not run a second-output experiment against the old sketch.

Automated validation: `ReinforcerTests` tests command routing, random eligibility, legacy maps, schema roundtrips, sparse RTDB restoration, and raw/Yoked identities with simulated commands only.
