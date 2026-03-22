using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// Central audio coordinator. Manages:
    ///   - Lens ambient drones (one per active lens, crossfaded on lens change)
    ///   - Narrative motifs (one-shot stingers for story beats)
    ///   - UI / puzzle sound effects
    ///   - Haptic feedback (iOS)
    ///
    /// Attach to the GameManager prefab. Persists across scenes.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        // ── Lens drones ───────────────────────────────────────────────────────────
        [Header("Drone Sources — two for seamless crossfade")]
        [SerializeField] private AudioSource droneA;
        [SerializeField] private AudioSource droneB;
        [SerializeField] private float crossfadeDuration = 1.2f;

        // ── Motif source (one-shot narrative stingers) ────────────────────────────
        [Header("Motif / SFX Source")]
        [SerializeField] private AudioSource motifSource;

        // ── Sound clips ───────────────────────────────────────────────────────────
        [Header("Motifs — assign in Inspector")]
        [SerializeField] private AudioClip panelRestoredClip;
        [SerializeField] private AudioClip lensUnlockClip;
        [SerializeField] private AudioClip pageFlipClip;
        [SerializeField] private AudioClip corruptionFlashClip;
        [SerializeField] private AudioClip gutterEntityClip;
        [SerializeField] private AudioClip knowledgeKeyCollectedClip;
        [SerializeField] private AudioClip t5UnlockClip;
        [SerializeField] private AudioClip djedBarActivatedClip;
        [SerializeField] private AudioClip circuitCloseClip;

        // ── Haptic payload map ─────────────────────────────────────────────────────
        private enum HapticEvent
        {
            Tap, LongPressStart, LongPressComplete,
            PanelRestore, LensSwitch, GutterEntity, Corruption
        }

        private bool _droneBActive;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            LensSystem.Instance.OnLensChanged  += HandleLensChanged;
            LensSystem.Instance.OnLensUnlocked += _ => PlayMotif(lensUnlockClip);
            if (NarrativeState.Instance != null)
                NarrativeState.Instance.OnT5Unlocked += () => PlayMotif(t5UnlockClip);
        }

        private void OnDisable()
        {
            if (LensSystem.Instance != null)
            {
                LensSystem.Instance.OnLensChanged  -= HandleLensChanged;
                LensSystem.Instance.OnLensUnlocked -= _ => PlayMotif(lensUnlockClip);
            }
        }

        // ── Lens drone crossfade ──────────────────────────────────────────────────

        public void PlayDroneForLens(LensType lens)
        {
            var def = GameManager.Instance.GetLensDefinition(lens);
            if (def?.ambientNote == null) return;
            CrossfadeDrone(def.ambientNote);
            Haptic(HapticEvent.LensSwitch);
        }

        private void HandleLensChanged(LensType _, LensType next) => PlayDroneForLens(next);

        private void CrossfadeDrone(AudioClip newClip)
        {
            AudioSource incoming = _droneBActive ? droneA : droneB;
            AudioSource outgoing = _droneBActive ? droneB : droneA;
            _droneBActive = !_droneBActive;

            incoming.clip = newClip;
            incoming.loop = true;
            incoming.volume = 0f;
            incoming.Play();

            StopAllCoroutines();
            StartCoroutine(CrossfadeCoroutine(outgoing, incoming, crossfadeDuration));
        }

        private static IEnumerator CrossfadeCoroutine(AudioSource from, AudioSource to, float duration)
        {
            float t = 0f;
            float startVol = from.volume;
            while (t < duration)
            {
                t += Time.deltaTime;
                float ratio = t / duration;
                from.volume = Mathf.Lerp(startVol, 0f, ratio);
                to.volume   = Mathf.Lerp(0f, 1f, ratio);
                yield return null;
            }
            from.Stop();
        }

        // ── Motifs ────────────────────────────────────────────────────────────────

        private void PlayMotif(AudioClip clip)
        {
            if (clip == null || motifSource == null) return;
            motifSource.PlayOneShot(clip);
        }

        /// <summary>
        /// Public motif API used by puzzle/faction scripts.
        /// </summary>
        public void PlayMotif(MotifType motif)
        {
            switch (motif)
            {
                case MotifType.PanelRestored:        PlayMotif(panelRestoredClip); break;
                case MotifType.LensUnlock:           PlayMotif(lensUnlockClip); break;
                case MotifType.PageFlip:             PlayMotif(pageFlipClip); break;
                case MotifType.CorruptionFlash:      PlayMotif(corruptionFlashClip); break;
                case MotifType.GutterEntity:         PlayMotif(gutterEntityClip); break;
                case MotifType.KnowledgeKeyCollected:PlayMotif(knowledgeKeyCollectedClip); break;
                case MotifType.T5Unlock:             PlayMotif(t5UnlockClip); break;
                case MotifType.DjedBarActivated:     PlayMotif(djedBarActivatedClip); break;
                case MotifType.CircuitClose:         PlayMotif(circuitCloseClip); break;
            }
        }

        /// <summary>Direct clip playback (used by ArkAssemblyPuzzle individual notes).</summary>
        public void PlayOneShot(AudioClip clip) => PlayMotif(clip);

        public void OnPanelRestored()
        {
            PlayMotif(panelRestoredClip);
            Haptic(HapticEvent.PanelRestore);
        }

        public void OnPageFlip()     => PlayMotif(pageFlipClip);
        public void OnCorruption()
        {
            PlayMotif(corruptionFlashClip);
            Haptic(HapticEvent.Corruption);
        }

        public void OnGutterEntity()
        {
            PlayMotif(gutterEntityClip);
            Haptic(HapticEvent.GutterEntity);
        }

        public void OnKnowledgeKeyCollected() => PlayMotif(knowledgeKeyCollectedClip);
        public void OnDjedBarActivated()      => PlayMotif(djedBarActivatedClip);
        public void OnCircuitClose()          => PlayMotif(circuitCloseClip);

        /// <summary>
        /// All-chord finale: plays all five lens drones simultaneously at equal volume.
        /// Used for the Issue 12 circuit close cinematic.
        /// </summary>
        public void PlayCircuitFinale()
        {
            // Spawn five one-shot sources and play each lens note simultaneously
            var lenses = new[]
            {
                LensType.Mythic, LensType.Technologic, LensType.Symbolic,
                LensType.Political, LensType.Spiritual
            };
            foreach (var lens in lenses)
            {
                var def = GameManager.Instance.GetLensDefinition(lens);
                if (def?.ambientNote != null) motifSource.PlayOneShot(def.ambientNote, 0.7f);
            }
            PlayMotif(circuitCloseClip);
        }

        // ── Haptics ───────────────────────────────────────────────────────────────

        private static void Haptic(HapticEvent evt)
        {
            switch (evt)
            {
                case HapticEvent.LongPressComplete:
                case HapticEvent.PanelRestore:
                    global::ArchiveOfEchoes.Haptic.Play(HapticFeedback.ImpactMedium);
                    break;
                case HapticEvent.GutterEntity:
                case HapticEvent.LensSwitch:
                    global::ArchiveOfEchoes.Haptic.Play(HapticFeedback.ImpactLight);
                    break;
                case HapticEvent.Corruption:
                    global::ArchiveOfEchoes.Haptic.Play(HapticFeedback.ImpactHeavy);
                    break;
                default:
                    global::ArchiveOfEchoes.Haptic.Play(HapticFeedback.ImpactLight);
                    break;
            }
        }
    }
}
