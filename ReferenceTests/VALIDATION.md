# Validation snapshot - 2026-11-10

Candidate: bin/ReferenceValidation/ReinforcementSchedules.exe
SHA256: D42765F7B1E09E77EDB9E2069ACAB5C06C63061E671F22434D1F6024C1E9499D

Passed on the final candidate sources:
- ReferenceRuntimeTests: 40 integration assertions, including literal 50-sequence cutoff, seeded-history independence, delayed final trial/time cutoff, exact Yoked quantities/types and finite exhaustion, input-contingent Classic batches, graph-mode integration, deep clone and XML v5.
- ReferenceUiTests: 9 assertions, staged Apply/Cancel and reference UID display; screenshots ReferenceSetup.png and ReferenceDialog.png.
- ParserTests: 38 assertions, historical formats, malformed sources, boundaries, closed-session eligibility and exact outcomes/batches.
- CloudReferenceTests: 8 assertions through a fake HTTP handler, read-only listing, destination isolation and future atomic index payload.
- LagChartTests: 27 assertions including both transition modes, 50,000 sequences, scrolling/export and component/chamber isolation.
- LagRuntimeTests: 50,171 assertions.
- LagIndependentTests.ps1: 15,049 assertions.
- LagEditorTests.ps1: all UI assertions passed (31 named checks).
- IntervalTests: 599 integration/boundary assertions plus all 2,147,483 FI values and 6,502,430 VI intervals checked.
- StorageTests: 82 assertions.

Independent read-only integration review: no remaining critical actionable findings after fixes to MaxRefs tooltip and disposed-owner import cancellation.

No experimental hardware, real session data mutation, real Firebase connection, or real RTDB write was used. The existing Debug/Release executables were not replaced. Hardware timing/dispensing validation remains necessary on an empty chamber.

Reproduction: build the main project with OutputPath=bin\ReferenceValidation\ and IntermediateOutputPath=obj\ReferenceValidation\. ReferenceRuntimeTests and ReferenceUiTests link that candidate. Parser/Cloud/Chart projects compile production helper files directly. Existing Lag/Interval test projects reference their respective isolated main builds; see project HintPath.
