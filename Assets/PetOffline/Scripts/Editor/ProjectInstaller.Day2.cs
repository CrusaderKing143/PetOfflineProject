using PetOffline.Gameplay;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PetOffline.Editor
{
    internal static class ProjectInstallerDay2
    {
        public static void Build(ProjectInstallAssets assets)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var world = new GameObject("World");
            ProjectInstallerWorldFactory.CreateBackground(world.transform, "Static Background", assets.Day2Background);
            ProjectInstallerWorldFactory.CreatePatch(
                world.transform, "Banana Floor Patch", assets.Day2BananaPatch,
                assets.Day2BananaPosition, assets.Day2GeneratedScale);
            Day2Objects objects = CreateGameplayObjects(world.transform, assets);
            CreateForeground(world.transform, objects, assets);
            CreateBlockers(world.transform);
            BindRuntime(world.transform, objects, assets);
            SaveScene(scene);
        }

        private static Day2Objects CreateGameplayObjects(
            Transform world,
            ProjectInstallAssets assets)
        {
            var result = new Day2Objects();
            result.PlayerSpawn = ProjectInstallerWorldFactory.CreatePoint(
                world, "Player Spawn", new Vector3(1.15f, -3.55f, 0f));
            result.Player = ProjectInstallerWorldFactory.CreatePlayer(
                world, result.PlayerSpawn.position, assets);
            result.Banana = ProjectInstallerWorldFactory.CreateCarryable(
                world, "Banana", assets.Day2BananaPosition, Vector3.one * 0.7f,
                assets.BananaSprite, CarryableId.Banana, PlayerCarryStyle.Standard, false,
                assets.CarryableConfig);
            CreateZones(world, assets, result);
            CreateRobot(world, assets, result);
            CreateSensors(world, assets, result);
            CreateEndingPath(world, result);
            return result;
        }

        private static void CreateZones(
            Transform world,
            ProjectInstallAssets assets,
            Day2Objects result)
        {
            result.BalconySun = ProjectInstallerWorldFactory.CreateTrigger(
                world, "Balcony Sun Zone", new Vector3(-4.65f, 2.0f, 0f),
                new Vector2(3.6f, 2.3f), assets, new Color(1f, 0.82f, 0.25f, 0.13f));
            result.LivingSun = ProjectInstallerWorldFactory.CreateTrigger(
                world, "Living Room Sun Zone", new Vector3(5.25f, 3.15f, 0f),
                new Vector2(3.5f, 1.8f), assets, new Color(1f, 0.82f, 0.25f, 0.11f));
            result.FeederZone = ProjectInstallerWorldFactory.CreateTrigger(
                world, "Feeder Zone", new Vector3(1.15f, -3.4f, 0f),
                new Vector2(2.0f, 1.4f), assets, new Color(0.35f, 0.8f, 1f, 0.08f));
            result.SideDoor = ProjectInstallerWorldFactory.CreateTrigger(
                world, "Side Door Trigger", new Vector3(1.55f, 2.15f, 0f),
                new Vector2(1.25f, 2.1f), assets, new Color(0.5f, 0.85f, 1f, 0.09f));
            result.BananaGoal = ProjectInstallerWorldFactory.CreateGoal(
                world, "Banana Robot Path Goal", new Vector3(0.65f, -2.15f, 0f),
                new Vector2(1.8f, 1.0f), assets, new Color(0.95f, 0.75f, 0.2f, 0.15f));
            result.SlipZone = ProjectInstallerWorldFactory.CreateSlipZone(
                world, result.BananaGoal.transform.position, new Vector2(2.0f, 1.2f), result.Banana);
        }

        private static void CreateRobot(
            Transform world,
            ProjectInstallAssets assets,
            Day2Objects result)
        {
            Transform[] patrol = ProjectInstallerWorldFactory.CreateWaypoints(
                world, "Robot Patrol",
                new Vector3(4.5f, 2.35f, 0f),
                new Vector3(3.1f, 1.0f, 0f),
                new Vector3(-1.25f, 1.0f, 0f),
                new Vector3(-1.25f, -2.15f, 0f),
                result.BananaGoal.transform.position,
                new Vector3(-1.25f, -2.15f, 0f),
                new Vector3(-1.25f, 1.0f, 0f),
                new Vector3(3.1f, 1.0f, 0f));
            result.Robot = ProjectInstallerWorldFactory.CreateRobot(
                world, patrol[0].position, patrol, assets);
            result.FeederImpact = ProjectInstallerWorldFactory.CreatePoint(
                world, "Feeder Impact", new Vector3(1.15f, -3.25f, 0f));
        }

        private static void CreateSensors(
            Transform world,
            ProjectInstallAssets assets,
            Day2Objects result)
        {
            result.MainCamera = ProjectInstallerWorldFactory.CreateSensor(
                world, "Main Camera", new Vector3(4.0f, -2.55f, 0f), 103f,
                result.Player.transform, assets);
            result.BackupCamera = ProjectInstallerWorldFactory.CreateSensor(
                world, "Backup Camera", new Vector3(1.65f, 3.4f, 0f), 103f,
                result.Player.transform, assets);
        }

        private static void CreateEndingPath(Transform world, Day2Objects result)
        {
            result.QuietEndingPath = ProjectInstallerWorldFactory.CreateWaypoints(
                world, "Quiet Ending Path",
                new Vector3(4.7f, 3.0f, 0f),
                new Vector3(3.5f, 1.45f, 0f),
                new Vector3(5.5f, 3.75f, 0f));
        }

        private static void CreateForeground(
            Transform world,
            Day2Objects objects,
            ProjectInstallAssets assets)
        {
            objects.FeederVisual = ProjectInstallerWorldFactory.CreateForeground(
                world, "Feeder Foreground", assets.FeederSprite,
                objects.FeederImpact.position, Vector3.one * 0.62f, 350);
            ProjectInstallerWorldFactory.CreateForeground(
                world, "Side Door Left", assets.DoorLeftSprite,
                new Vector3(1.15f, 2.2f, 0f), Vector3.one * 0.8f, 120);
            ProjectInstallerWorldFactory.CreateForeground(
                world, "Side Door Right", assets.DoorRightSprite,
                new Vector3(1.95f, 2.2f, 0f), Vector3.one * 0.8f, 120);
        }

        private static void CreateBlockers(Transform world)
        {
            Transform blockers = ProjectInstallerWorldFactory.CreatePoint(
                world, "Furniture Boundaries and Sight Blockers", Vector3.zero);
            CreateOuterBounds(blockers);
            ProjectInstallerWorldFactory.CreateBlocker(
                blockers, "Kitchen Island", new Vector3(1.4f, -0.65f, 0f), new Vector2(4.2f, 1.6f));
            ProjectInstallerWorldFactory.CreateBlocker(
                blockers, "Kitchen Counter", new Vector3(4.7f, -0.75f, 0f), new Vector2(1.1f, 3.1f));
            ProjectInstallerWorldFactory.CreateBlocker(
                blockers, "Balcony Pool", new Vector3(-6.3f, -0.4f, 0f), new Vector2(4.1f, 2.5f));
            ProjectInstallerWorldFactory.CreateBlocker(
                blockers, "Balcony Furniture", new Vector3(-3.1f, 3.25f, 0f), new Vector2(2.7f, 1.0f));
            ProjectInstallerWorldFactory.CreateBlocker(
                blockers, "Living Wall", new Vector3(2.15f, 2.75f, 0f), new Vector2(0.55f, 2.6f));
        }

        private static void CreateOuterBounds(Transform parent)
        {
            ProjectInstallerWorldFactory.CreateBlocker(
                parent, "North Wall", new Vector3(0f, 5.15f, 0f), new Vector2(20f, 0.45f));
            ProjectInstallerWorldFactory.CreateBlocker(
                parent, "South Wall", new Vector3(0f, -5.15f, 0f), new Vector2(20f, 0.45f));
            ProjectInstallerWorldFactory.CreateBlocker(
                parent, "West Wall", new Vector3(-9.4f, 0f, 0f), new Vector2(0.45f, 10.3f));
            ProjectInstallerWorldFactory.CreateBlocker(
                parent, "East Wall", new Vector3(9.4f, 0f, 0f), new Vector2(0.45f, 10.3f));
        }

        private static void BindRuntime(
            Transform world,
            Day2Objects objects,
            ProjectInstallAssets assets)
        {
            GameObject runtimeObject = ProjectInstallerWorldFactory.CreatePoint(
                world, "Day 2 Runtime", Vector3.zero).gameObject;
            Day2Runtime runtime = runtimeObject.AddComponent<Day2Runtime>();
            ProjectInstallerSerialization.Edit(runtime, serialized =>
            {
                ProjectInstallerSerialization.Reference(serialized, "config", assets.Day2Config);
                ProjectInstallerSerialization.Reference(serialized, "openingDialogue", assets.Day2Dialogue);
                ProjectInstallerSerialization.Reference(serialized, "player", objects.Player);
                ProjectInstallerSerialization.Reference(serialized, "banana", objects.Banana);
                ProjectInstallerSerialization.Reference(serialized, "balconySunZone", objects.BalconySun);
                ProjectInstallerSerialization.Reference(serialized, "livingRoomSunZone", objects.LivingSun);
                ProjectInstallerSerialization.Reference(serialized, "feederZone", objects.FeederZone);
                ProjectInstallerSerialization.Reference(serialized, "sideDoorZone", objects.SideDoor);
                ProjectInstallerSerialization.Reference(serialized, "bananaPathGoal", objects.BananaGoal);
                ProjectInstallerSerialization.Reference(serialized, "bananaSlipZone", objects.SlipZone);
                ProjectInstallerSerialization.Reference(serialized, "mainCameraSensor", objects.MainCamera);
                ProjectInstallerSerialization.Reference(serialized, "backupCameraSensor", objects.BackupCamera);
                ProjectInstallerSerialization.Reference(serialized, "robot", objects.Robot);
                ProjectInstallerSerialization.Reference(serialized, "feederImpact", objects.FeederImpact);
                ProjectInstallerSerialization.Reference(serialized, "feederVisual", objects.FeederVisual);
                ProjectInstallerSerialization.Reference(serialized, "playerSpawn", objects.PlayerSpawn);
                ProjectInstallerSerialization.References(serialized, "quietEndingPath", objects.QuietEndingPath);
            });
        }

        private static void SaveScene(Scene scene)
        {
            if (!EditorSceneManager.SaveScene(scene, ProjectInstallerPaths.Main2Scene))
            {
                throw new System.InvalidOperationException("Main2 scene could not be saved.");
            }
        }

        private sealed class Day2Objects
        {
            public PlayerController Player;
            public Carryable Banana;
            public TriggerZone BalconySun;
            public TriggerZone LivingSun;
            public TriggerZone FeederZone;
            public TriggerZone SideDoor;
            public GoalZone BananaGoal;
            public BananaSlipZone SlipZone;
            public CameraSensor MainCamera;
            public CameraSensor BackupCamera;
            public RobotPatrol Robot;
            public Transform FeederImpact;
            public SpriteRenderer FeederVisual;
            public Transform PlayerSpawn;
            public Transform[] QuietEndingPath;
        }
    }
}
