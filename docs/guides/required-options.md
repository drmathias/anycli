<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Require an option

Use `[RequiredOption]` when a command cannot run without a named option.

```csharp
public sealed partial record DeployParameters
{
    [RequiredOption("--environment", "-e")]
    public required string Environment { get; init; }
}
```

```bash
deploy --environment staging
deploy -e staging
```

If the option is omitted, AnyCLI reports a structured command-line error before the command runs. The default command-line error exit code is `2`.

`[RequiredOption]` does not support supplying default values; use `[Option]` with a default when omission is allowed.

## Related

- [Parameter attributes reference](../reference/parameter-attributes.md)
- [Set default values](default-values.md)
- [Command-line syntax](../reference/command-line-syntax.md)
