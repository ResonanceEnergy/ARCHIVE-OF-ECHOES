using UnityEngine;

namespace ArchiveOfEchoes
{
    [CreateAssetMenu(menuName = "Archive/Panel Data", fileName = "Panel_")]
    public class PanelData : ScriptableObject
    {
        [Header("Identity")]
        public string panelId;
        public PanelType panelType;

        [Header("Visual")]
        public Sprite panelArtwork;
        public Sprite corruptedOverlay;

        [Header("Lens Variants")]
        public LensPanelVariant[] lensVariants;

        [Header("Narrative")]
        [TextArea(2, 5)] public string captionText;
        [TextArea(1, 3)] public string archivistNote;
        [TextArea(1, 3)] public string gutterText;

        [Header("Corruption")]
        public bool startsCorrupted;
        [Range(0f, 1f)] public float corruptionLevel = 0.8f;

        [Header("Puzzle")]
        public PuzzleConfig puzzleConfig;

        [Header("Branch")]
        public bool isBranchPoint;
        public BranchOption[] branchOptions;

        [Header("Knowledge Keys")]
        public KnowledgeKeyData[] revealsKeys;

        [Header("Dual-Lens Lock")]
        public bool requiresDualLens;
        public LensType requiredLensA;
        public LensType requiredLensB;
    }

    [System.Serializable]
    public class LensPanelVariant
    {
        public LensType lens;
        public Sprite altArtwork;
        [TextArea(1, 3)] public string altCaption;
        public bool revealsHiddenContent;
    }

    [System.Serializable]
    public class PuzzleConfig
    {
        public PanelType puzzleType;
        [Tooltip("Seconds player must hold for B1 Stabilize puzzle")]
        public float stabilizeDuration = 2f;
        [Tooltip("Number of panels in A1 Panel Reorder puzzle")]
        public int reorderPanelCount = 4;
        public bool isOptional;
    }

    [System.Serializable]
    public class BranchOption
    {
        public string label;
        public string targetIssueId;
        public int targetPageIndex;
        [Tooltip("Leave empty to allow any lens")]
        public LensType[] requiredLenses;
    }
}
