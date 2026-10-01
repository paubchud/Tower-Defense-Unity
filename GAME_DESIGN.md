# Tower Defense PVP - Game Design

Status: working design based on the owner's gameplay description. This document describes the intended game; DEVELOPMENT_PLAN.md specifies the order of implementation. Nothing here implies a feature is already implemented.

## Core idea

A 1v1 tower-defense game where each player controls a class-based hero, defends a castle, sends enemy units down the opponent's lane, and develops an economy. Players buy land, harvest materials, build defenses, and can invade the opponent's territory to steal resources and fight. Authored map challenges provide additional ways to strengthen a player during a match.

Long-term progression unlocks different ways to play. Strength gained inside a match is temporary and resets for the next match.

## Player flow

1. Main menu: Play, Store, Exit.
2. Store: spend currency earned through playing on class unlocks, alternative starter equipment, loadout choices, and customization.
3. Play: select a class and starter loadout, then enter the multiplayer lobby and ready up. The precise matchmaking flow can be designed later.
4. Match: defend, send units, improve units, develop the hero, acquire land, harvest materials, build towers, and invade.
5. Match results: show the outcome and earned persistent currency, then offer rematch or return to menu.

The Store's purchase currency is earned by playing. Real-money purchases are not part of the current specification.

## Presentation and controls

Confirmed direction:

- Use 3D models for heroes, units, towers, castles, resources, and the environment.
- Keep the camera centered on the hero at all times, following them continuously like a third-person camera. Start with an elevated/top-down viewing angle for the prototype; angle, distance, and projection can be tuned during playtesting.
- Move the hero directly with WASD.
- Keep right-click dragging as the camera interaction. Current interpretation of this request: dragging rotates/orbits the view around the hero while keeping the hero as its focus.

Working camera defaults, open to playtesting:

- Hold the right mouse button and drag horizontally to orbit around the hero; vertical dragging can adjust the viewing angle within tuned limits.
- On release, preserve the chosen viewing angle and continue following the hero. Camera focus remains on the hero during both movement and dragging.
- Use camera-relative WASD movement as the prototype default: forward follows the view's forward direction projected onto the ground. Rotating the camera alone does not move the hero.
- Keep the hero visible when scenery obstructs the view; the camera's distance/obstruction behavior needs testing with the 3D map.
- Reserve right-click dragging for camera orbit. Combat ability and interaction bindings must not conflict with it, and UI interactions must not also rotate the camera.

Camera behavior belongs to local presentation and should be separate from hero movement, combat, and networking. Camera rotation alone does not extend attack, harvest, or building range. It also must not expose information hidden by any later visibility rules.

Keep gameplay positions, aim targets, and placement checks in world space so camera adjustments are practical. Moving from an elevated view to a closer behind-the-hero view may need changes to aiming, targeting, obstruction handling, and interaction feedback.

## Classes and starter loadouts

- Initial classes: Warrior and Wizard.
- Working prototype roles: Warrior emphasizes close-range combat; Wizard emphasizes ranged spells. Exact attacks, abilities, and balance are still to be designed.
- Both initial classes are available during prototype tests so neither player is blocked by progression.
- Future classes must be addable through class definitions and reusable abilities, without rewriting the menu, match economy, or networking rules.
- A class definition describes its stable ID, presentation, base gameplay values, abilities, and valid starter equipment.
- A selected loadout references those definitions. Mutable health, resources, cooldowns, and match levels belong to the player's match state.
- The server validates class and equipment choices before starting the match.

The references to runes and champion select describe choosing a playstyle before a match. They do not commit the game to reproducing another game's systems.

## Progression and fairness

The Store may use the word "upgrade," but persistent gameplay purchases must be alternatives with meaningful tradeoffs. Do not sell permanent increases to damage, health, starting gold, harvesting speed, tower strength, or resource income without a compensating design tradeoff.

Examples of possible sidegrades, subject to later design and balance:

- A quicker melee weapon with less reach or less damage per hit.
- A spell with a larger area but a longer cooldown.
- Equipment that favors harvesting at the expense of combat capability.

Each new class and starter option needs comparable overall strength and a viable default loadout. Unlocking more choices can still provide situational advantages, so equal power is a balancing goal that must be tested, not something guaranteed by labeling a purchase a sidegrade.

Distinguish two forms of development:

- Persistent: ownership of classes, starter choices, and cosmetics. These survive between matches.
- In-match: unit upgrades, hero levels, land, towers, materials, and automation. These reset each match.

## Currencies and resources

| Resource | Earned from | Used for | Lifetime |
| --- | --- | --- | --- |
| Persistent currency, name TBD | Playing completed matches; reward formula TBD | Class unlocks, starter equipment alternatives, customization | Between matches |
| Match XP | Sending units to the opponent, interpreted from the owner's description | Upgrading the units the player sends | Current match |
| Match gold | Killing units sent by the opponent | Buying land and increasing the hero's match level | Current match |
| Materials | Mining and harvesting map resources | Building towers; further recipes TBD | Current match |

XP is spendable currency for unit upgrades. It is distinct from the hero's match level and the persistent currency.

The gold connection to hero leveling is confirmed; whether players buy levels directly or buy something that advances levels still needs a decision. The exact rules for gold rewards, challenges, materials, and upgrade costs remain to be balanced.

## Sending units and defending

- Each player has a predetermined lane with an entry point and their own castle at its end.
- A player sends attacking units along the opponent's lane toward the opponent's castle.
- A valid send grants the sender match XP. Exact timing, send cost, cooldowns, queue limits, and XP values are undecided.
- The defender earns gold from killing incoming units. Reward attribution for kills during invasions needs an explicit rule.
- Spend match XP to improve sending-unit strength or behavior during that match.
- Upgrade scope is undecided: unit type, individual unit, or subsequent sends. Prototype proposal: improve future sends of a unit type and leave already spawned units unchanged.
- Towers and heroes participate in defense. Their targeting and damage must use the same authoritative combat rules.
- Units reaching the castle can damage it. Prototype proposal: they apply a defined amount of damage and despawn; this is not yet a final rule.

Automatic timed waves are not assumed to be the primary source of enemies. A test wave may be useful for diagnostics; the core gameplay loop must support player-directed sends.

## Land, harvesting, and towers

- Maps are authored beforehand. The first test map has one straight lane per player, two castles, and nearby areas for land and resources.
- Gold buys access to land where towers can be placed.
- Materials are acquired through mining or harvesting and used for tower construction.
- Working prototype proposal: predefined purchasable plots with building slots and resource nodes. This lets later maps reuse the same ownership and placement rules.
- Placement must respect plot ownership, costs, building space, and lane clearance. Walls or towers must not accidentally block a fixed lane.
- Resource nodes have server-owned state such as remaining yield and harvest timing. Respawn, depletion, and regeneration rules remain undecided.
- Automatic harvesting is a later in-match development feature. Its costs, output, and exposure to invasion must be balanced after manual harvesting works.
- Resource production and spending need maximum rates and capacities so extending a match does not create unlimited objects or network traffic.

## Heroes, invasion, and map challenges

The player directly controls their Warrior or Wizard with WASD in a 3D world. The camera continuously follows and stays centered on the hero, initially from an elevated/top-down angle. Right-click dragging is interpreted as orbiting around that focus. Aim behavior and combat/interaction bindings remain undecided. Working assumption: the hero can move into the opponent's territory under the invasion rules below.

An invasion lets a player steal resources and attack the opponent, while leaving their own side more exposed. The exact entry/return method and the types of resources that can be stolen need a decision before implementation.

Rules to design before the invasion milestone:

- Whether heroes can freely cross the map or use an entrance, gate, teleport, or cooldown.
- Whether heroes can damage other heroes, sent units, towers, harvesters, and castles.
- Hero death, respawn delay/location, loss of carried resources, and retreat rules.
- Where stolen resources come from: carried inventory, resource nodes, storage, or some combination.
- Limits on stealing and protections against permanently trapping a player without a way to recover.

Authored challenges can reward players with temporary match strength or resources. Types, placement, rewards, and whether challenges are contested remain open. Add one simple challenge after the core economy and invasion loop are stable.

## Match state and victory

Prototype proposal: destroying the opposing castle wins the match. The castle's exact damage sources and any time limit or stalemate rule remain undecided.

Every match starts with defined resources, health, land ownership, and class/loadout state. The server controls ready state, match start, resource transactions, combat, match end, and reset.

Once the match ends, stop accepting combat and economy commands. Record the result once and cleanly reset all temporary state for a rematch. A prototype disconnect can end the match; later reconnect and ranked policies require separate design.

## First playable scope

The first network smoke test proves two players can connect and control their selected classes on the same map. The first complete gameplay prototype then includes:

- Main menu, Play flow, Warrior/Wizard selection, and a Store placeholder explaining future sidegrades.
- 3D characters/environment, a hero-centered follow camera with an elevated initial view, WASD movement, and right-click drag camera orbit.
- Two players, two straight lanes, two castles, and a readable HUD.
- One sendable unit type, one XP upgrade, and kill rewards in gold.
- One tower type, one purchasable land option, and one harvestable material.
- One basic attack per class and one gold-funded hero improvement after its leveling rule is chosen.
- A minimal invasion with defined combat, death/return, and resource theft rules.
- Castle destruction, results, and rematch/reset.

The actual Store economy, large class roster, automation, additional maps, multiple challenge types, ranked play, and visual polish follow this prototype.

## Balance questions for prototype tests

- Sending creates XP for the attacker and potential gold for the defender. Test whether sending produces useful pressure, whether deliberate low-risk trading accelerates growth too much, and whether costs/cooldowns prevent endless progression from repeated sends.
- Kills grant gold that can buy both hero strength and land. Test whether an early lead makes every subsequent fight and economy investment easier, and choose growth limits or recovery options from playtest results.
- Ensure the initial hero, resources, and accessible nodes allow progress. The first send, land purchase, harvest, and tower must not require a resource that can only be earned by already owning that tower or land.
- Compare time spent defending, harvesting, sending, and invading. Each should create a useful choice, and a defeated or raided player should retain a practical path back into the match.
- Test class/loadout alternatives against multiple opponents and situations. An option that is always best violates the persistent sidegrade goal.

## Open decisions, in implementation order

1. Remaining controls/platform choices: aim behavior, combat/interaction bindings, camera angle/distance/orbit tuning, and target platform. 3D models, continuous hero-centered camera follow, and WASD are confirmed; right-click dragging is interpreted as orbiting around the hero.
2. Networking solution, local test connection flow, host versus trusted dedicated server needs.
3. Sending costs/cooldowns, XP award timing, unit upgrade scope, and kill reward attribution.
4. Gold-funded hero leveling, starting land, plot placement rules, and material recipes.
5. Invasion access, damage permissions, theft rules, death, and respawn.
6. Persistent reward formula, class/equipment tradeoffs, and map challenges.

Choose each group before building the milestone that depends on it. Avoid implementing detailed content or permanent reward systems ahead of those decisions.
