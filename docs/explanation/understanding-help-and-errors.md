<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Understanding help and error handling

AnyCLI treats help, normal command output, and command-line errors as separate concerns. That separation is mostly invisible in a small app, but it determines which extension point to use when the output needs to change.

## Three kinds of failure

A command-line application can fail in three different ways:

- **Command-line input errors** happen before the command body runs. Examples are unknown commands, unknown options, missing required values, and invalid values.
- **Command exceptions** are thrown by the command body. They represent application failures, not parsing failures.
- **Cancellation** is a signal to stop work. It is not an error in the command-line input.

AnyCLI handles command-line input errors through its error pipeline. It does not convert command exceptions or cancellation into structured command errors.

## Help is assembled in layers

Help is not a fixed text block. It is assembled from layers:

```text
command metadata
    -> headings, labels, and descriptions
    -> formatting for fallback values
    -> rendering and layout
```

This layering determines the smallest change for a task:

| To change | Use |
| --- | --- |
| A parameter description | `[HelpDescription]` |
| Display text for a default value | `[HelpDefaultValue]` |
| Headings, labels, or localized descriptions | A help text provider |
| Display of fallback defaults | A help value formatter |
| Layout and sections | A help renderer |

Changing a description does not require replacing the renderer. Changing the layout does not require replacing localization.

## Output has separate channels

Help goes to standard output. Command-line errors go to standard error. Normal command output can use either stream depending on whether it is successful output or a failure.

Keeping those channels distinct lets scripts capture normal output without help or error text, and lets tests assert each stream independently.

## Spectre.Console is a presentation adapter

The Spectre.Console adapter changes presentation, not meaning. It can replace help rendering and error writing while the underlying help text, error messages, and exit codes remain unchanged.

Use richer output when the audience is a person reading a terminal. Keep machine-readable output plain, because markup and ANSI styling are noise to scripts.

## Related

- [Help reference](../reference/help.md)
- [Errors reference](../reference/errors.md)
- [Customize help output](../guides/customize-help-output.md)
- [Customize command-line errors](../guides/customize-command-line-errors.md)
- [Spectre.Console adapter reference](../reference/spectre-console-adapter.md)
