using PetOffline.Gameplay;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PetOffline.Editor
{
    internal static class ProjectInstallerWorldFactory
    {
        public static SpriteRenderer CreateBackground(
            Transform parent,
            string name,
            Sprite sprite)
        {
            GameObject target = CreateObject(parent, name, Vector3.zero, 0);
            SpriteRenderer renderer = target.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = -1000;
            return renderer;
        }

        public static SpriteRenderer CreatePatch(
            Transform parent,
            string name,
            Sprite sprite,
            Vector3 position,
            float scale)
        {
            GameObject target = CreateObject(parent, name, position, 0);
            target.transform.localScale = Vector3.one * scale;
            SpriteRenderer renderer = target.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = -999;
            return renderer;
        }

        public static PlayerController CreatePlayer(
            Transform parent,
            Vector3 position,
            ProjectInstallAssets assets)
        {
            GameObject target = CreateObject(
                parent, "Player", position, ProjectInstallerPaths.PlayerLayer);
            target.transform.localScale = Vector3.one * 0.3f;
            SpriteRenderer renderer = target.AddComponent<SpriteRenderer>();
            renderer.sprite = assets.PlayerSprite;
            Animator animator = target.AddComponent<Animator>();
            animator.runtimeAnimatorController = assets.PlayerController;
            Rigidbody2D body = ConfigureDynamicBody(target.AddComponent<Rigidbody2D>(), 1f);
            var collider = target.AddComponent<CapsuleCollider2D>();
            collider.size = new Vector2(2.6f, 1.8f);
            collider.offset = new Vector2(0f, -0.25f);
            PlayerAnimatorDriver driver = target.AddComponent<PlayerAnimatorDriver>();
            Transform anchor = CreateLocalTransform(
                target.transform, "CarryAnchor", new Vector3(0f, 1.4f, 0f));
            PlayerController player = target.AddComponent<PlayerController>();
            BindPlayer(player, assets, body, collider, driver, anchor);
            BindAnimatorDriver(driver, animator);
            AddSorter(target, renderer, 20);
            return player;
        }

        public static Carryable CreateCarryable(
            Transform parent,
            string name,
            Vector3 position,
            Vector3 scale,
            Sprite sprite,
            CarryableId id,
            PlayerCarryStyle style,
            bool dropOnBark,
            CarryableConfig config)
        {
            GameObject target = CreateObject(
                parent, name, position, ProjectInstallerPaths.CarryableLayer);
            target.transform.localScale = scale;
            SpriteRenderer renderer = target.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            Rigidbody2D body = ConfigureDynamicBody(target.AddComponent<Rigidbody2D>(), 0.4f);
            var collider = target.AddComponent<BoxCollider2D>();
            collider.size = sprite != null ? sprite.bounds.size * 0.72f : Vector2.one;
            Carryable carryable = target.AddComponent<Carryable>();
            ProjectInstallerSerialization.Edit(carryable, serialized =>
            {
                ProjectInstallerSerialization.Require(serialized, "id").enumValueIndex = (int)id;
                ProjectInstallerSerialization.Require(serialized, "carryStyle").enumValueIndex = (int)style;
                ProjectInstallerSerialization.Reference(serialized, "config", config);
                ProjectInstallerSerialization.Require(serialized, "dropOnBark").boolValue = dropOnBark;
                ProjectInstallerSerialization.Reference(serialized, "body", body);
                ProjectInstallerSerialization.Reference(serialized, "itemCollider", collider);
                ProjectInstallerSerialization.Reference(serialized, "itemRenderer", renderer);
            });
            AddSorter(target, renderer, 10);
            return carryable;
        }

        public static GoalZone CreateGoal(
            Transform parent,
            string name,
            Vector3 position,
            Vector2 size,
            ProjectInstallAssets assets,
            Color markerColor)
        {
            GameObject target = CreateObject(
                parent, name, position, ProjectInstallerPaths.GameplayTriggerLayer);
            var collider = target.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = size;
            GoalZone goal = target.AddComponent<GoalZone>();
            ProjectInstallerSerialization.Edit(goal, serialized =>
                ProjectInstallerSerialization.Reference(serialized, "zoneCollider", collider));
            CreateMarker(target.transform, assets.WhiteSprite, size, markerColor);
            return goal;
        }

        public static TriggerZone CreateTrigger(
            Transform parent,
            string name,
            Vector3 position,
            Vector2 size,
            ProjectInstallAssets assets,
            Color markerColor)
        {
            GameObject target = CreateObject(
                parent, name, position, ProjectInstallerPaths.GameplayTriggerLayer);
            var collider = target.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = size;
            TriggerZone trigger = target.AddComponent<TriggerZone>();
            if (markerColor.a > 0f)
            {
                CreateMarker(target.transform, assets.WhiteSprite, size, markerColor);
            }

            return trigger;
        }

        public static BananaSlipZone CreateSlipZone(
            Transform parent,
            Vector3 position,
            Vector2 size,
            Carryable banana)
        {
            GameObject target = CreateObject(
                parent, "BananaSlipZone", position, ProjectInstallerPaths.GameplayTriggerLayer);
            var collider = target.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = size;
            BananaSlipZone zone = target.AddComponent<BananaSlipZone>();
            ProjectInstallerSerialization.Edit(zone, serialized =>
            {
                ProjectInstallerSerialization.Reference(serialized, "banana", banana);
                ProjectInstallerSerialization.Require(serialized, "playerSlideDirection").vector2Value =
                    new Vector2(1f, -0.35f).normalized;
            });
            return zone;
        }

        public static CameraSensor CreateSensor(
            Transform parent,
            string name,
            Vector3 position,
            float baseAngle,
            Transform target,
            ProjectInstallAssets assets)
        {
            GameObject sensorObject = CreateObject(parent, name, position, 0);
            sensorObject.transform.rotation = Quaternion.Euler(0f, 0f, baseAngle);
            Transform head = CreateLocalTransform(sensorObject.transform, "SensorHead", Vector3.zero);
            SpriteRenderer renderer = head.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = assets.CameraSprite;
            renderer.sortingOrder = 40;
            head.localScale = Vector3.one * 0.34f;
            CameraSensor sensor = sensorObject.AddComponent<CameraSensor>();
            ProjectInstallerSerialization.Edit(sensor, serialized =>
            {
                ProjectInstallerSerialization.Reference(serialized, "config", assets.CameraConfig);
                ProjectInstallerSerialization.Reference(serialized, "sensorHead", head);
                ProjectInstallerSerialization.Reference(serialized, "target", target);
            });
            return sensor;
        }

        public static RobotPatrol CreateRobot(
            Transform parent,
            Vector3 position,
            Transform[] waypoints,
            ProjectInstallAssets assets)
        {
            GameObject target = CreateObject(
                parent, "Robot", position, ProjectInstallerPaths.RobotLayer);
            target.transform.localScale = Vector3.one * 0.42f;
            SpriteRenderer renderer = target.AddComponent<SpriteRenderer>();
            renderer.sprite = assets.RobotSprite;
            Animator animator = target.AddComponent<Animator>();
            animator.runtimeAnimatorController = assets.RobotController;
            Rigidbody2D body = ConfigureDynamicBody(target.AddComponent<Rigidbody2D>(), 4f);
            var collider = target.AddComponent<CircleCollider2D>();
            collider.radius = 0.8f;
            RobotPatrol robot = target.AddComponent<RobotPatrol>();
            ProjectInstallerSerialization.Edit(robot, serialized =>
            {
                ProjectInstallerSerialization.Reference(serialized, "body", body);
                ProjectInstallerSerialization.Reference(serialized, "animator", animator);
                ProjectInstallerSerialization.References(serialized, "waypoints", waypoints);
                ProjectInstallerSerialization.Require(serialized, "speed").floatValue = 1.4f;
            });
            AddSorter(target, renderer, 15);
            return robot;
        }

        public static Transform[] CreateWaypoints(
            Transform parent,
            string groupName,
            params Vector3[] positions)
        {
            Transform group = CreateTransform(parent, groupName, Vector3.zero);
            var waypoints = new Transform[positions.Length];
            for (int index = 0; index < positions.Length; index++)
            {
                waypoints[index] = CreateTransform(
                    group, "Waypoint " + (index + 1), positions[index]);
            }

            return waypoints;
        }

        public static Transform CreatePoint(Transform parent, string name, Vector3 position)
        {
            return CreateTransform(parent, name, position);
        }

        public static void CreateBlocker(
            Transform parent,
            string name,
            Vector3 position,
            Vector2 size)
        {
            GameObject target = CreateObject(
                parent, name, position, ProjectInstallerPaths.SightBlockerLayer);
            var collider = target.AddComponent<BoxCollider2D>();
            collider.size = size;
        }

        public static SpriteRenderer CreateForeground(
            Transform parent,
            string name,
            Sprite sprite,
            Vector3 position,
            Vector3 scale,
            int order)
        {
            GameObject target = CreateObject(parent, name, position, 0);
            target.transform.localScale = scale;
            SpriteRenderer renderer = target.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            return renderer;
        }

        private static GameObject CreateObject(
            Transform parent,
            string name,
            Vector3 position,
            int layer)
        {
            var target = new GameObject(name);
            target.transform.SetParent(parent, false);
            target.transform.position = position;
            target.layer = layer;
            return target;
        }

        private static Transform CreateTransform(
            Transform parent,
            string name,
            Vector3 position)
        {
            GameObject target = CreateObject(parent, name, position, 0);
            return target.transform;
        }

        private static Transform CreateLocalTransform(
            Transform parent,
            string name,
            Vector3 localPosition)
        {
            var target = new GameObject(name);
            target.transform.SetParent(parent, false);
            target.transform.localPosition = localPosition;
            return target.transform;
        }

        private static Rigidbody2D ConfigureDynamicBody(Rigidbody2D body, float mass)
        {
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.mass = mass;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            return body;
        }

        private static void BindPlayer(
            PlayerController player,
            ProjectInstallAssets assets,
            Rigidbody2D body,
            Collider2D collider,
            PlayerAnimatorDriver driver,
            Transform anchor)
        {
            ProjectInstallerSerialization.Edit(player, serialized =>
            {
                ProjectInstallerSerialization.Reference(serialized, "config", assets.PlayerConfig);
                ProjectInstallerSerialization.Reference(serialized, "carryableConfig", assets.CarryableConfig);
                ProjectInstallerSerialization.Reference(serialized, "body", body);
                ProjectInstallerSerialization.Reference(serialized, "bodyCollider", collider);
                ProjectInstallerSerialization.Reference(serialized, "animatorDriver", driver);
                ProjectInstallerSerialization.Reference(serialized, "carryAnchor", anchor);
                ProjectInstallerSerialization.Require(serialized, "interactionMask").intValue =
                    1 << ProjectInstallerPaths.CarryableLayer;
            });
        }

        private static void BindAnimatorDriver(PlayerAnimatorDriver driver, Animator animator)
        {
            ProjectInstallerSerialization.Edit(driver, serialized =>
                ProjectInstallerSerialization.Reference(serialized, "animator", animator));
        }

        private static void AddSorter(GameObject target, SpriteRenderer renderer, int offset)
        {
            YAxisSorter sorter = target.AddComponent<YAxisSorter>();
            ProjectInstallerSerialization.Edit(sorter, serialized =>
            {
                ProjectInstallerSerialization.Reference(serialized, "targetRenderer", renderer);
                ProjectInstallerSerialization.Require(serialized, "offset").intValue = offset;
            });
        }

        private static void CreateMarker(
            Transform parent,
            Sprite sprite,
            Vector2 size,
            Color color)
        {
            var marker = new GameObject("Marker");
            marker.transform.SetParent(parent, false);
            SpriteRenderer renderer = marker.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = -500;
            Vector2 bounds = sprite != null ? sprite.bounds.size : Vector2.one;
            marker.transform.localScale = new Vector3(
                size.x / Mathf.Max(0.01f, bounds.x),
                size.y / Mathf.Max(0.01f, bounds.y),
                1f);
        }
    }
}
