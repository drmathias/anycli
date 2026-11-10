#!/usr/bin/env bash

# SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
#
# SPDX-License-Identifier: MIT

set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
benchmarks_root="$repo_root/benchmarks"
hosts_root="$repo_root/artifacts/benchmarks/hosts"
results_root="$benchmarks_root/results/latest"
rid="${RID:-$(dotnet --info | awk -F': *' '/RID:/{print $2; exit}')}"

if [[ -z "$rid" ]]; then
    echo "Could not determine the .NET runtime identifier." >&2
    exit 1
fi

echo "Runtime identifier: $rid"

rm -rf "$hosts_root"
mkdir -p "$hosts_root/jit"

for host in AnyCli SystemCommandLine SpectreCli ConsoleAppFramework; do
    echo "Publishing JIT host: $host"
    dotnet publish "$benchmarks_root/hosts/$host.Host/$host.Host.csproj" \
        --configuration Release \
        --property:BenchmarkAot=false \
        --output "$hosts_root/jit/$host"
done

if [[ "${BENCH_SKIP_AOT:-0}" != "1" ]]; then
    mkdir -p "$hosts_root/aot"

    # Spectre.Console.Cli is reflection-based and does not support NativeAOT.
    for host in AnyCli SystemCommandLine ConsoleAppFramework; do
        echo "Publishing AOT host: $host"
        dotnet publish "$benchmarks_root/hosts/$host.Host/$host.Host.csproj" \
            --configuration Release \
            --runtime "$rid" \
            --property:BenchmarkAot=true \
            --output "$hosts_root/aot/$host"
    done
fi

benchmark_args=("$@")
official_run=0
if [[ ${#benchmark_args[@]} -eq 0 ]]; then
    official_run=1
    benchmark_args=(--filter '*')
    rm -rf "$results_root"
    mkdir -p "$results_root"
    rm -rf "$repo_root/BenchmarkDotNet.Artifacts"
fi

BENCH_HOSTS_DIR="$hosts_root" \
dotnet run --configuration Release \
    --project "$benchmarks_root/AnyCli.Benchmarks/AnyCli.Benchmarks.csproj" \
    -- "${benchmark_args[@]}"

if [[ "$official_run" -eq 1 && -d "$repo_root/BenchmarkDotNet.Artifacts/results" ]]; then
    cp -R "$repo_root/BenchmarkDotNet.Artifacts/results/." "$results_root/"
    echo "Benchmark results copied to $results_root"
    dotnet run "$benchmarks_root/update-readme.cs" -- "$repo_root"
    echo "Updated benchmark results in README.md"
else
    echo "Filtered run: skipped README and latest-results updates."
fi
