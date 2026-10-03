using System;
using System.IO;
using TowerDefense.Data;
using TowerDefense.Diagnostics;
using TowerDefense.Networking;
using TowerDefense.Presentation;
using TowerDefense.UI;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace TowerDefense.Editor
{
    public static class PrototypeBuilder
    {
        private const string Root = "Assets/TowerDefense";

        [MenuItem("Tools/Tower Defense/Create Missing Prototype Assets")]
        public static void Generate()
        {
            EnsureFolder(Root + "/Content"); EnsureFolder(Root + "/Scenes"); EnsureFolder(Root + "/Prefabs");
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP Lit shader is missing.");
            var material = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Content/PrototypeBase.mat");
            if (material == null)
            {
                material = new Material(shader) { color = Color.white };
                AssetDatabase.CreateAsset(material, Root + "/Content/PrototypeBase.mat");
            }
            var primalTower = Asset<TowerDefinition>("PrimateTower", t => { t.Id = "primate-arrow"; t.DisplayName = "Arrow tower"; t.Tint = new Color(0.84f, 0.6f, 0.32f); });
            var mysticTower = Asset<TowerDefinition>("MysticTower", t => { t.Id = "mystic-focus"; t.DisplayName = "Focus tower"; t.Tint = new Color(0.45f, 0.65f, 0.95f); });
            var primate = Asset<TechnologyGroupDefinition>("Primate", g => { g.Id = "primate"; g.DisplayName = "Primate"; g.Towers = new[] { primalTower }; });
            var mystic = Asset<TechnologyGroupDefinition>("Mystic", g => { g.Id = "mystic"; g.DisplayName = "Mystic"; g.Towers = new[] { mysticTower }; });
            var stamina = Asset<EnergyDefinition>("Stamina", e => { e.Id = "stamina"; e.DisplayName = "Stamina"; e.Tint = Color.green; });
            var mana = Asset<EnergyDefinition>("Mana", e => { e.Id = "mana"; e.DisplayName = "Mana"; e.Tint = Color.cyan; });
            var sword = Item("TrainingSword", "training-sword", "Training sword", ItemKind.MeleeWeapon, new Color(0.8f, 0.85f, 0.9f));
            var staff = Item("TrainingStaff", "training-staff", "Training staff", ItemKind.RangedWeapon, new Color(0.45f, 0.65f, 0.98f));
            var tool = Item("Pickaxe", "pickaxe", "Pickaxe", ItemKind.HarvestTool, new Color(0.58f, 0.65f, 0.7f));
            var armor = Item("MailArmor", "mail-armor", "Mail armor", ItemKind.Armor, new Color(0.42f, 0.48f, 0.53f));
            var robes = Item("WizardRobes", "wizard-robes", "Wizard robes", ItemKind.Armor, new Color(0.34f, 0.36f, 0.69f));
            var warrior = Asset<HeroClassDefinition>("Warrior", c =>
            {
                c.Id = "warrior"; c.DisplayName = "Warrior"; c.Description = "Close-range fighter.\nSword, armor, and Primate towers.";
                c.TechnologyGroup = primate; c.BodyColor = new Color(0.58f, 0.32f, 0.22f); c.AccentColor = new Color(0.95f, 0.67f, 0.27f);
                c.StartingWeapon = sword; c.StartingTool = tool; c.StartingArmor = armor;
                c.Energies = new[] { new StartingEnergy { Definition = stamina, Capacity = 100, Initial = 100, RecoveryPerSecond = 10 } };
                c.AttackCosts = new[] { new AttackEnergyCost { EnergyId = "stamina", Amount = 8 } };
            });
            var wizard = Asset<HeroClassDefinition>("Wizard", c =>
            {
                c.Id = "wizard"; c.DisplayName = "Wizard"; c.Description = "Ranged spellcaster.\nStaff, robes, and Mystic towers.";
                c.TechnologyGroup = mystic; c.BodyColor = new Color(0.29f, 0.29f, 0.6f); c.AccentColor = new Color(0.4f, 0.75f, 1);
                c.StartingWeapon = staff; c.StartingTool = tool; c.StartingArmor = robes;
                c.Energies = new[] { new StartingEnergy { Definition = mana, Capacity = 100, Initial = 100, RecoveryPerSecond = 8 } };
                c.AttackDamage = 18; c.AttackRange = 10; c.AttackInterval = 0.5f;
                c.AttackCosts = new[] { new AttackEnergyCost { EnergyId = "mana", Amount = 12 } };
            });
            var map = Asset<MapDefinition>("StraightDuel", m =>
            {
                m.Id = "straight-duel"; m.HalfSize = new Vector2(24, 25);
                m.Lanes = new[]
                {
                    new LaneDefinition { Id = "blue-lane", Entry = new Vector3(-10, 0, -18), Castle = new Vector3(-10, 0, 17), HeroSpawn = new Vector3(-6, 0.1f, 10), SideColor = new Color(0.22f, 0.55f, 0.88f) },
                    new LaneDefinition { Id = "red-lane", Entry = new Vector3(10, 0, -18), Castle = new Vector3(10, 0, 17), HeroSpawn = new Vector3(6, 0.1f, 10), SideColor = new Color(0.82f, 0.32f, 0.27f) }
                };
                m.Plots = new[] { new Vector3(-16, 0, -3), new Vector3(-16, 0, 5), new Vector3(16, 0, -3), new Vector3(16, 0, 5) };
                m.ResourceNodes = new[] { new Vector3(-19, 0, 12), new Vector3(-19, 0, -12), new Vector3(19, 0, 12), new Vector3(19, 0, -12) };
            });
            var combat = Asset<CombatRulesDefinition>("CombatRules", c => { });
            var catalog = Asset<ContentCatalog>("PrototypeCatalog", c => { c.Classes = new[] { warrior, wizard }; c.TestMap = map; c.BaseMaterial = material; c.CombatRules = combat; });
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/NetworkHero.prefab");
            if (prefab == null)
            {
                var hero = new GameObject("NetworkHero");
                hero.AddComponent<NetworkObject>();
                var controller = hero.AddComponent<CharacterController>();
                controller.center = Vector3.up; controller.height = 2; controller.radius = 0.4f;
                hero.AddComponent<NetworkTransform>();
                hero.AddComponent<NetworkHero>().Catalog = catalog;
                prefab = PrefabUtility.SaveAsPrefabAsset(hero, Root + "/Prefabs/NetworkHero.prefab");
                UnityEngine.Object.DestroyImmediate(hero);
            }
            var pipeline = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>("Assets/Settings/PC_RPAsset.asset");
            if (GraphicsSettings.defaultRenderPipeline == null && pipeline != null) GraphicsSettings.defaultRenderPipeline = pipeline;
            CreateMenu(catalog, prefab);
            CreateArena(catalog);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(Root + "/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene(Root + "/Scenes/TestArena.unity", true)
            };
            AssetDatabase.SaveAssets();
            Debug.Log("TD_GENERATE_PASS");
        }

        private static T Asset<T>(string name, Action<T> initialize) where T : ScriptableObject
        {
            string path = Root + "/Content/" + name + ".asset";
            var value = AssetDatabase.LoadAssetAtPath<T>(path);
            if (value != null) return value;
            value = ScriptableObject.CreateInstance<T>(); initialize(value); AssetDatabase.CreateAsset(value, path);
            return value;
        }

        private static ItemDefinition Item(string name, string id, string display, ItemKind kind, Color color) =>
            Asset<ItemDefinition>(name, item => { item.Id = id; item.DisplayName = display; item.Kind = kind; item.Tint = color; });

        private static void CreateMenu(ContentCatalog catalog, GameObject heroPrefab)
        {
            string path = Root + "/Scenes/MainMenu.unity";
            if (File.Exists(path)) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var session = new GameObject("Bootstrap").AddComponent<PrototypeSession>();
            session.Catalog = catalog; session.HeroPrefab = heroPrefab;
            new GameObject("Menu").AddComponent<PrototypeView>();
            new GameObject("Development diagnostics").AddComponent<SmokeTestDriver>();
            var camera = new GameObject("Menu camera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0.04f, 0.06f, 0.1f);
            camera.tag = "MainCamera"; camera.gameObject.AddComponent<AudioListener>();
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void CreateArena(ContentCatalog catalog)
        {
            string path = Root + "/Scenes/TestArena.unity";
            if (File.Exists(path)) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Authored arena").AddComponent<ArenaWorld>().Catalog = catalog;
            new GameObject("Arena HUD").AddComponent<PrototypeView>().MatchView = true;
            var camera = new GameObject("Hero camera").AddComponent<Camera>();
            camera.tag = "MainCamera"; camera.gameObject.AddComponent<AudioListener>();
            camera.transform.position = new Vector3(0, 25, -15); camera.transform.rotation = Quaternion.Euler(55, 0, 0);
            camera.backgroundColor = new Color(0.11f, 0.17f, 0.23f); camera.fieldOfView = 55;
            camera.gameObject.AddComponent<HeroOrbitCamera>();
            var light = new GameObject("Sun").AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.4f;
            light.shadows = LightShadows.Soft; light.transform.rotation = Quaternion.Euler(50, -30, 0);
            RenderSettings.ambientLight = new Color(0.52f, 0.57f, 0.62f);
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }

        public static void BuildWindows() => BuildVersionedWindows();

        public static void BuildInternetWindows() => BuildVersionedWindows();

        [MenuItem("Tools/Tower Defense/Build Versioned Windows Player")]
        public static void BuildVersionedWindows() => BuildVersionedPlayer(BuildTarget.StandaloneWindows64, VersionedBuild.Windows);

        [MenuItem("Tools/Tower Defense/Build Versioned Mac Player (Universal Mono)")]
        public static void BuildVersionedMac()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX))
                throw new InvalidOperationException("Install Mac Build Support (Mono) for this Unity editor in Unity Hub.");
            EditorUserBuildSettings.SetPlatformSettings(BuildPipeline.GetBuildTargetName(BuildTarget.StandaloneOSX), "Architecture", "x64arm64");
            // macOS IL2CPP requires a Mac/Xcode. The cross-built prototype intentionally uses Mono.
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            BuildVersionedPlayer(BuildTarget.StandaloneOSX, VersionedBuild.MacOS);
        }

        private static void BuildVersionedPlayer(BuildTarget target, string platform)
        {
            string version = PlayerSettings.bundleVersion;
            VersionedBuild.ValidateVersion(version);
            string repository = Path.GetFullPath(Path.Combine(Application.dataPath, "../.."));
            string directory = VersionedBuild.NextDirectory(repository, version, platform);
            Generate();
            string output = Path.Combine(directory, platform == VersionedBuild.Windows ? "TowerDefense.exe" : "TowerDefense.app");
            Directory.CreateDirectory(directory);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { Root + "/Scenes/MainMenu.unity", Root + "/Scenes/TestArena.unity" },
                locationPathName = output, target = target,
                options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Prototype build failed: " + report.summary.result);
            EosBuildConfiguration.CopyToBuild(directory, platform);
            // Never change the launcher or latest-build manifest after a failed/canceled build.
            VersionedBuild.Publish(repository, directory, version, platform: platform);
            Debug.Log("TD_BUILD_PASS " + output);
        }
    }
}
