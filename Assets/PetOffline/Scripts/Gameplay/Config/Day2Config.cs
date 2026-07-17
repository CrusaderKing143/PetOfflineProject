using UnityEngine;

namespace PetOffline.Gameplay
{
    [CreateAssetMenu(menuName = "Pet Offline/Day 2 Config")]
    public sealed class Day2Config : ScriptableObject
    {
        [SerializeField, Min(0.1f)] private float firstSunSeconds = 10f;
        [SerializeField, Min(0.1f)] private float ignoredConfirmationSeconds = 9f;
        [SerializeField, Min(0.1f)] private float backupLessonSeconds = 10f;
        [SerializeField, Min(0.1f)] private float finalSunSeconds = 20f;
        [SerializeField, Min(0.1f)] private float restoreConfirmationSeconds = 4f;
        [SerializeField, Min(0.1f)] private float quietEndingSeconds = 9f;

        public float FirstSunSeconds => firstSunSeconds;
        public float IgnoredConfirmationSeconds => ignoredConfirmationSeconds;
        public float BackupLessonSeconds => backupLessonSeconds;
        public float FinalSunSeconds => finalSunSeconds;
        public float RestoreConfirmationSeconds => restoreConfirmationSeconds;
        public float QuietEndingSeconds => quietEndingSeconds;
    }
}
