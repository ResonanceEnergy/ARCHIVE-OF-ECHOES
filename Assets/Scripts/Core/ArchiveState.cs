using System.Collections.Generic;
using UnityEngine;

namespace ArchiveOfEchoes
{
    // ── Enum stubs needed by new puzzle/faction systems ───────────────────────────
    // (MotifType, HapticFeedback are defined in AudioManager.cs; reproduced here as
    //  partial-namespace peers for cross-script access without circular dependencies.)
    /// <summary>
    /// Serialisable player save state. Persisted via JSON in PlayerPrefs.
    /// All game systems read/write through this object.
    /// </summary>
    [System.Serializable]
    public class ArchiveState
    {
        // ── Navigation ──────────────────────────────────────────────────────────
        public string currentIssueId = "issue_00";
        public int currentPageIndex = 0;

        // ── Progress ─────────────────────────────────────────────────────────────
        public List<string> completedIssues = new();
        public List<string> completedPanelIds = new();
        public List<string> collectedKeyIds = new();
        public List<string> completedGutterDoors = new();

        // ── Lenses ───────────────────────────────────────────────────────────────
        public LensType activeLens = LensType.Mythic;
        public List<LensType> unlockedLenses = new() { LensType.Mythic };

        // ── Narrative trackers ───────────────────────────────────────────────────
        public int scribeEscalationLevel = 0;   // 0–5
        public bool resonantsContacted = false;
        public int djedBarCount = 0;             // 0–4
        public bool arkAssembled = false;

        // ── Ending unlock flags ───────────────────────────────────────────────────
        public bool allOptionalPuzzlesComplete = false;
        public bool allGutterDoorsComplete = false;
        public bool allGutterContentComplete = false;

        // ── Persistence ──────────────────────────────────────────────────────────
        private const string SaveKey = "archive_state_v1";

        public void Save()
        {
            string json = JsonUtility.ToJson(this);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        public static ArchiveState Load()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
                return new ArchiveState();

            string json = PlayerPrefs.GetString(SaveKey);
            return JsonUtility.FromJson<ArchiveState>(json) ?? new ArchiveState();
        }

        public static void DeleteSave() => PlayerPrefs.DeleteKey(SaveKey);

        // ── Convenience queries ───────────────────────────────────────────────────
        public bool IsLensUnlocked(LensType lens) => unlockedLenses.Contains(lens);
        public bool HasKey(string keyId) => collectedKeyIds.Contains(keyId);
        public bool IsIssueComplete(string issueId) => completedIssues.Contains(issueId);

        // ── Ark component / ability tracking ─────────────────────────────────────
        public List<string> collectedArkComponents = new();
        public List<string> unlockedArkAbilities   = new();
        public Dictionary<string, string> decreeChoices = new();
        public bool identityMerged            = false;
        public bool resonantProtectionActive  = false;

        public bool HasArkComponent(string id)  => collectedArkComponents.Contains(id);
        public void AddArkComponent(string id)  { if (!HasArkComponent(id)) collectedArkComponents.Add(id); }
        public bool HasArkAbility(string id)    => unlockedArkAbilities.Contains(id);
        public void AddArkAbility(string id)    { if (!HasArkAbility(id)) unlockedArkAbilities.Add(id); }

        /// <summary>Records which fragment the player chose for an ambiguous decree gap.</summary>
        public void RecordDecreeChoice(string gapId, string fragmentId)
        {
            if (decreeChoices == null) decreeChoices = new();
            decreeChoices[gapId] = fragmentId;
        }

        /// <summary>
        /// Determines ending variant from the lens active at Issue 12 capstone placement.
        /// Gutter Path also triggers when all gutter content has been completed.
        /// </summary>
        public EndingVariant DetermineEnding(LensType lensAtPlacement)
        {
            if (allGutterContentComplete || lensAtPlacement == LensType.Technologic)
                return EndingVariant.GutterPath;
            if (lensAtPlacement == LensType.Symbolic || lensAtPlacement == LensType.Spiritual)
                return EndingVariant.ResonantHarmony;
            return EndingVariant.LockedStability;
        }
    }
}
