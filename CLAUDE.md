# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

*Not So Fun Halloween* — a top-down 2D roguelike built in Unity 2022.3.51f1 (URP, with 2D Lighting — there's a `Global Light 2D` in the scene). Day/night cycle: days for exploring/preparing, nights spawn waves of enemies. Player is a pumpkin (Edmund) fighting off trick-or-treaters with melee (sword) and ranged (pumpkin seed slingshot) weapons.

## Development workflow

This is a Unity Editor project, not a CLI-buildable one — there's no npm/make/CLI build, lint, or test command. Development happens by opening the project in Unity 2022.3.51f1 and using the Editor directly. No automated tests currently exist in the project despite `com.unity.test-framework` being a package dependency.

The project has the Unity MCP bridge (`com.coplaydev.unity-mcp`) installed, which lets an AI agent inspect and modify the Editor directly. Two standing rules for working in this repo via that bridge, regardless of who initiated the session:
- **Never enter Play Mode** (`manage_editor` play action) to test changes — the developer always tests manually themselves.
- **Never wire scenes** (creating GameObjects, adding components, assigning references, building UI hierarchies) via MCP tools like `manage_gameobject`/`manage_components`/`manage_ui`, even when connected — the developer does all scene wiring by hand, deliberately, for their own learning. MCP is fine for read-only verification: `read_console` for compile errors, `refresh_unity` to force a recompile check, or reading scene state to diagnose a bug.

## Architecture

All gameplay scripts live flat in `Assets/_Scripts` (no subfolders). Config assets live in `Assets/Config` (+ `Assets/Config/Waves`). UI/announcement sprites live in `Assets/Art Assets/UI`.

**Dominant pattern: event-driven decoupling.** Most cross-system communication goes through C# events, not direct references — a publisher fires an event and doesn't know who's listening (`event Action`/`event Action<int>`), and independent listener scripts subscribe in `OnEnable` and unsubscribe in `OnDisable`. When adding a new reaction to something (new SFX, new UI feedback, new gameplay effect), look for an existing event to subscribe to before adding a direct reference — that's the established idiom here, not the exception.

**Combat** is built around the `IDamageable` interface (`TakeDamage(float)`, `IsDead`) and `Health` (event-driven: fires `OnDamaged`/`OnDied`, doesn't know about flashing/death FX/scoring itself — those are separate listeners like `EnemyHitFlash`). Any attacker (sword, seed, enemy contact) only ever talks to `IDamageable`/`Health`, never to a concrete enemy or player type. Three damage sources: `SwordHitbox` (melee trigger collider, active only during a swing, one hit per target per swing), `PumpkinSeed` (projectile, destroys itself on hit or after a lifetime), `EnemyAttack` (contact damage with a cooldown, config-driven amount). `WeaponInventory` toggles the melee/projectile weapon GameObjects' active state on keys 1/2 — whichever is active handles its own input in `Update`.

**Enemy AI** (`EnemyAI`) is a small state machine (`Patrol` → `Chase` → `Attack` → `Dead`) driven entirely by an `EnemyConfig` ScriptableObject, so Easy/Medium/Hard enemies are the same script with different config assets plugged in. Uses whisker-raycast obstacle avoidance and an aggro timer that any damage source (even from off-screen) resets via `Health.OnDamaged`, pulling the enemy into a chase regardless of `detectionRange`.

**Config-as-data**: `EnemyConfig` (difficulty tier + all tunable AI/combat numbers, tagged with an `EnemyDifficulty` enum), `WaveConfig` (per-difficulty min/max spawn counts for one wave via `DifficultySpawnRange`), `SwordConfig` (swing timing/damage). Tunable numeric design values belong in a ScriptableObject, not hardcoded in a MonoBehaviour.

**Wave/night system**: `NightManager` runs a night as 3 waves back-to-back via coroutines, gated behind an explicit `BeginNight()` call (it does not auto-start in `Start()` — something else, like `NightStartPrompt`, has to call it, e.g. after a "press Space" prompt). It exposes `OnCountdownTick`, `OnWaveStarted`, `OnWaveCleared`, `OnNightComplete` and knows nothing about who listens — `WaveAnnouncerUI` (sprite-based banner/countdown display) and `WaveAudioManager` (SFX) are independent subscribers, neither aware the other exists. `EnemySpawner` spawns enemies at a random point guaranteed outside the camera's current orthographic view (computed from camera size/aspect, not hardcoded), grouped and counted per `WaveConfig`'s difficulty ranges.

**DOTween** (`Assets/Plugins/Demigiant/DOTween`) is the animation library used throughout (sword swings, UI pop/fade, idle squash-and-stretch). Its **TextMeshPro module is not installed** in this project — `TMP_Text.DOFade()` etc. are unavailable; use `CanvasGroup.DOFade()` or `Image.DOFade()` (from the UI module, which is present) for fading TMP/UI content instead.
