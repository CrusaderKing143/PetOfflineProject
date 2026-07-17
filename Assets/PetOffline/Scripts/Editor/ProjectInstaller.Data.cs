using System;
using System.IO;
using PetOffline.Gameplay;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace PetOffline.Editor
{
    internal static class ProjectInstallerData
    {
        private const string Day1BackgroundPath =
            "Assets/Resources/scene1_bg_animation/bg.png";
        private const string Day2BackgroundPath =
            "Assets/Resources/scene/场景2/bg.png";

        public static ProjectInstallAssets CreateAssets()
        {
            var assets = new ProjectInstallAssets
            {
                PlayerConfig = GetOrCreate<PlayerConfig>("PlayerConfig.asset"),
                CarryableConfig = GetOrCreate<CarryableConfig>("CarryableConfig.asset"),
                CameraConfig = GetOrCreate<CameraScanConfig>("CameraScanConfig.asset"),
                Day1Config = GetOrCreate<Day1Config>("Day1Config.asset"),
                Day2Config = GetOrCreate<Day2Config>("Day2Config.asset"),
                Day1Dialogue = GetOrCreate<DialogueScript>("Day1OpeningDialogue.asset"),
                Day2Dialogue = GetOrCreate<DialogueScript>("Day2OpeningDialogue.asset")
            };

            ConfigureConfigs(assets);
            ConfigureDialogues(assets);
            GenerateBackgroundCrops();
            GenerateUiCutouts();
            LoadArtAssets(assets);
            CalculateGeneratedPositions(assets);
            AssetDatabase.SaveAssets();
            return assets;
        }

        private static T GetOrCreate<T>(string fileName) where T : ScriptableObject
        {
            string path = ProjectInstallerPaths.DataRoot + "/" + fileName;
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void ConfigureConfigs(ProjectInstallAssets assets)
        {
            ConfigurePlayer(assets.PlayerConfig);
            ConfigureCarryable(assets.CarryableConfig);
            ConfigureCamera(assets.CameraConfig);
            ConfigureDay1(assets.Day1Config);
            ConfigureDay2(assets.Day2Config);
        }

        private static void ConfigurePlayer(PlayerConfig config)
        {
            ProjectInstallerSerialization.Edit(config, serialized =>
            {
                ProjectInstallerSerialization.Require(serialized, "moveSpeed").floatValue = 2.8f;
                ProjectInstallerSerialization.Require(serialized, "dashDuration").floatValue = 0.25f;
                ProjectInstallerSerialization.Require(serialized, "dashSpeedMultiplier").floatValue = 2.5f;
                ProjectInstallerSerialization.Require(serialized, "dashCooldown").floatValue = 1f;
                ProjectInstallerSerialization.Require(serialized, "interactionRadius").floatValue = 1f;
                ProjectInstallerSerialization.Require(serialized, "isometricVerticalScale").floatValue = 0.5f;
            });
        }

        private static void ConfigureCarryable(CarryableConfig config)
        {
            ProjectInstallerSerialization.Edit(config, serialized =>
            {
                ProjectInstallerSerialization.Require(serialized, "dropDistance").floatValue = 0.65f;
                ProjectInstallerSerialization.Require(serialized, "pillowMoveMultiplier").floatValue = 0.6f;
                ProjectInstallerSerialization.Require(serialized, "droppedLinearDrag").floatValue = 2f;
            });
        }

        private static void ConfigureCamera(CameraScanConfig config)
        {
            ProjectInstallerSerialization.Edit(config, serialized =>
            {
                ProjectInstallerSerialization.Require(serialized, "distance").floatValue = 12f;
                ProjectInstallerSerialization.Require(serialized, "halfAngle").floatValue = 30f;
                ProjectInstallerSerialization.Require(serialized, "discoveryDuration").floatValue = 1.2f;
                ProjectInstallerSerialization.Require(serialized, "minimumScanAngle").floatValue = -35f;
                ProjectInstallerSerialization.Require(serialized, "maximumScanAngle").floatValue = 35f;
                ProjectInstallerSerialization.Require(serialized, "scanDegreesPerSecond").floatValue = 18f;
                ProjectInstallerSerialization.Require(serialized, "obstructionMask").intValue =
                    1 << ProjectInstallerPaths.SightBlockerLayer;
            });
        }

        private static void ConfigureDay1(Day1Config config)
        {
            ProjectInstallerSerialization.Edit(config, serialized =>
            {
                ProjectInstallerSerialization.Require(serialized, "shoesHoldSeconds").floatValue = 2f;
                ProjectInstallerSerialization.Require(serialized, "firstBossCallSeconds").floatValue = 14f;
                ProjectInstallerSerialization.Require(serialized, "laterBossCallSeconds").floatValue = 26f;
                ProjectInstallerSerialization.Require(serialized, "responseWindowSeconds").floatValue = 3.6f;
                ProjectInstallerSerialization.Require(serialized, "safeWindowSeconds").floatValue = 3f;
                ProjectInstallerSerialization.Require(serialized, "alertSeconds").floatValue = 7f;
            });
        }

        private static void ConfigureDay2(Day2Config config)
        {
            ProjectInstallerSerialization.Edit(config, serialized =>
            {
                ProjectInstallerSerialization.Require(serialized, "firstSunSeconds").floatValue = 10f;
                ProjectInstallerSerialization.Require(serialized, "ignoredConfirmationSeconds").floatValue = 9f;
                ProjectInstallerSerialization.Require(serialized, "backupLessonSeconds").floatValue = 10f;
                ProjectInstallerSerialization.Require(serialized, "finalSunSeconds").floatValue = 20f;
                ProjectInstallerSerialization.Require(serialized, "restoreConfirmationSeconds").floatValue = 4f;
                ProjectInstallerSerialization.Require(serialized, "quietEndingSeconds").floatValue = 9f;
            });
        }

        private static void ConfigureDialogues(ProjectInstallAssets assets)
        {
            SetDialogue(assets.Day1Dialogue, new[]
            {
                new DialogueLine { speaker = "BOSS", text = "Latte, keep the apartment tidy while I am away." },
                new DialogueLine { speaker = "LATTE", text = "The cameras are watching. I can still fix a few things." },
                new DialogueLine { speaker = "BOSS", text = "Start with the shoes, then return my pillow." }
            });
            SetDialogue(assets.Day2Dialogue, new[]
            {
                new DialogueLine { speaker = "OWNER", text = "Good morning, Latte. Stay where the cameras can see you." },
                new DialogueLine { speaker = "LATTE", text = "The balcony is warm, but every quiet moment asks for confirmation." },
                new DialogueLine { speaker = "OWNER", text = "Come back to the feeder whenever I call." }
            });
        }

        private static void SetDialogue(DialogueScript script, DialogueLine[] lines)
        {
            ProjectInstallerSerialization.Edit(script, serialized =>
            {
                SerializedProperty property = ProjectInstallerSerialization.Require(serialized, "lines");
                property.arraySize = lines.Length;
                for (int index = 0; index < lines.Length; index++)
                {
                    SerializedProperty line = property.GetArrayElementAtIndex(index);
                    line.FindPropertyRelative("speaker").stringValue = lines[index].speaker;
                    line.FindPropertyRelative("text").stringValue = lines[index].text;
                }
            });
        }

        private static void GenerateBackgroundCrops()
        {
            WithReadableTexture(Day1BackgroundPath, texture =>
            {
                RectInt shoesRect = Day1ShoesRect(texture);
                WriteShoeVisual(texture, shoesRect, GeneratedPath("Day1Shoes.png"));
                WriteCrop(texture, Day1PatchSourceRect(texture), GeneratedPath("Day1ShoesFloorPatch.png"));
            });
            WithReadableTexture(Day2BackgroundPath, texture =>
            {
                WriteCrop(texture, Day2PatchSourceRect(texture), GeneratedPath("Day2BananaFloorPatch.png"));
            });
        }

        private static void GenerateUiCutouts()
        {
            WriteCheckerCutout(
                "Assets/Resources/UI/begin/title.png",
                GeneratedPath("TitleClean.png"));
            WriteCheckerCutout(
                "Assets/Resources/UI/begin/yellow.png",
                GeneratedPath("ButtonYellowClean.png"));
            WriteCheckerCutout(
                "Assets/Resources/UI/begin/blue.png",
                GeneratedPath("ButtonBlueClean.png"));
        }

        private static void WriteCheckerCutout(string sourcePath, string outputPath)
        {
            WithReadableTexture(sourcePath, texture =>
            {
                Color[] pixels = texture.GetPixels();
                for (int index = 0; index < pixels.Length; index++)
                {
                    pixels[index] = RemoveCheckerPixel(pixels[index]);
                }

                WriteTexture(texture.width, texture.height, pixels, outputPath);
            });
        }

        private static Color RemoveCheckerPixel(Color color)
        {
            float value = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
            float minimum = Mathf.Min(color.r, Mathf.Min(color.g, color.b));
            if (value - minimum > 0.035f || value < 0.9f)
            {
                return color;
            }

            color.a = 0f;
            return color;
        }

        private static void WithReadableTexture(string path, Action<Texture2D> action)
        {
            byte[] bytes = File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                if (!ImageConversion.LoadImage(texture, bytes, false))
                {
                    throw new InvalidOperationException($"PNG data could not be decoded: {path}");
                }

                action(texture);
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }

        private static void WriteCrop(Texture2D source, RectInt rect, string outputPath)
        {
            Color[] pixels = source.GetPixels(rect.x, rect.y, rect.width, rect.height);
            WriteTexture(rect.width, rect.height, pixels, outputPath);
        }

        private static void WriteShoeVisual(Texture2D source, RectInt rect, string outputPath)
        {
            Color[] pixels = source.GetPixels(rect.x, rect.y, rect.width, rect.height);
            for (int y = 0; y < rect.height; y++)
            {
                for (int x = 0; x < rect.width; x++)
                {
                    float u = (x + 0.5f) / rect.width;
                    float v = (y + 0.5f) / rect.height;
                    bool visible = InsideEllipse(u, v, 0.36f, 0.55f, 0.25f, 0.25f, -12f)
                        || InsideEllipse(u, v, 0.68f, 0.43f, 0.26f, 0.24f, -12f);
                    if (!visible)
                    {
                        pixels[y * rect.width + x].a = 0f;
                    }
                }
            }

            WriteTexture(rect.width, rect.height, pixels, outputPath);
        }

        private static bool InsideEllipse(
            float x,
            float y,
            float centerX,
            float centerY,
            float radiusX,
            float radiusY,
            float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float dx = x - centerX;
            float dy = y - centerY;
            float rotatedX = Mathf.Cos(radians) * dx - Mathf.Sin(radians) * dy;
            float rotatedY = Mathf.Sin(radians) * dx + Mathf.Cos(radians) * dy;
            return rotatedX * rotatedX / (radiusX * radiusX)
                + rotatedY * rotatedY / (radiusY * radiusY) <= 1f;
        }

        private static void WriteTexture(int width, int height, Color[] pixels, string outputPath)
        {
            var output = new Texture2D(width, height, TextureFormat.RGBA32, false);
            output.SetPixels(pixels);
            output.Apply(false, false);
            File.WriteAllBytes(outputPath, output.EncodeToPNG());
            Object.DestroyImmediate(output);
            AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceUpdate);
            ConfigureGeneratedSprite(outputPath);
        }

        private static void ConfigureGeneratedSprite(string path)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private static void LoadArtAssets(ProjectInstallAssets assets)
        {
            assets.Day1Background = ProjectInstallerAssets.LoadSprite(Day1BackgroundPath);
            assets.Day2Background = ProjectInstallerAssets.LoadSprite(Day2BackgroundPath);
            assets.Day1Shoes = ProjectInstallerAssets.LoadSprite(GeneratedPath("Day1Shoes.png"));
            assets.Day1ShoesPatch = ProjectInstallerAssets.LoadSprite(GeneratedPath("Day1ShoesFloorPatch.png"));
            assets.Day2BananaPatch = ProjectInstallerAssets.LoadSprite(GeneratedPath("Day2BananaFloorPatch.png"));
            LoadGameplayArt(assets);
            LoadUiArt(assets);
        }

        private static void LoadGameplayArt(ProjectInstallAssets assets)
        {
            assets.PlayerSprite = ProjectInstallerAssets.LoadSprite(
                "Assets/Resources/dog_animation/dog_walk/dog_walk1/合成 1/walk1_00000.png");
            assets.PillowSprite = ProjectInstallerAssets.LoadSprite(
                "Assets/Resources/item_animation/baozhen/animationbaozhen/baozhen-drop2_00.png");
            assets.BananaSprite = ProjectInstallerAssets.LoadSprite(
                "Assets/Resources/item_animation/banana1/banana/香蕉-run_00.png");
            assets.RobotSprite = ProjectInstallerAssets.LoadSprite(
                "Assets/Resources/Sweepingrobot_animation/扫地机/scene1/扫地机右下待机1/合成 5/扫地机右下待机1_00000.png");
            assets.CameraSprite = ProjectInstallerAssets.LoadSprite(
                "Assets/Resources/camera/processed_frames_98_1783923284564/processed_frame_001.png");
            assets.PlayerController = ProjectInstallerAssets.LoadRequired<RuntimeAnimatorController>(
                "Assets/Resources/dog_animation/Animations/dog.controller");
            assets.RobotController = ProjectInstallerAssets.LoadRequired<RuntimeAnimatorController>(
                "Assets/Resources/Sweepingrobot_animation/Animations/Sweepingrobot.controller");
            assets.FeederSprite = ProjectInstallerAssets.LoadSprite(
                "Assets/Resources/scene/场景2/食物机器.png");
            assets.DoorLeftSprite = ProjectInstallerAssets.LoadSprite(
                "Assets/Resources/scene/场景2/door-mid-left.png");
            assets.DoorRightSprite = ProjectInstallerAssets.LoadSprite(
                "Assets/Resources/scene/场景2/door-mid-right.png");
        }

        private static void LoadUiArt(ProjectInstallAssets assets)
        {
            assets.TitleSprite = ProjectInstallerAssets.LoadSprite(GeneratedPath("TitleClean.png"));
            assets.PrimaryButtonSprite = ProjectInstallerAssets.LoadSprite(GeneratedPath("ButtonYellowClean.png"));
            assets.BlueButtonSprite = ProjectInstallerAssets.LoadSprite(GeneratedPath("ButtonBlueClean.png"));
            assets.MissionSprite = ProjectInstallerAssets.LoadSprite("Assets/Resources/UI/Mission_List/Mission_List.png");
            assets.Day2MissionSprite = ProjectInstallerAssets.LoadSprite(
                "Assets/Resources/UI/Mission_List/Mission_List2.png");
            assets.ConversationSprite = ProjectInstallerAssets.LoadSprite("Assets/Resources/UI/conversation/boss1.png");
            assets.OwnerPortraitSprite = ProjectInstallerAssets.LoadSprite(
                "Assets/Resources/UI/conversation/owner1.png");
            assets.RobotPortraitSprite = ProjectInstallerAssets.LoadSprite(
                "Assets/Resources/UI/conversation/robot1.png");
            assets.LatteBackgroundSprite = ProjectInstallerAssets.LoadSprite("Assets/Resources/UI/Latte/background.png");
            assets.LatteGreenSprite = ProjectInstallerAssets.LoadSprite("Assets/Resources/UI/Latte/green.png");
            assets.Day1ReportSprite = ProjectInstallerAssets.LoadSprite("Assets/Resources/UI/report/day1.png");
            assets.Day2ReportSprite = ProjectInstallerAssets.LoadSprite("Assets/Resources/UI/report/day2.png");
            assets.IllustrateWordsSprite = ProjectInstallerAssets.LoadSprite("Assets/Resources/UI/illustrate/words.png");
            assets.IllustrateIconSprite = ProjectInstallerAssets.LoadSprite("Assets/Resources/UI/illustrate/icon.png");
            assets.WhiteSprite = ProjectInstallerAssets.LoadSprite(
                "Assets/Resources/UI/Latte/white.png");
            assets.Font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            assets.OpeningVideo = ProjectInstallerAssets.LoadRequired<VideoClip>(
                "Assets/Resources/UI/begin/737c3940-dee2-4c3a-99f1-b5a0b01c9066.mp4");
            assets.OpeningRenderTexture = ProjectInstallerAssets.LoadRequired<RenderTexture>(
                "Assets/Resources/BeginVideo.renderTexture");
        }

        private static void CalculateGeneratedPositions(ProjectInstallAssets assets)
        {
            Texture2D day1 = ProjectInstallerAssets.LoadRequired<Texture2D>(Day1BackgroundPath);
            Texture2D day2 = ProjectInstallerAssets.LoadRequired<Texture2D>(Day2BackgroundPath);
            assets.Day1ShoesPosition = PixelCenterToWorld(
                Day1ShoesRect(day1), day1, assets.Day1Background.bounds);
            assets.Day2BananaPosition = PixelCenterToWorld(
                Day2BananaRect(day2), day2, assets.Day2Background.bounds);
            assets.Day1GeneratedScale = 100f / SourcePixelsPerUnit(day1, assets.Day1Background);
            assets.Day2GeneratedScale = 100f / SourcePixelsPerUnit(day2, assets.Day2Background);
        }

        private static Vector3 PixelCenterToWorld(
            RectInt rect,
            Texture2D texture,
            Bounds backgroundBounds)
        {
            float normalizedX = rect.center.x / texture.width - 0.5f;
            float normalizedY = rect.center.y / texture.height - 0.5f;
            float x = backgroundBounds.center.x + normalizedX * backgroundBounds.size.x;
            float y = backgroundBounds.center.y + normalizedY * backgroundBounds.size.y;
            return new Vector3(x, y, 0f);
        }

        private static float SourcePixelsPerUnit(Texture2D texture, Sprite background)
        {
            return texture.width / background.bounds.size.x;
        }

        private static RectInt Day1ShoesRect(Texture2D texture)
        {
            return NormalizedRect(texture, 0.545f, 0.675f, 0.075f, 0.09f);
        }

        private static RectInt Day1PatchSourceRect(Texture2D texture)
        {
            return NormalizedRect(texture, 0.545f, 0.565f, 0.075f, 0.09f);
        }

        private static RectInt Day2BananaRect(Texture2D texture)
        {
            return NormalizedRect(texture, 0.555f, 0.17f, 0.08f, 0.105f);
        }

        private static RectInt Day2PatchSourceRect(Texture2D texture)
        {
            return NormalizedRect(texture, 0.455f, 0.17f, 0.08f, 0.105f);
        }

        private static RectInt NormalizedRect(
            Texture2D texture,
            float x,
            float y,
            float width,
            float height)
        {
            int pixelX = Mathf.RoundToInt(texture.width * x);
            int pixelY = Mathf.RoundToInt(texture.height * y);
            int pixelWidth = Mathf.RoundToInt(texture.width * width);
            int pixelHeight = Mathf.RoundToInt(texture.height * height);
            return new RectInt(pixelX, pixelY, pixelWidth, pixelHeight);
        }

        private static string GeneratedPath(string fileName)
        {
            return ProjectInstallerPaths.GeneratedRoot + "/" + fileName;
        }
    }
}
