# Concurrent VI calculator

Classic component editor > Concurrent VI Calculator opens a planning tool for 2-6 alternatives. Enter the desired global equivalent VI in seconds and positive relative reinforcement-rate weights (e.g., 1 and 3). It does not edit any input, chain, macro, or runtime setting. Close/Escape cancels; reopening starts with VI20 and 1:1.

For global mean G and weights w_i, p_i = w_i / sum(w), VI_i = G / p_i. Thus sum(1 / VI_i) = 1 / G. At G=20: 1:1 gives 40,40; 1:3 gives 80,26.666667; 1:27 gives 560,20.740741; three equal alternatives give 60,60,60. Weights describe rates, not response requirements or interval ratios.

Simple Classic VI means currently use whole seconds. The tool therefore shows both ideal fractional means and independently rounded whole-second means (minimum one). It reports the equivalent combined VI, rate/minute and percent rate deviation of the rounded means. Choose the same Iterations as your planned inputs to check the existing FH/timer limits; out-of-range entries are flagged, never clamped or applied. It is not a rounding optimizer. The final component must still pass normal validation.

These are nominal programmed rates. The runtime keeps a separate timer and ready flag per input, waits for a collecting response, and restarts the input's interval through its normal schedule flow. COD, feedback/timeout, delivery probability, responding and finite sessions can change obtained rates. The sum does not imply the aggregate has the same interreinforcement-time distribution as one VI or predict response allocation. Reinforcer amounts are not used in this rate calculation. No changes were made to scheduling, COD, hardware, persistence or RTDB.

Choice background: Baum (1974), On two types of deviation from the matching law: bias and undermatching, https://doi.org/10.1901/jeab.1974.22-231; Baum (1979), Matching, undermatching, and overmatching in studies of choice, https://doi.org/10.1901/jeab.1979.32-269. Matching concerns obtained reinforcement and behavior; the calculator is arithmetic for programming nominal independent VI rates, not an implementation of the matching law.

## Validation
Windows PowerShell: powershell.exe -NoProfile -STA -File ConcurrentViTests.ps1 -AssemblyPath bin\Release\ReinforcementSchedules.exe
41 numerical/UI checks cover 2-6 alternatives, unequal ratios, invalid inputs, range overflow, rounding, visibility by schedule mode, Close and repeated opening without input mutation. Screenshots rendered by WinForms were inspected. Existing Add/Delete and Lag deletion regression checks pass. Physical chamber timing was not tested or changed.