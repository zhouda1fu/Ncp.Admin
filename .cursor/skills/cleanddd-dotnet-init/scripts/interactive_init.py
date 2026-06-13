#!/usr/bin/env python3
"""Interactive initializer for CleanDDD dotnet projects."""
from __future__ import annotations

import argparse
import subprocess
import sys
from pathlib import Path
from typing import Iterable, List

FRAMEWORKS = ["net8.0", "net9.0", "net10.0"]
DATABASES = ["MySql", "SqlServer", "PostgreSQL", "Sqlite", "GaussDB", "DMDB", "MongoDB"]
MESSAGE_QUEUES = ["RabbitMQ", "Kafka", "AzureServiceBus", "AmazonSQS", "NATS", "RedisStreams", "Pulsar"]


def parse_bool_arg(value: str) -> bool:
    lowered = value.strip().lower()
    if lowered in {"true", "t", "1", "yes", "y"}:
        return True
    if lowered in {"false", "f", "0", "no", "n"}:
        return False
    raise argparse.ArgumentTypeError("Expected boolean value (true/false)")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Initialize a CleanDDD dotnet project.")
    parser.add_argument("--Framework", "-F", dest="framework", help="Target framework")
    parser.add_argument("--Database", "-D", dest="database", help="Database provider")
    parser.add_argument("--MessageQueue", "-M", dest="message_queue", help="Message queue provider")
    parser.add_argument("--UseAspire", "-U", dest="use_aspire", type=parse_bool_arg, help="Enable Aspire dashboard")
    parser.add_argument("--IncludeCopilotInstructions", dest="include_copilot_instructions", type=parse_bool_arg, help="Include Copilot instructions")
    parser.add_argument("--name", dest="project_name", help="Project name")
    parser.add_argument("--output", "-o", dest="output_dir", help="Output directory")
    parser.add_argument("--no-confirm", action="store_true", dest="no_confirm", help="Run without confirmation prompt")
    parser.add_argument("--skip-template-install", action="store_true", dest="skip_install", help="Skip installing NetCorePal.Template")
    return parser.parse_args()


def validate_choice(provided: str | None, options: Iterable[str], label: str, default: str) -> str:
    if provided:
        if provided not in options:
            raise SystemExit(f"Invalid {label}: {provided}. Allowed: {', '.join(options)}")
        return provided
    return prompt_choice(label, options, default)


def normalize_project_name(raw: str) -> str:
    """Convert input name to PascalCase segments joined by dots."""
    if not raw:
        return ""
    raw = raw.replace("-", ".")
    segments: List[str] = []
    for segment in raw.split("."):
        words = segment.replace("_", " ").split()
        if not words:
            continue
        segments.append("".join(word.capitalize() for word in words))
    return ".".join(segments) if segments else raw


def prompt_choice(label: str, options: Iterable[str], default: str) -> str:
    options = list(options)
    default_index = options.index(default) if default in options else 0
    print(f"\n{label}")
    for idx, opt in enumerate(options, start=1):
        suffix = " (default)" if idx - 1 == default_index else ""
        print(f"  [{idx}] {opt}{suffix}")
    while True:
        choice = input(f"Select 1-{len(options)} [default {default_index + 1}]: ").strip()
        if not choice:
            return options[default_index]
        if choice.isdigit():
            num = int(choice)
            if 1 <= num <= len(options):
                return options[num - 1]
        print("Invalid choice, try again.")


def prompt_bool(label: str, default: bool) -> bool:
    default_text = "Y/n" if default else "y/N"
    while True:
        value = input(f"{label} [{default_text}]: ").strip().lower()
        if not value:
            return default
        if value in {"y", "yes"}:
            return True
        if value in {"n", "no"}:
            return False
        print("Please enter y or n.")


def prompt_text(label: str, default: str) -> str:
    value = input(f"{label} [default '{default}']: ").strip()
    return value or default


def build_command(choices: dict) -> List[str]:
    return [
        "dotnet",
        "new",
        "netcorepal-web",
        "--Framework",
        choices["framework"],
        "--Database",
        choices["database"],
        "--MessageQueue",
        choices["message_queue"],
        "--UseAspire",
        str(choices["use_aspire"]).lower(),
        "--IncludeCopilotInstructions",
        str(choices["include_copilot_instructions"]).lower(),
        "--name",
        choices["project_name"],
        "--output",
        choices["output_dir"],
    ]


def ensure_template_installed() -> bool:
    """Ensure NetCorePal.Template is installed via dotnet new install."""
    print("\nEnsuring 'NetCorePal.Template' is installed...")
    try:
        subprocess.run([
            "dotnet",
            "new",
            "install",
            "NetCorePal.Template",
        ], check=True)
        print("Template 'NetCorePal.Template' is installed.")
        return True
    except FileNotFoundError:
        print("dotnet CLI not found. Please install .NET SDK.", file=sys.stderr)
        return False
    except subprocess.CalledProcessError as exc:
        print(f"Failed to install NetCorePal.Template (exit code {exc.returncode}).", file=sys.stderr)
        return False


def main() -> int:
    args = parse_args()
    cwd = Path.cwd()
    default_name = normalize_project_name(cwd.name)

    # Ensure required template is available before collecting inputs
    if not args.skip_install and not ensure_template_installed():
        return 1

    framework = validate_choice(args.framework, FRAMEWORKS, "Framework", "net10.0")
    database = validate_choice(args.database, DATABASES, "Database", "MySql")
    message_queue = validate_choice(args.message_queue, MESSAGE_QUEUES, "MessageQueue", "RabbitMQ")

    use_aspire = args.use_aspire if args.use_aspire is not None else prompt_bool("Enable Aspire dashboard?", True)
    include_copilot = (
        args.include_copilot_instructions
        if args.include_copilot_instructions is not None
        else prompt_bool("Include Copilot instructions?", False)
    )

    project_name_input = args.project_name if args.project_name else prompt_text("Project name", default_name)
    project_name = normalize_project_name(project_name_input)

    output_dir = args.output_dir if args.output_dir else prompt_text("Output directory", str(cwd))

    choices = {
        "framework": framework,
        "database": database,
        "message_queue": message_queue,
        "use_aspire": use_aspire,
        "include_copilot_instructions": include_copilot,
        "project_name": project_name,
        "output_dir": output_dir,
    }

    print("\nSelected options:")
    for key, value in choices.items():
        print(f"  {key}: {value}")

    if not args.no_confirm:
        if not prompt_bool("Run dotnet new with these options?", True):
            cmd = " ".join(build_command(choices))
            print(f"\nCommand preview:\n{cmd}")
            return 0

    cmd = build_command(choices)
    print(f"\nRunning: {' '.join(cmd)}\n")
    try:
        subprocess.run(cmd, check=True)
    except FileNotFoundError:
        print("dotnet CLI not found. Please install .NET SDK.", file=sys.stderr)
        return 1
    except subprocess.CalledProcessError as exc:
        print(f"dotnet new failed with exit code {exc.returncode}", file=sys.stderr)
        return exc.returncode

    print("Project created successfully.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
