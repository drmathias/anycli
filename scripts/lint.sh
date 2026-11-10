#!/usr/bin/env bash

# SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
#
# SPDX-License-Identifier: MIT

set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repo_root"

if ! command -v reuse >/dev/null 2>&1; then
    echo "reuse is required: https://reuse.software/" >&2
    exit 1
fi

reuse lint
dotnet restore AnyCli.slnx
dotnet format AnyCli.slnx --verify-no-changes --severity warn --no-restore
