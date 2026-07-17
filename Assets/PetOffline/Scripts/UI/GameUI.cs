using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace PetOffline
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(GameSession))]
    public sealed class GameUI : MonoBehaviour
    {
        [Header("Title")]
        [SerializeField] private GameObject titleRoot;
        [SerializeField] private Button newGameButton;
        [SerializeField] private GameObject videoRoot;
        [SerializeField] private RawImage videoImage;
        [SerializeField] private VideoPlayer openingVideo;

        [Header("HUD")]
        [SerializeField] private GameObject hudRoot;
        [SerializeField] private Image missionBackground;
        [SerializeField] private Sprite day1MissionSprite;
        [SerializeField] private Sprite day2MissionSprite;
        [SerializeField] private Text objectiveText;
        [SerializeField] private Image progressFill;
        [SerializeField] private Text progressText;
        [SerializeField] private Text taskCountText;
        [SerializeField] private GameObject[] taskCheckmarks;

        [Header("Camera State")]
        [SerializeField] private GameObject cameraStateRoot;
        [SerializeField] private Text cameraStateText;
        [SerializeField] private Color safeColor;
        [SerializeField] private Color scanningColor;
        [SerializeField] private Color alertColor;
        [SerializeField] private Color offlineColor;

        [Header("Dialogue")]
        [SerializeField] private GameObject dialogueRoot;
        [SerializeField] private Image portraitImage;
        [SerializeField] private Sprite bossPortrait;
        [SerializeField] private Sprite ownerPortrait;
        [SerializeField] private Sprite robotPortrait;
        [SerializeField] private Text speakerText;
        [SerializeField] private Text dialogueText;

        [Header("Report")]
        [SerializeField] private GameObject reportRoot;
        [SerializeField] private Image reportImage;
        [SerializeField] private Button reportContinueButton;
        [SerializeField] private Sprite day1ReportSprite;
        [SerializeField] private Sprite day2ReportSprite;

        [Header("Choice")]
        [SerializeField] private GameObject choiceRoot;
        [SerializeField] private Button restoreConnectionButton;
        [SerializeField] private Button keepQuietButton;

        [Header("Ending")]
        [SerializeField] private GameObject endingRoot;
        [SerializeField] private Text endingText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button endingReturnTitleButton;

        [Header("Pause")]
        [SerializeField] private GameObject pauseRoot;
        [SerializeField] private Text pauseText;
        [SerializeField] private Button pauseReturnTitleButton;
        [SerializeField] private string pausedMessage;

        private GameSession session;

        private void Awake()
        {
            session = GetComponent<GameSession>();
            newGameButton.onClick.AddListener(session.NewGame);
            reportContinueButton.onClick.AddListener(session.ContinueReport);
            restoreConnectionButton.onClick.AddListener(
                () => session.SubmitChoice(FinalChoice.RestoreConnection));
            keepQuietButton.onClick.AddListener(
                () => session.SubmitChoice(FinalChoice.KeepQuiet));
            restartButton.onClick.AddListener(session.Restart);
            endingReturnTitleButton.onClick.AddListener(session.ReturnTitle);
            pauseReturnTitleButton.onClick.AddListener(session.ReturnTitle);
        }

        public void ShowTitle()
        {
            HideAll();
            titleRoot.SetActive(true);
            videoRoot.SetActive(true);
            videoImage.enabled = true;
            newGameButton.interactable = true;
            openingVideo.isLooping = true;
            openingVideo.Play();
        }

        public void ShowLoading()
        {
            HideAll();
        }

        public void BeginLevel(bool day2, int totalTasks)
        {
            StopVideo();
            titleRoot.SetActive(false);
            missionBackground.sprite = day2 ? day2MissionSprite : day1MissionSprite;
            SetTasks(0, totalTasks);
            SetProgress(0f, string.Empty);
            SetCamera(CameraUiState.Hidden);
            ShowWorld();
        }

        public void ShowWorld()
        {
            HideStoryPanels();
            hudRoot.SetActive(true);
        }

        public void ShowDialogue(string speaker, string text)
        {
            HideStoryPanels();
            hudRoot.SetActive(true);
            dialogueRoot.SetActive(true);
            speakerText.text = speaker;
            dialogueText.text = text;
            DrawPortrait(speaker);
        }

        public void ShowReport(bool day2)
        {
            HideStoryPanels();
            reportRoot.SetActive(true);
            reportImage.sprite = day2 ? day2ReportSprite : day1ReportSprite;
            reportContinueButton.interactable = true;
        }

        public void ShowChoice()
        {
            HideStoryPanels();
            choiceRoot.SetActive(true);
            restoreConnectionButton.interactable = true;
            keepQuietButton.interactable = true;
        }

        public void ShowEnding(string text)
        {
            HideStoryPanels();
            endingRoot.SetActive(true);
            endingText.text = text;
            restartButton.interactable = true;
            endingReturnTitleButton.interactable = true;
        }

        public void ShowPause(bool visible)
        {
            pauseRoot.SetActive(visible);
            pauseText.text = visible ? pausedMessage : string.Empty;
            pauseReturnTitleButton.interactable = visible;
        }

        public void SetObjective(string text)
        {
            objectiveText.text = text;
        }

        public void SetProgress(float value, string label)
        {
            float progress = Mathf.Clamp01(value);
            progressFill.fillAmount = progress;
            progressText.text = string.IsNullOrEmpty(label)
                ? Mathf.RoundToInt(progress * 100f) + "%"
                : label;
        }

        public void SetTasks(int completed, int total)
        {
            taskCountText.text = completed + "/" + total;
            for (int index = 0; index < taskCheckmarks.Length; index++)
            {
                taskCheckmarks[index].SetActive(index < completed);
            }
        }

        public void SetCamera(CameraUiState state)
        {
            cameraStateRoot.SetActive(state != CameraUiState.Hidden);
            cameraStateText.text = CameraLabel(state);
            cameraStateText.color = CameraColor(state);
        }

        private void HideAll()
        {
            StopVideo();
            titleRoot.SetActive(false);
            pauseRoot.SetActive(false);
            HideStoryPanels();
        }

        private void HideStoryPanels()
        {
            hudRoot.SetActive(false);
            dialogueRoot.SetActive(false);
            reportRoot.SetActive(false);
            choiceRoot.SetActive(false);
            endingRoot.SetActive(false);
        }

        private void StopVideo()
        {
            if (openingVideo.isPlaying)
            {
                openingVideo.Pause();
            }

            videoImage.enabled = false;
            videoRoot.SetActive(false);
        }

        private void DrawPortrait(string speaker)
        {
            Sprite portrait = speaker == "BOSS"
                ? bossPortrait
                : speaker == "OWNER" ? ownerPortrait : speaker == "ROBOT" ? robotPortrait : null;
            portraitImage.sprite = portrait;
            portraitImage.enabled = portrait;
        }

        private static string CameraLabel(CameraUiState state)
        {
            switch (state)
            {
                case CameraUiState.Safe: return "SAFE";
                case CameraUiState.Scanning: return "SCANNING";
                case CameraUiState.Alert: return "ALERT";
                case CameraUiState.Offline: return "OFFLINE";
                default: return string.Empty;
            }
        }

        private Color CameraColor(CameraUiState state)
        {
            switch (state)
            {
                case CameraUiState.Safe: return safeColor;
                case CameraUiState.Alert: return alertColor;
                case CameraUiState.Offline: return offlineColor;
                default: return scanningColor;
            }
        }
    }
}
