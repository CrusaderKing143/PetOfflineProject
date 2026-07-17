using System;
using PetOffline.Gameplay;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace PetOffline.Editor
{
    internal static class ProjectInstallerPaths
    {
        public const string StartPanelScene = "Assets/Scenes/StartPanel.unity";
        public const string Main1Scene = "Assets/Scenes/Main1.unity";
        public const string Main2Scene = "Assets/Scenes/Main2.unity";
        public const string DataRoot = "Assets/PetOffline/Data";
        public const string GeneratedRoot = DataRoot + "/Generated";

        public const int SightBlockerLayer = 8;
        public const int GameplayTriggerLayer = 9;
        public const int PlayerLayer = 10;
        public const int CarryableLayer = 11;
        public const int RobotLayer = 12;
    }

    internal sealed class ProjectInstallAssets
    {
        public PlayerConfig PlayerConfig;
        public CarryableConfig CarryableConfig;
        public CameraScanConfig CameraConfig;
        public Day1Config Day1Config;
        public Day2Config Day2Config;
        public DialogueScript Day1Dialogue;
        public DialogueScript Day2Dialogue;

        public Sprite Day1Background;
        public Sprite Day2Background;
        public Sprite Day1Shoes;
        public Sprite Day1ShoesPatch;
        public Sprite Day2BananaPatch;
        public Vector3 Day1ShoesPosition;
        public Vector3 Day2BananaPosition;
        public float Day1GeneratedScale;
        public float Day2GeneratedScale;

        public Sprite PlayerSprite;
        public Sprite PillowSprite;
        public Sprite BananaSprite;
        public Sprite RobotSprite;
        public Sprite CameraSprite;
        public RuntimeAnimatorController PlayerController;
        public RuntimeAnimatorController RobotController;

        public Sprite TitleSprite;
        public Sprite PrimaryButtonSprite;
        public Sprite BlueButtonSprite;
        public Sprite MissionSprite;
        public Sprite Day2MissionSprite;
        public Sprite ConversationSprite;
        public Sprite OwnerPortraitSprite;
        public Sprite RobotPortraitSprite;
        public Sprite LatteBackgroundSprite;
        public Sprite LatteGreenSprite;
        public Sprite Day1ReportSprite;
        public Sprite Day2ReportSprite;
        public Sprite IllustrateWordsSprite;
        public Sprite IllustrateIconSprite;
        public Sprite FeederSprite;
        public Sprite DoorLeftSprite;
        public Sprite DoorRightSprite;
        public Sprite WhiteSprite;
        public Font Font;
        public VideoClip OpeningVideo;
        public RenderTexture OpeningRenderTexture;
    }

    internal static class ProjectInstallerSerialization
    {
        public static void Edit(Object target, Action<SerializedObject> edit)
        {
            var serialized = new SerializedObject(target);
            serialized.Update();
            edit(serialized);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        public static SerializedProperty Require(SerializedObject serialized, string propertyName)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException(
                    $"{serialized.targetObject.GetType().Name}.{propertyName} was not found.");
            }

            return property;
        }

        public static void Reference(
            SerializedObject serialized,
            string propertyName,
            Object value)
        {
            Require(serialized, propertyName).objectReferenceValue = value;
        }

        public static void References(
            SerializedObject serialized,
            string propertyName,
            Object[] values)
        {
            SerializedProperty property = Require(serialized, propertyName);
            property.arraySize = values.Length;
            for (int index = 0; index < values.Length; index++)
            {
                property.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
            }
        }
    }

    internal static class ProjectInstallerAssets
    {
        public static T LoadRequired<T>(string path) where T : Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                throw new InvalidOperationException($"Required asset is missing: {path}");
            }

            return asset;
        }

        public static Sprite LoadSprite(string path)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
            {
                return sprite;
            }

            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer))
            {
                throw new InvalidOperationException($"Required sprite is missing: {path}");
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
            return LoadRequired<Sprite>(path);
        }
    }
}
