using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PetOffline.Tests
{
    internal static class SmokeScreenshotCapture
    {
        private const int Width = 1920;
        private const int Height = 1080;

        public static void WritePng(string path)
        {
            Camera camera = Object.FindObjectOfType<Camera>();
            if (camera == null)
            {
                throw new InvalidOperationException("No camera is available for the smoke-test screenshot.");
            }

            CanvasState[] canvasStates = PrepareCanvases(camera);
            RenderTexture target = RenderTexture.GetTemporary(Width, Height, 24, RenderTextureFormat.ARGB32);
            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            try
            {
                camera.targetTexture = target;
                Canvas.ForceUpdateCanvases();
                camera.Render();
                WriteRenderTexture(path, target);
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                RestoreCanvases(canvasStates);
                RenderTexture.ReleaseTemporary(target);
            }
        }

        private static CanvasState[] PrepareCanvases(Camera camera)
        {
            Canvas[] canvases = Object.FindObjectsOfType<Canvas>();
            CanvasState[] states = canvases.Select(canvas => new CanvasState(canvas)).ToArray();
            foreach (Canvas canvas in canvases)
            {
                if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    continue;
                }

                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 1f;
            }

            return states;
        }

        private static void WriteRenderTexture(string path, RenderTexture target)
        {
            RenderTexture.active = target;
            var texture = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0f, 0f, Width, Height), 0, 0);
            texture.Apply(false, false);
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }

        private static void RestoreCanvases(CanvasState[] states)
        {
            foreach (CanvasState state in states)
            {
                state.Restore();
            }
        }

        private readonly struct CanvasState
        {
            private readonly Canvas canvas;
            private readonly RenderMode renderMode;
            private readonly Camera worldCamera;
            private readonly float planeDistance;

            public CanvasState(Canvas canvas)
            {
                this.canvas = canvas;
                renderMode = canvas.renderMode;
                worldCamera = canvas.worldCamera;
                planeDistance = canvas.planeDistance;
            }

            public void Restore()
            {
                canvas.renderMode = renderMode;
                canvas.worldCamera = worldCamera;
                canvas.planeDistance = planeDistance;
            }
        }
    }
}
