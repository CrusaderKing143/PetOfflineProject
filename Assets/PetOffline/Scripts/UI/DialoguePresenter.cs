using PetOffline.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PetOffline.UI
{
    [DisallowMultipleComponent]
    public sealed class DialoguePresenter : SessionPresenterBase
    {
        [SerializeField] private GameObject dialogueRoot;
        [SerializeField] private Image portraitImage;
        [SerializeField] private Sprite bossPortrait;
        [SerializeField] private Sprite ownerPortrait;
        [SerializeField] private Sprite robotPortrait;
        [SerializeField] private Text speakerText;
        [SerializeField] private Text dialogueText;

        protected override void Redraw()
        {
            ILevelViewModel viewModel = ViewModel;
            bool visible = viewModel != null && viewModel.UiMode == LevelUiMode.Dialogue;
            SetActive(dialogueRoot, visible);
            if (!visible)
            {
                return;
            }

            if (speakerText != null)
            {
                speakerText.text = viewModel.DialogueSpeaker ?? string.Empty;
            }

            DrawPortrait(viewModel.DialogueSpeaker);

            if (dialogueText != null)
            {
                dialogueText.text = viewModel.DialogueText ?? string.Empty;
            }
        }

        private void DrawPortrait(string speaker)
        {
            if (portraitImage == null)
            {
                return;
            }

            Sprite portrait = speaker == "BOSS"
                ? bossPortrait
                : speaker == "OWNER" ? ownerPortrait : speaker == "ROBOT" ? robotPortrait : null;
            portraitImage.sprite = portrait;
            portraitImage.enabled = portrait != null;
        }
    }
}
