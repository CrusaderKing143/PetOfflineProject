using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ItemPlacementMissionController : MonoBehaviour
{
    [Serializable]
    private sealed class MissionStep
    {
        [SerializeField] private DogCarryItemType requiredItemType;
        [SerializeField] private Collider2D targetArea;
        [SerializeField] private GameObject completionIndicator;

        public bool Matches(DogCarryItemPickup pickup)
        {
            return pickup.ItemType == requiredItemType
                && targetArea != null
                && targetArea.OverlapPoint(pickup.transform.position);
        }

        public void SetCompletionVisible(bool visible)
        {
            if (completionIndicator != null)
            {
                completionIndicator.SetActive(visible);
            }
        }
    }

    [SerializeField] private DogController dogController;
    [SerializeField] private MissionStep[] missionSteps = Array.Empty<MissionStep>();

    private int currentStepIndex;

    private void Awake()
    {
        foreach (MissionStep step in missionSteps)
        {
            step.SetCompletionVisible(false);
        }
    }

    private void OnEnable()
    {
        if (dogController != null)
        {
            dogController.ItemDropped += HandleItemDropped;
        }
    }

    private void OnDisable()
    {
        if (dogController != null)
        {
            dogController.ItemDropped -= HandleItemDropped;
        }
    }

    private void HandleItemDropped(DogCarryItemPickup pickup)
    {
        if (pickup == null || currentStepIndex >= missionSteps.Length)
        {
            return;
        }

        MissionStep currentStep = missionSteps[currentStepIndex];
        if (!currentStep.Matches(pickup))
        {
            return;
        }

        pickup.Lock();
        currentStep.SetCompletionVisible(true);
        currentStepIndex++;
    }
}
