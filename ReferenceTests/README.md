# Reference backend tests

These standalone .NET Framework 4.7.2 projects compile the production reference reader and cloud storage sources directly. They do not start the chamber application. CloudReferenceTests uses a fake HttpMessageHandler which rejects non-GET reference requests, and synthetic session data. It does not contact Firebase or experimental hardware. Its QueueSession check writes a synthetic local manifest and invokes PendingUpdates without synchronizing it.

From the repository root, in a Visual Studio Developer PowerShell:

    msbuild ReferenceTests\ParserTests.vbproj /t:Rebuild
    .\ReferenceTests\bin\Parser\ReferenceParserTests.exe
    msbuild ReferenceTests\CloudReferenceTests.vbproj /t:Rebuild
    .\ReferenceTests\bin\Cloud\CloudReferenceTests.exe

Parser tests cover complete/partial sequences, source components, failed trials in prior history, exact outcomes, cancellation, batch data, malformed input, mismatched UIDs, deep copies, Classic FT/VT rejection, and invalid snapshots. Cloud tests cover exact-subject suggestions, lightweight legacy metadata reads, destination changes, no reader writes, and atomic future session/index payloads.
