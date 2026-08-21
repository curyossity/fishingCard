# Game Design

Read this file before making code, asset, prefab, scene, or architecture changes in this repository.

## Purpose

This document describes the general idea of the game. Use it to guide implementation decisions, feature scope, architecture, naming, presentation, and gameplay behavior.

## General Concept

The game is a card-based fishing roguelike. Everything in the game is represented through illustrated cards. There is no animated character, explorable world, or traditional game environment.

The player begins each run on a fishing boat and builds a deck of cards representing their fishing techniques, bait, equipment, and special actions. The objective is to catch valuable sea creatures, improve the deck and equipment, and gradually reach deeper and more dangerous waters.

The main tension is risk vs. reward: the deeper the player fishes, the rarer and more valuable the potential catches become, but the chance of losing catches or failing the expedition also increases.

## Visual Direction

The game should look and feel like a beautiful physical card game brought to a screen.

The primary inspiration is the provided reference image: large vertical illustrated cards with a handmade/storybook aesthetic. A card may depict the boat above the water and the creatures below it, with the fishing line visually continuing through the card.

The cards themselves are the world.

Different locations, depths, creatures, encounters, and discoveries should be communicated primarily through card artwork, with relatively minimal UI surrounding them.

## Core Gameplay

The player has a small hand, approximately 4 cards, and plays one card each turn. After playing a card, another is drawn from the player's deck.

Cards allow the player to perform actions such as:

- Descend deeper.
- Attract certain types of creatures.
- Reveal or search for creatures.
- Catch or reel in a creature.
- Improve the chances of catching something valuable.
- Manipulate the next encounter.
- Increase how many creatures can be caught.
- Reduce risk.
- Perform unusual or powerful fishing techniques.

The exact card mechanics are intentionally open for experimentation.

## Fishing And Depth

Depth is one of the central progression mechanics of a run.

Near the surface, creatures are relatively common and safe.

As the fishing line goes deeper, the available creature pool changes. Rare fish, predators, strange deep-sea creatures, treasures, and eventually legendary or mysterious encounters become possible.

Players should frequently face the decision: do I secure what I already have, or keep pushing deeper for something much more valuable?

## Creatures

Sea creatures are also represented by cards.

Creatures should not simply be different amounts of gold. Many should have properties that affect gameplay.

For example, a small fish might be usable as bait for a predator, a rare creature might disappear if it is not caught quickly, or a dangerous creature might threaten another catch.

This allows creatures to become part of the player's strategy rather than simply rewards.

## Roguelike Progression

During a run, the player should be able to improve and modify their deck, discover new cards, upgrade equipment, and create combinations between cards.

Different builds should encourage different fishing strategies. For example:

- Specializing in catching many small creatures.
- Hunting enormous predators.
- Manipulating probabilities to find rare creatures.
- Aggressively descending into deep water.

After catching creatures, the player can sell them for gold and use the rewards to acquire better equipment, cards, upgrades, or other progression.

The long-term game should also encourage discovering new creatures, locations, cards, and increasingly strange parts of the ocean.

## Core Design Philosophy

The game should remain simple to understand but capable of producing interesting combinations and difficult decisions.

Avoid turning it into a traditional RPG with cards replacing buttons. The cards should be the game.

The central fantasy is:

Build a fishing deck, manipulate what lies beneath your hook, catch increasingly extraordinary creatures, and decide how deep you are willing to go before the risk becomes too great.
