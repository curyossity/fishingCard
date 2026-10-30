# Discovery Journal — tracked quests, no rewards

The new-theme Harbor now opens Discovery Journal from its destination panel or the top navigation on every hub page. It uses the existing native paper/ink UI treatment, with requirements, progress bars, completion status and the exact three supplied rewards. Back to Harbor and Set Sail retain the existing run loop.

- **The Perfect School:** best successful expedition with four or more attached Schooling creatures. Reward text: unlock Schoolmaster's Charm for purchase; first Schooling Catch each expedition has −1 weight.
- **The Collector:** best successful expedition returning at least five distinct effective creature tags. Reward text: unlock a new special Event card.
- **Risk Taker:** three separate expeditions that successfully Surface while overloaded. Reward text: unlock Salvager's Kit for purchase; once per expedition, release one attached creature and draw one Technique.

Both Catch and Bait creatures count. Creature-like Apex attachments count as creatures. A repeated species can contribute multiple Schooling creatures; each tag counts only once in an expedition. Effective temporary/granted tags are captured before run cleanup. Lost/released creatures, uncommitted encounters and non-creatures do not count. A line-break return advances none of these quests. Surface load follows the existing result's evaluated load, including Steady Line relief. Single-expedition requirements never accumulate partial counts from different runs. Completion is retained and no reward is granted, even when a quest is marked COMPLETE.

The shared progress model is separate from the Journal presentation and authored JSON catalog. The always-active tracker listens to the controller's new completion notification, which includes a stable expedition GUID. Progress is stored locally between launches under the versioned PlayerPrefs key `FishingCards.DiscoveryJournal.v1`. Risk Taker stores only the distinct IDs needed for its authored target, preventing duplicate completion notifications from counting twice. Unsupported/corrupt saves are left untouched and reported on the page. Unsaved evidence retries on later completions and disabling the tracker. Tracking starts with this increment; old result screens do not backfill missing historical expedition records.

## Verification

Run `DiscoveryJournalChecks.ValidateRules()` and `JournalHarborChecks.ValidateAndCapture()` outside Play Mode. In Play Mode, `DiscoveryJournalChecks.BeginLiveCheck()` tests real Surface, persistence through a fresh tracker, completed Journal UI and reward isolation using a unique disposable PlayerPrefs key. `JournalHarborChecks.BeginProductionSmoke()` verifies pointer navigation, Journal entry/return and two fresh expedition loops. The test save key is removed afterward; player progress is not modified.

See `RulesVerification.json`, `ComponentVerification.json`, `LiveProgressVerification.json` and `PlayVerification.json` for results. `journal-Unity4K.png` and `journal-Unity1080p.png` show the native Journal render. This is a Unity-only functionality increment; no Penpot revision is pending, and previous completed design integrations remain completed.


Single-expedition objectives display only COMPLETE or INCOMPLETE, without partial numbers, a best-expedition caption or a progress bar. Cumulative objectives display their qualifying successful-expedition count and progress bar. Saved evidence and reward behavior are unchanged.
