# Cleaner Shrimp validation

Runtime and editor scripts compile against the project's Unity 6000.5.9f1
assembly references. A standalone harness executed 40 assertions against the
actual gameplay source files with minimal Unity API adapters. This validates
rule calculations and controller flow, but does not replace Unity execution.

The isolated Unity editor attempt failed during startup: licensing reported no
headless entitlement and Package Manager IPC could not connect. The actual
Unity behavior checks and modal render therefore remain unverified.

To run the full suite in the licensed Unity editor, use **Fishing Cards >
Validation > Cleaner Shrimp**. The suite loads the real card asset, tests exact
negative-effect removal and modifier lifetimes, checks biome-entry offers and
sequential swaps, verifies suspended controller actions, and renders the modal.
It writes its result and screenshot to this folder.

Only Coastal Waters is currently authored. Configure a real destination through
BiomeDefinition's Next Biome field to exercise Cleaning Station in the game;
validation uses fixture biomes and does not invent production biome content.
