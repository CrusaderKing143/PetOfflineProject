using System;
using UnityEngine;

namespace PetOffline.Gameplay
{
    [Serializable]
    public struct DialogueLine
    {
        public string speaker;
        [TextArea(2, 5)] public string text;
    }

    [CreateAssetMenu(menuName = "Pet Offline/Dialogue Script")]
    public sealed class DialogueScript : ScriptableObject
    {
        [SerializeField] private DialogueLine[] lines = Array.Empty<DialogueLine>();

        public int Count => lines.Length;

        public DialogueLine GetLine(int index)
        {
            if (lines.Length == 0)
            {
                return default;
            }

            return lines[Mathf.Clamp(index, 0, lines.Length - 1)];
        }
    }
}
