"""Collect current live-flow evidence after the real-flow regression suites finish."""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
OUTPUT = ROOT / "Docs/Validation/ChargedCabinet"


def main():
    paths = {"slotFlow": "Docs/Validation/SlotAudio/LiveVerification.json",
             "scoreFlow": "Docs/Validation/LandingScoreRush/LiveVerification.json",
             "buildFlow": "Docs/Validation/BuildPolish/LiveVerification.json"}
    results = {name: json.loads((ROOT / path).read_text(encoding="utf-8-sig")) for name, path in paths.items()}
    assert all(row["passed"] for row in results.values())
    (OUTPUT / "LiveFlowVerification.json").write_text(json.dumps(dict(date="2026-11-21", suites=results), indent=2) + "\n", encoding="utf-8")
    print(json.dumps(dict(flow=results)))


if __name__ == "__main__":
    main()
