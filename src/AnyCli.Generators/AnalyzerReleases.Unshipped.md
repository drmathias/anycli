; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
ANYCLI001 | AnyCli | Error | Unsupported parameter type
ANYCLI002 | AnyCli | Error | Invalid parameter parser
ANYCLI003 | AnyCli | Error | Invalid variadic parameter
ANYCLI004 | AnyCli | Warning | Duplicate parameter parser registration
ANYCLI005 | AnyCli | Error | Invalid variadic minimum count
ANYCLI007 | AnyCli | Error | Invalid argument position
ANYCLI009 | AnyCli | Error | Invalid option name
ANYCLI013 | AnyCli | Error | Invalid option alias
ANYCLI014 | AnyCli | Error | Duplicate option name
ANYCLI015 | AnyCli | Error | Global option declared as argument
ANYCLI016 | AnyCli | Error | Conflicting parameters kind
ANYCLI017 | AnyCli | Error | Invalid variadic maximum count
ANYCLI018 | AnyCli | Error | Interceptor global options mismatch
ANYCLI019 | AnyCli | Error | Interceptor exit code mismatch
ANYCLI020 | AnyCli | Error | Invalid default value provider
ANYCLI022 | AnyCli | Warning | Help default value on required parameter
ANYCLI023 | AnyCli | Warning | Help default value without default value
ANYCLI025 | AnyCli | Warning | Ambiguous default value provider
ANYCLI026 | AnyCli | Warning | Ambiguous parameter parser
