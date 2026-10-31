"""Apply the user's unlimited-hand amendment to current rule and responsibility documents."""
from pathlib import Path

def update(path, replacements):
    p=Path(path); s=p.read_text(encoding='utf-8')
    for a,b in replacements: s=s.replace(a,b)
    p.write_text(s,encoding='utf-8')

update('Docs/Architecture.md',[
('`TechniqueDeckRuntime.MaximumHandSize` owns the per-expedition cap: four normally, six with Tideglass Reel; every explicit draw and recovery uses that cap.', '`TechniqueDeckRuntime` has no hand-size cap. Tideglass Reel adds two opening cards; subsequent draws/recoveries are unrestricted by hand size. Obsolete capacity state, API arguments and discard-for-room callbacks are removed.'),
('Optional hand discards completed before any shared release draws, with four-slot capacity','Automatic explicit draws after prior Release reactions, without capacity discards'),
('A per-expedition hand cap (four normally, six with Tideglass Reel) for setup, effect draws, and recovery;', 'Uncapped opening/effect draws and recovery;'),
('Interactive presentation of the current Technique hand, capped at four by the runtime owner.', 'Interactive presentation of the current uncapped Technique hand; the original UI paginates and the candidate UI compresses its inspectable fan.'),
('the deck owner enforces four cards. Full-hand Cod recovery reserves its exact discard occurrence before offering discard-one-or-decline.', 'the deck owner returns the selected physical discard occurrence without a hand-size check or discard-for-room choice.'),
('It then offers optional exact hand discards while two draws cannot fit four slots. Drawing consumes only the requested count that fits, including after oversized recovery;', 'It then draws the exact requested count, stopping only when no cards remain in either pile;'),
('A missing played occurrence or declining still consumes it. Full-hand recovery permits one discard; available-space recovery returns only the exact token.', 'A missing played occurrence still consumes it. Recovery automatically returns only the exact token after earlier reactions; there is no hand-size check or room choice.'),
('Keep all hand mutation paths within four cards.', 'Hand size is unrestricted; draw only the effect-authored count and never duplicate physical cards.'),
('| Hand cap | TechniqueDeckRuntime and CreatureEffectRuntime | Four-card setup/effect-draw/recovery cap; Cod reserves an exact discard occurrence before discard-one-or-decline room selection |','| Hand ownership | TechniqueDeckRuntime and CreatureEffectRuntime | No hand-size limit; explicit draws and exact recoveries preserve physical occurrence identity and completion order |'),
])

update('Docs/GDD_IMPLEMENTATION_BRIEF.md',[
('**Equipment exception to all four-card references below:** the default opening hand and cap remain four. Equipped Tideglass Reel deals six and sets that expedition\'s cap to six for all draws/recoveries; no automatic refill is added. Another loadout restores the four-card cap next run.', '**Unlimited Technique hand amendment (31 October 2026):** four is the default opening draw, not a hand limit. Tideglass Reel starts with six; any equipment loadout may gain more cards through explicit effects. No automatic refill or discard-for-room requirement exists. This supersedes older per-card full-hand wording. [Validation](Validation/UnlimitedTechniqueHand/README.md).'),
('The hand limit is four; full-hand Cod recovery allows discarding one Technique for room or declining.', 'There is no hand limit; Cod recovery returns the selected discarded Technique directly.'),
('then optionally discard hand Techniques and draw up to two that fit four slots, using normal reshuffle.', 'then draw two Techniques, using normal reshuffle.'),
('Full four-card hand permits one discard or declining; a declined/missing occurrence still spends the grant and never substitutes a different copy.', 'No hand-size check or room discard applies; a missing occurrence still spends the grant and never substitutes a different copy.'),
('immediately draws one Technique that fits four slots; full hand permits one discard or declining, never a saved draw.', 'immediately draws one Technique without a hand-size check, never a saved draw.'),
('Before any drawing, optionally discard to fit four slots; draw as many as fit.', 'Draw the authored count automatically without discarding for room.'),
('Hand size remains four.', 'The default opening draw remains four; hand size has no limit.'),
('The Technique hand limit is four.', 'The Technique hand has no size limit.'),
('For Cod recovery with a full hand, discard one Technique to make room or decline; Sea Urchin retains the same policy. Overflow past four is not allowed.', 'Cod returns the selected discarded Technique; Sea Urchin returns only its exact played occurrence after earlier reactions. Neither requires discarding for room or checks hand size.'),
('and the four-card cap.', 'and uncapped hand growth.'),
])

update('Docs/Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md',[
('### Hand and draws\n\n', '### Hand and draws\n\n**31 October 2026 amendment:** there is no Technique hand-size limit. This supersedes every older per-card full-hand, four-slot and discard-for-room instruction in this document. Four remains the default opening draw; Tideglass Reel adds two opening cards. Explicit creature/Technique effects draw or recover their stated count automatically, using normal reshuffles and physical-card ownership. No discard is required merely to make room. Required discards, optional redraws, Recast and exchanges retain their own rules.\n\n'),
('The hand limit is **four**, dealt at the start of a run.', 'The normal opening draw is **four**; the hand has no size limit.'),
('When Cod recovery is offered with a full four-card hand, the player may discard one Technique to make room or decline recovery. Recovery must not create a fifth hand card. Sea Urchin uses the same discard-one-to-make-room or decline policy. Recovery still follows each creature\'s trigger and opportunity limits.', 'Cod recovery returns the selected discarded Technique without a hand discard. Sea Urchin automatically returns the exact played occurrence after earlier reactions. Recovery still follows each creature\'s trigger and opportunity limits and cannot duplicate physical cards.'),
('Reel deals six starting Techniques and raises that expedition\'s hand limit to six: this is an explicit exception to every default four-card limit in the Technique/creature rules. All explicit draws and recoveries honor the expedition limit;', 'Reel deals six starting Techniques. Hand size is unrestricted for every loadout, so subsequent explicit draws and recoveries can grow it beyond six;'),
('- A 4-card technique hand and a small technique deck.', '- A four-card default opening draw and a small technique deck; no ongoing hand-size limit.'),
])
print('Updated current rule documents; earlier validation records remain historical.')
