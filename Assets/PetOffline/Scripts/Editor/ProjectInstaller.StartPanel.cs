using PetOffline.Core;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

namespace PetOffline.Editor
{
    internal static class ProjectInstallerStartPanel
    {
        public static void Build(ProjectInstallAssets assets)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();
            GameSession session = CreateSystems();
            CreateEventSystem();
            Canvas canvas = CreateCanvas();
            CreateUi(canvas, session, assets);
            SaveScene(scene, ProjectInstallerPaths.StartPanelScene);
        }

        private static void CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 7.19f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(
                0.21960784f, 0.16862746f, 0.22745098f, 1f);
            cameraObject.AddComponent<AudioListener>();
        }

        private static GameSession CreateSystems()
        {
            var systems = new GameObject("Systems");
            SceneFlowService sceneFlow = systems.AddComponent<SceneFlowService>();
            GameSession session = systems.AddComponent<GameSession>();
            LegacyInputRouter input = systems.AddComponent<LegacyInputRouter>();

            ProjectInstallerSerialization.Edit(sceneFlow, serialized =>
            {
                ProjectInstallerSerialization.Require(serialized, "startPanelScene").stringValue = "StartPanel";
                ProjectInstallerSerialization.Require(serialized, "day1Scene").stringValue = "Main1";
                ProjectInstallerSerialization.Require(serialized, "day2Scene").stringValue = "Main2";
            });
            ProjectInstallerSerialization.Edit(session, serialized =>
                ProjectInstallerSerialization.Reference(serialized, "sceneFlow", sceneFlow));
            ProjectInstallerSerialization.Edit(input, serialized =>
                ProjectInstallerSerialization.Reference(serialized, "session", session));
            return session;
        }

        private static void CreateEventSystem()
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static Canvas CreateCanvas()
        {
            var root = new GameObject(
                "UIRoot",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static void CreateUi(Canvas canvas, GameSession session, ProjectInstallAssets assets)
        {
            GameObject videoRoot = CreateVideo(canvas.transform, assets, out RawImage rawImage, out VideoPlayer video);
            GameObject presenters = ProjectInstallerUiFactory.CreateRect(canvas.transform, "Presenters");
            ProjectInstallerUiFactory.Stretch(presenters);
            ProjectInstallerUi.BuildTitle(canvas.transform, presenters, session, assets, videoRoot, rawImage, video);
            ProjectInstallerUi.BuildHud(canvas.transform, presenters, session, assets);
            ProjectInstallerUi.BuildDialogue(canvas.transform, presenters, session, assets);
            ProjectInstallerUiOverlays.BuildReport(canvas.transform, presenters, session, assets);
            ProjectInstallerUiOverlays.BuildChoiceAndEnding(canvas.transform, presenters, session, assets);
            ProjectInstallerUiOverlays.BuildPause(canvas.transform, presenters, session, assets);
        }

        private static GameObject CreateVideo(
            Transform canvas,
            ProjectInstallAssets assets,
            out RawImage rawImage,
            out VideoPlayer video)
        {
            GameObject root = ProjectInstallerUiFactory.CreateRect(canvas, "OpeningVideo");
            ProjectInstallerUiFactory.Stretch(root);
            rawImage = root.AddComponent<RawImage>();
            rawImage.texture = assets.OpeningRenderTexture;
            rawImage.color = Color.white;
            rawImage.raycastTarget = false;
            video = root.AddComponent<VideoPlayer>();
            video.playOnAwake = false;
            video.isLooping = true;
            video.source = VideoSource.VideoClip;
            video.clip = assets.OpeningVideo;
            video.renderMode = VideoRenderMode.RenderTexture;
            video.targetTexture = assets.OpeningRenderTexture;
            video.audioOutputMode = VideoAudioOutputMode.Direct;
            video.controlledAudioTrackCount = 1;
            video.EnableAudioTrack(0, true);
            video.SetDirectAudioVolume(0, 1f);
            return root;
        }

        private static void SaveScene(Scene scene, string path)
        {
            if (!EditorSceneManager.SaveScene(scene, path))
            {
                throw new System.InvalidOperationException($"Scene could not be saved: {path}");
            }
        }
    }
}
