# Tower Defense PVP - Game Design

Status: working design based on the owner's gameplay description. This document describes the intended game; DEVELOPMENT_PLAN.md specifies the order of implementation. Nothing here implies a feature is already implemented. README.md lists the actual first implementation and its temporary defaults separately from these intended rules.

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

## Publishing, guest identity, and saved progress

- The latest owner decision is private Steam development playtesting now, with guest play accommodated later. Steam accounts are required for the current internet mode, but future guests must not require Steam. EOS remains canceled; no guest backend is selected and Steam/guest cross-play is not required.
- Multiplayer should not require router port forwarding or metered third-party service-usage charges. Current private testing uses Steamworks identity/lobbies/P2P and explicit development App ID 480, separate from gameplay authority. The 0.1.2 private build is locally verified but still needs two-account/device replication checks. A real App ID is needed later for production distribution. Guest identity/networking will use an independent provider; public service limits/costs need a separate decision. The published 0.1 prototype still uses Unity Relay.
- Key later saved progress by provider-namespaced identity (Steam, private test or guest); keep account/profile IDs separate from per-match Netcode IDs. Define save schema/migrations/recovery and optional Steam Cloud or guest account linking before permanent unlocks. Multiplayer does not automatically implement saved progression. See GUEST_PLAY_PLAN.md.
- Keep private credentials server-side. Player-hosted matches are suitable for private prototype tests, not trusted competitive rewards; persistence/security and storefront costs need their own plan.
- Name tested builds by update version: `0.1` completes Step 1. Maintain the same local shortcut and a latest GitHub Release download, while retaining previous versions for rollback.

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

## Hotbar, inventory, and menu shortcuts

- Provide a scrollable inventory hotbar for selecting weapons and other usable items. Working control interpretation: the mouse wheel changes the selected hotbar slot; slot count, wrapping behavior, and optional number-key selection remain undecided.
- Keep the carried inventory, quick-access hotbar, and worn equipment slots distinct. A hotbar entry references an owned item rather than creating another copy of it.
- Add named input actions for opening the in-match shop, upgrade interface, troop-sending interface, inventory/equipment, and other important panels. Specific keys are undecided; support changing bindings without rewriting gameplay logic.
- The in-match shop is distinct from the Main Menu Store: it handles match purchases, while the Store handles persistent class/loadout unlocks and customization.
- Reserve mouse-wheel gameplay input for hotbar selection. Any future camera zoom needs a different binding or explicit input context. A scroll event over a menu should scroll that menu without also switching the held weapon.
- Menu interactions must not also trigger camera orbit, attacks, or troop sends. A shortcut opens an interface; purchases and sends still require their normal server validation.
- In ghost mode, inventory and permitted own-side management remain accessible. Selecting a weapon or harvesting tool does not grant permission to attack or gather while dead.

## Classes and starter loadouts

- Initial classes: Warrior and Wizard.
- Working prototype roles: Warrior emphasizes close-range combat; Wizard emphasizes ranged spells. Exact attacks, abilities, and balance are still to be designed.
- Both initial classes are available during prototype tests so neither player is blocked by progression.
- The owner has approximately 25 class concepts prepared, ranging from easy to difficult to use. Add beginner-friendly classes first and introduce more demanding classes through later progression/unlocks. The full roster and individual designs have not yet been supplied here.
- Future classes must be addable through class definitions and reusable abilities, without rewriting the menu, inventory, match economy, or networking rules.
- A class definition describes its stable ID, technology group, presentation, complexity, unlock requirements, base gameplay values, energy pools, abilities, equipment compatibility, and default/alternative starter loadouts.
- A selected loadout references those definitions. Mutable health, resources, cooldowns, and match levels belong to the player's match state.
- The server validates class and equipment choices before starting the match.

The references to runes and champion select describe choosing a playstyle before a match. They do not commit the game to reproducing another game's systems.

## Technology groups and shared tower sets

The roster target is five technology groups with five classes in each, for 25 classes. Use the owner's name **Primate** as given. Three group names and most class assignments remain unspecified.

| Technology group | Known classes | Shared tower set |
| --- | --- | --- |
| Primate | Warrior; Hunter with a bow, planned later | Primate tower catalog shared by every Primate class |
| Mystic | Wizard | Mystic tower catalog shared by every Mystic class |
| Group 3, name TBD | Not yet supplied | Its own catalog, TBD |
| Group 4, name TBD | Not yet supplied | Its own catalog, TBD |
| Group 5, name TBD | Not yet supplied | Its own catalog, TBD |

Tower availability follows **class -> technology group -> tower catalog**. Warrior and Hunter use the same Primate catalog; Wizard uses the different Mystic catalog. Specific towers, recipes, values, and unlocks within each catalog still need design.

Technology-group differences currently specify tower access. Different troop-sending catalogs per group have not been specified.

Store the catalog on the technology-group definition and reference it from each class. Do not duplicate tower lists across five class definitions or assume two classes in one group must have the same weapons, equipment, abilities, or energy pools. The five-by-five roster is a content target, not a hard-coded engine limit.

Working match rule: class and technology group are fixed when the match begins. The server derives eligible towers from that selection and validates building/upgrades against the catalog. Mid-match class or group switching is not specified.

## Class energies

- Support mana, stamina, and additional energy types through reusable resource definitions. A class can have no energy pool, one pool, or several pools as its design requires.
- Define resource IDs, maximum/starting amounts, recovery rules, and ability/item costs as data. Pool assignments and values for Warrior/Wizard remain undecided.
- Ability and item use must validate the selected item, class compatibility, life state, cooldown, and all required energy costs before applying an effect. Simultaneous actions must not overspend a pool.
- Energy is separate from gold, match XP, materials, and persistent currency. The HUD shows the pools relevant to the selected class rather than assuming every class has mana.
- Equipment and upgrades may affect energy behavior during a match. Death/respawn recovery, regeneration while a ghost, and handling changes to a pool's maximum remain open design decisions.

## Equipment and in-match item growth

- Provide equipment slots for weapons, armor/clothing, and any other item categories the final designs require. Slot count, names, and class restrictions remain undecided.
- Some classes may start without armor, some in robes, and some in armor. Starter gear belongs to the class/loadout definition rather than a universal outfit applied to all heroes.
- Support obtaining and equipping items during a match. Finding gear, crafting it, and upgrading it are intended possibilities; acquisition rules, recipes, and upgrade paths are not yet decided.
- Separate an item's shared definition from its owned instance/stack and equipped state. Inventory, hotbar, and equipment UI must reference consistent ownership.
- In-match equipment improvements can increase power as part of that match's economy. Persistent Store purchases remain starter alternatives with meaningful tradeoffs.
- Working lifetime rule: gear found/crafted/improved during a match resets with the match, consistent with other temporary growth. Persistent ownership of starter options is separate; retaining found loot between matches has not been requested.
- Equipment retained/lost on death, equipping/crafting while a ghost, and effects on energy/stat values need rules before those interactions are implemented. Crafting must not become a route around the ban on collecting resources while dead.

## Progression and fairness

The Store may use the word "upgrade," but persistent gameplay purchases must be alternatives with meaningful tradeoffs. Do not sell permanent increases to damage, health, starting gold, harvesting speed, tower strength, or resource income without a compensating design tradeoff.

Examples of possible sidegrades, subject to later design and balance:

- A quicker melee weapon with less reach or less damage per hit.
- A spell with a larger area but a longer cooldown.
- Equipment that favors harvesting at the expense of combat capability.

Each new class and starter option needs comparable overall strength and a viable default loadout. Unlocking more choices can still provide situational advantages, so equal power is a balancing goal that must be tested, not something guaranteed by labeling a purchase a sidegrade.

Class progression introduces more options and complexity, not an automatic increase in strength. Keep unlock requirements configurable: currency costs, class prerequisites, or mastery milestones are possible mechanisms, but their exact combination and order are undecided. Show difficulty/playstyle and the technology group during class selection; beginner classes must stay viable against later unlocks.

Distinguish two forms of development:

- Persistent: ownership of classes, starter choices, and cosmetics. These survive between matches.
- In-match: unit upgrades, hero levels, acquired/equipped gear, energy state, land, towers, materials, and automation. These reset each match.

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
- Each technology group has its own tower catalog shared by its member classes. Use the selected class's group to populate build menus and validate construction; Primate and Mystic start with one test tower each before their larger sets are designed.
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
- Respawn delay/location, carried-resource handling on death, ghost roaming limits, and retreat rules. Timed respawn with ghost management is confirmed below.
- Where stolen resources come from: carried inventory, resource nodes, storage, or some combination.
- Limits on stealing and protections against permanently trapping a player without a way to recover.

Authored challenges can reward players with temporary match strength or resources. Types, placement, rewards, and whether challenges are contested remain open. Add one simple challenge after the core economy and invasion loop are stable.

## Death, ghost mode, and respawn

Confirmed rule: dying starts a respawn timer. During that timer, the player controls a roaming ghost and can still buy, sell, and interact with their own side. They cannot attack or pick up more resources. When the timer ends, the hero respawns.

| Action while a ghost | Rule |
| --- | --- |
| Roam and control the camera | Allowed; exact roaming limits remain to be decided |
| Buy, sell, and manage the player's own side | Allowed through available interactions, using normal ownership, cost, and placement checks |
| Attack or use an ability to deal hero damage | Not allowed |
| Pick up materials, mine, harvest, or steal resources | Not allowed |

Buying and selling remain normal economic transactions. The collection restriction is not a blanket ban on balance changes: a valid sale can refund currency, and a purchase can spend existing currency/materials. Any future interaction that directly claims newly harvested materials must respect the ghost restriction.

The intended penalty is temporary loss of the hero's combat presence and manual resource collection. The player can still make decisions and manage defenses, but a poorly timed death leaves their side exposed and delays gathering.

Working implementation defaults, subject to balancing:

- Use explicit Alive, Ghost, and Respawning states. The server controls the death transition, respawn deadline, and restoration of the living hero.
- The hero-centered camera follows the ghost during the timer, then follows the respawned hero. Keep the player's identity, class, side, and match economy separate from the temporary hero body.
- Existing towers and already sent units continue operating while their owner is a ghost. Treat their actions separately from the dead hero's prohibited attacks.
- Reject new ghost attack/collection requests and cancel unfinished hero harvesting when death occurs. Repeated damage/death messages must not restart the timer or duplicate rewards.
- Show a clear ghost appearance and a countdown, with feedback explaining unavailable actions. Enforce permissions on the server as well as in the UI.
- End-of-match and rematch cleanup cancel pending respawn timers and ghost interactions.

Still to decide: timer duration/scaling; respawn location and restored health; whether carried materials are kept, dropped, or lost; ghost visibility, collision, and vulnerability; territory/proximity limits on roaming and management; ability/projectile effects already in flight at death; and which passive rewards or automated output continue during ghost mode. Sending new units while dead is also an explicit management rule to settle.

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
- A small working hotbar and shortcuts for the in-match shop, upgrades, and troop-sending interface.
- One starter tower per implemented technology group (Primate and Mystic), one purchasable land option, and one harvestable material.
- One own-side selling/refund interaction, with the sellable asset and refund policy chosen before the economy milestone.
- One basic attack per class, minimal starter inventory/equipment, and selected energy-pool support after class resource rules are chosen.
- One gold-funded hero improvement after its leveling rule is chosen.
- Timed hero respawn with a roaming ghost that can manage their own side but cannot attack or collect resources.
- A minimal invasion with defined combat, return, and resource theft rules.
- Castle destruction, results, and rematch/reset.

The actual Store/unlock economy, full 25-class roster and five tower catalogs, equipment loot/crafting/upgrade content, automation, additional maps, multiple challenge types, ranked play, and visual polish follow this prototype. Their shared data and ownership structures are established in the prototype; their full content is added in stages.

## Balance questions for prototype tests

- Sending creates XP for the attacker and potential gold for the defender. Test whether sending produces useful pressure, whether deliberate low-risk trading accelerates growth too much, and whether costs/cooldowns prevent endless progression from repeated sends.
- Kills grant gold that can buy both hero strength and land. Test whether an early lead makes every subsequent fight and economy investment easier, and choose growth limits or recovery options from playtest results.
- Ensure the initial hero, resources, and accessible nodes allow progress. The first send, land purchase, harvest, and tower must not require a resource that can only be earned by already owning that tower or land.
- Compare time spent defending, harvesting, sending, and invading. Each should create a useful choice, and a defeated or raided player should retain a practical path back into the match.
- Test whether the ghost timer creates a meaningful cost through lost combat and gathering time while keeping management useful. Ghost roaming must not become a better scouting or collection strategy than staying alive.
- Test class/loadout alternatives against multiple opponents and situations. An option that is always best violates the persistent sidegrade goal.
- Test whole technology-group tower sets alongside their classes. A fair hero duel alone does not establish fair access to defense, economy, and invasion pressure.
- Check that increasing class complexity does not mean increasing guaranteed power, and that starting without armor has a balanced class/loadout tradeoff.

## Open decisions, in implementation order

1. Remaining controls/platform choices: aim behavior, combat/interaction/menu bindings, hotbar size/selection behavior, camera angle/distance/orbit tuning, and target platform. 3D models, continuous hero-centered camera follow, and WASD are confirmed; right-click dragging is interpreted as orbiting around the hero.
2. Steamworks App ID/distribution configuration, two-account/device verification of the implemented Steam preview, save recovery, and host versus trusted dedicated server needs. EOS is canceled. The published 0.1 Unity Relay slice is retained for recovery; it is not the final Steam networking solution.
3. Sending costs/cooldowns, XP award timing, unit upgrade scope, and kill reward attribution.
4. Initial class energy pools/costs/recovery, starter equipment slots and compatibility, first Primate/Mystic towers, gold-funded hero leveling, starting land, plot rules, and material recipes.
5. Invasion access, damage permissions, theft rules, respawn timer/location, carried-resource/equipment handling, ghost energy recovery, and remaining ghost equipment/crafting/sending permissions. The timed respawn and ghost management/attack/collection rules are confirmed.
6. Persistent reward and class-unlock rules, the full class roster and remaining group names, tower catalogs, equipment acquisition/crafting/upgrades, and map challenges.

Choose each group before building the milestone that depends on it. Avoid implementing detailed content or permanent reward systems ahead of those decisions.
