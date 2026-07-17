using PetOffline.Gameplay;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PetOffline.Editor
{
    internal static class ProjectInstallerDay1
    {
        public static void Build(ProjectInstallAssets assets)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var world = new GameObject("World");
            ProjectInstallerWorldFactory.CreateBackground(world.transform, "Static Background", assets.Day1Background);
            ProjectInstallerWorldFactory.CreatePatch(
                world.transform, "Shoes Floor Patch", assets.Day1ShoesPatch,
                assets.Day1ShoesPosition, assets.Day1GeneratedScale);

            Day1Objects objects = CreateGameplayObjects(world.transform, assets);
            CreateCameraA(world.transform, objects.Player.transform, assets);
            CreateBananaHazard(world.transform, assets);
            CreateBlockers(world.transform);
            BindRuntime(world.transform, objects, assets);
            SaveScene(scene);
        }

        private static Day1Objects CreateGameplayObjects(
            Transform world,
            ProjectInstallAssets assets)
        {
            var result = new Day1Objects();
            result.PlayerSpawn = ProjectInstallerWorldFactory.CreatePoint(
                world, "Player Spawn", new Vector3(-3.2f, -2.45f, 0f));
            result.Player = ProjectInstallerWorldFactory.CreatePlayer(
                world, result.PlayerSpawn.position, assets);
            result.Shoes = ProjectInstallerWorldFactory.CreateCarryable(
                world, "Shoes", assets.Day1ShoesPosition,
                Vector3.one * assets.Day1GeneratedScale, assets.Day1Shoes,
                CarryableId.Shoes, PlayerCarryStyle.Shoes, false, assets.CarryableConfig);
            result.Pillow = ProjectInstallerWorldFactory.CreateCarryable(
                world, "Pillow", new Vector3(-1.4f, -1.55f, 0f), Vector3.one * 0.4f,
                assets.PillowSprite, CarryableId.Pillow, PlayerCarryStyle.Pillow, true,
                assets.CarryableConfig);
            CreateZones(world, assets, result);
            CreateRobotAndPath(world, assets, result);
            result.CameraB = ProjectInstallerWorldFactory.CreateSensor(
                world, "Camera B", new Vector3(5.75f, 4.0f, 0f), 118f,
                result.Player.transform, assets);
            result.EndingPath = ProjectInstallerWorldFactory.CreateWaypoints(
                world, "Ending Path",
                new Vector3(-3.4f, -2.7f, 0f),
                new Vector3(-5.4f, -3.5f, 0f),
                new Vector3(-7.0f, -3.9f, 0f));
            return result;
        }

        private static void CreateZones(
            Transform world,
            ProjectInstallAssets assets,
            Day1Objects result)
        {
            result.ShoesGoal = ProjectInstallerWorldFactory.CreateGoal(
                world, "Camera A Goal", new Vector3(-5.45f, 0.75f, 0f),
                new Vector2(1.9f, 1.25f), assets, new Color(0.25f, 0.75f, 1f, 0.2f));
            result.PillowGoal = ProjectInstallerWorldFactory.CreateGoal(
                world, "Pillow Goal", new Vector3(-5.0f, -1.65f, 0f),
                new Vector2(1.8f, 1.25f), assets, new Color(0.45f, 0.95f, 0.45f, 0.18f));
            result.DogBed = ProjectInstallerWorldFactory.CreateTrigger(
                world, "Dog Bed", new Vector3(-5.0f, -1.65f, 0f),
                new Vector2(2.0f, 1.4f), assets, new Color(1f, 0.75f, 0.3f, 0.08f));
        }

        private static void CreateRobotAndPath(
            Transform world,
            ProjectInstallAssets assets,
            Day1Objects result)
        {
            Transform[] patrol = ProjectInstallerWorldFactory.CreateWaypoints(
                world, "Robot Patrol",
                new Vector3(-2.25f, -1.65f, 0f),
                new Vector3(-4.4f, -1.65f, 0f),
                new Vector3(-4.4f, -2.1f, 0f),
                new Vector3(3.4f, -2.1f, 0f),
                new Vector3(3.4f, -1.65f, 0f),
                new Vector3(3.4f, -2.1f, 0f),
                new Vector3(-2.25f, -2.1f, 0f));
            result.Robot = ProjectInstallerWorldFactory.CreateRobot(
                world, patrol[0].position, patrol, assets);
        }

        private static void CreateCameraA(
            Transform world,
            Transform player,
            ProjectInstallAssets assets)
        {
            CameraSensor cameraA = ProjectInstallerWorldFactory.CreateSensor(
                world, "Camera A", new Vector3(-6.6f, 2.8f, 0f), -150f, player, assets);
            cameraA.enabled = false;
        }

        private static void CreateBananaHazard(
            Transform world,
            ProjectInstallAssets assets)
        {
            Vector3 position = new Vector3(3.2f, -2.45f, 0f);
            ProjectInstallerWorldFactory.CreateForeground(
                world, "Banana Hazard", assets.BananaSprite,
                position, Vector3.one * 0.7f, 260);
            ProjectInstallerWorldFactory.CreateSlipZone(
                world, position, new Vector2(1.3f, 0.85f), null);
        }

        private static void CreateBlockers(Transform world)
        {
            Transform blockers = ProjectInstallerWorldFactory.CreatePoint(
                world, "Furniture Boundaries and Sight Blockers", Vector3.zero);
            CreateOuterBounds(blockers);
            ProjectInstallerWorldFactory.CreateBlocker(
                blockers, "Sofa", new Vector3(0.6f, -0.25f, 0f), new Vector2(4.6f, 1.7f));
            ProjectInstallerWorldFactory.CreateBlocker(
                blockers, "Coffee Table", new Vector3(1.6f, -0.8f, 0f), new Vector2(2.0f, 1.15f));
            ProjectInstallerWorldFactory.CreateBlocker(
                blockers, "TV Console", new Vector3(4.45f, 0.65f, 0f), new Vector2(3.2f, 0.75f));
            ProjectInstallerWorldFactory.CreateBlocker(
                blockers, "Cabinet", new Vector3(-2.75f, 2.45f, 0f), new Vector2(2.0f, 1.0f));
            ProjectInstallerWorldFactory.CreateBlocker(
                blockers, "Lamp", new Vector3(0.15f, 1.35f, 0f), new Vector2(0.6f, 0.6f));
        }

        private static void CreateOuterBounds(Transform parent)
        {
            ProjectInstallerWorldFactory.CreateBlocker(
                parent, "North Wall", new Vector3(0f, 4.75f, 0f), new Vector2(18.5f, 0.45f));
            ProjectInstallerWorldFactory.CreateBlocker(
                parent, "South Wall", new Vector3(0f, -4.75f, 0f), new Vector2(18.5f, 0.45f));
            ProjectInstallerWorldFactory.CreateBlocker(
                parent, "West Wall", new Vector3(-8.9f, 0f, 0f), new Vector2(0.45f, 9.5f));
            ProjectInstallerWorldFactory.CreateBlocker(
                parent, "East Wall", new Vector3(8.9f, 0f, 0f), new Vector2(0.45f, 9.5f));
        }

        private static void BindRuntime(
            Transform world,
            Day1Objects objects,
            ProjectInstallAssets assets)
        {
            GameObject runtimeObject = ProjectInstallerWorldFactory.CreatePoint(
                world, "Day 1 Runtime", Vector3.zero).gameObject;
            Day1Runtime runtime = runtimeObject.AddComponent<Day1Runtime>();
            ProjectInstallerSerialization.Edit(runtime, serialized =>
            {
                ProjectInstallerSerialization.Reference(serialized, "config", assets.Day1Config);
                ProjectInstallerSerialization.Reference(serialized, "openingDialogue", assets.Day1Dialogue);
                ProjectInstallerSerialization.Reference(serialized, "player", objects.Player);
                ProjectInstallerSerialization.Reference(serialized, "shoes", objects.Shoes);
                ProjectInstallerSerialization.Reference(serialized, "pillow", objects.Pillow);
                ProjectInstallerSerialization.Reference(serialized, "shoesGoal", objects.ShoesGoal);
                ProjectInstallerSerialization.Reference(serialized, "pillowGoal", objects.PillowGoal);
                ProjectInstallerSerialization.Reference(serialized, "dogBed", objects.DogBed);
                ProjectInstallerSerialization.Reference(serialized, "cameraB", objects.CameraB);
                ProjectInstallerSerialization.Reference(serialized, "robot", objects.Robot);
                ProjectInstallerSerialization.Reference(serialized, "playerSpawn", objects.PlayerSpawn);
                ProjectInstallerSerialization.References(serialized, "endingPath", objects.EndingPath);
            });
        }

        private static void SaveScene(Scene scene)
        {
            if (!EditorSceneManager.SaveScene(scene, ProjectInstallerPaths.Main1Scene))
            {
                throw new System.InvalidOperationException("Main1 scene could not be saved.");
            }
        }

        private sealed class Day1Objects
        {
            public PlayerController Player;
            public Carryable Shoes;
            public Carryable Pillow;
            public GoalZone ShoesGoal;
            public GoalZone PillowGoal;
            public TriggerZone DogBed;
            public CameraSensor CameraB;
            public RobotPatrol Robot;
            public Transform PlayerSpawn;
            public Transform[] EndingPath;
        }
    }
}
