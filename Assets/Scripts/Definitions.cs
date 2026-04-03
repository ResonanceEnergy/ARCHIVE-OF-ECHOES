namespace ArchiveOfEchoes
{
    public enum LensType
    {
        Mythic,       // default — D minor — unlocked from start
        Technologic,  // F# minor — unlocks Issue 02
        Symbolic,     // A minor  — unlocks Issue 03
        Political,    // C minor  — unlocks Issue 04
        Spiritual     // E major  — unlocks Issue 04
    }

    public enum PanelType
    {
        Static,     // no interaction required
        Stabilize,  // B-series: long-press corruption puzzle
        Reorder,    // A-series: drag panels to correct sequence
        Interact,   // tap / tap-hold to trigger content
        Overlay,    // A4: multi-page overlay reveal
        Gutter      // gutter entity / door panels
    }

    public enum CorruptionState
    {
        Clean,
        Corrupted,
        Restoring,
        Restored
    }

    public enum PageTransition
    {
        PageTurn,       // standard left/right comic page flip
        InkDive,        // pinch-in: zoom into panel micro-scene
        LensSwitch,     // lens radial closes and filter crossfades
        PanelResolve,   // puzzle complete: panel snaps clean
        GutterEntity,   // gutter text materialises from white space
        Corruption      // red-shift flash when Scribes attack
    }

    public enum NarrativePhase
    {
        Frame2100,      // opening / closing 2100 world
        ComicReader,    // standard page-reading mode
        PanelEntry,     // inside a panel micro-scene
        PuzzleOverlay,  // full-screen puzzle is active
        IssueComplete   // end-of-issue summary / transition
    }

    public enum EndingVariant
    {
        LockedStability,   // Mythic or Political lens at capstone placement
        ResonantHarmony,   // Symbolic or Spiritual lens at capstone placement
        GutterPath         // Technologic lens, or all Gutter content completed
    }

    public enum PageLayout
    {
        FourPanel,   // 2×2 grid
        ThreePanel,  // three unequal panels
        TwoPanel,    // left/right split
        FullBleed,   // single full-page panel
        Strip,       // horizontal strip of 3–4 narrow panels
        Cinematic    // one wide + two small stacked
    }

    /// <summary>Named motif tokens used by AudioManager.PlayMotif(MotifType).</summary>
    public enum MotifType
    {
        PanelRestored,
        LensUnlock,
        PageFlip,
        CorruptionFlash,
        GutterEntity,
        KnowledgeKeyCollected,
        T5Unlock,
        DjedBarActivated,
        CircuitClose,
        PaperRustle,    // foley: physical page crinkle (Phase 5)
        CapstonePlaced, // stone-on-stone seating click (Phase 5)
        FinaleChord,    // held all-lens resolution chord (Phase 5)
    }

    /// <summary>iOS haptic feedback tokens (iOS-only; no-op on other platforms).</summary>
    public enum HapticFeedback
    {
        ImpactLight,
        ImpactMedium,
        ImpactHeavy
    }

    /// <summary>
    /// Cross-platform haptic helper. Puzzle scripts call
    /// <c>Haptic.Play(HapticFeedback.ImpactMedium)</c> and this class
    /// routes to the correct platform API.
    /// </summary>
    public static class Haptic
    {
        public static void Play(HapticFeedback feedback)
        {
#if UNITY_IOS && !UNITY_EDITOR
            UnityEngine.Handheld.Vibrate();
#endif
        }
    }
}
