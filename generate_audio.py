#!/usr/bin/env python3
"""
generate_audio.py — Archive of Echoes procedural audio synthesis

Synthesises every audio asset the game needs using Python's stdlib only
(wave + math + struct + random).  No pip packages required.

Assets produced (18 total):
  Assets/Audio/Drones/      — 5 ambient looping drone clips (one per lens)
  Assets/Audio/Motifs/      — placeholder motif clip
  Assets/Audio/SFX/         — 12 one-shot SFX clips for AudioManager

Usage:
    python generate_audio.py                      # generate all missing
    python generate_audio.py --force              # overwrite all
    python generate_audio.py --dry-run            # list what would be made
    python generate_audio.py --category drones    # drones only
    python generate_audio.py --category sfx       # SFX only
    python generate_audio.py --id drone_mythic    # single asset

All files are 44100 Hz, 16-bit, mono WAV.
"""

import argparse
import json
import math
import random
import struct
import wave
from pathlib import Path

# ── Config ─────────────────────────────────────────────────────────────────────

PROJECT_ROOT = Path(__file__).parent
MANIFEST     = PROJECT_ROOT / "Assets" / "Audio" / "audio_manifest.json"
SR           = 44100          # sample rate
TAU          = 2.0 * math.pi

# ── Write helpers ──────────────────────────────────────────────────────────────

def _write_wav(path: Path, samples: list[float]) -> None:
    """Write normalised float samples [-1,1] to 16-bit mono WAV."""
    path.parent.mkdir(parents=True, exist_ok=True)
    peak = max((abs(s) for s in samples), default=1.0)
    norm = 0.88 / peak if peak > 0 else 1.0
    packed = struct.pack(
        f"<{len(samples)}h",
        *(max(-32767, min(32767, int(s * norm * 32767))) for s in samples),
    )
    with wave.open(str(path), "wb") as wf:
        wf.setnchannels(1)
        wf.setsampwidth(2)
        wf.setframerate(SR)
        wf.writeframes(packed)


def _write_unity_meta(path: Path) -> None:
    """Write a stub .meta so Unity imports the clip as a compressed AudioClip."""
    meta = path.with_suffix(path.suffix + ".meta")
    if meta.exists():
        return
    import uuid
    meta.write_text(
        f"fileFormatVersion: 2\n"
        f"guid: {uuid.uuid4().hex}\n"
        f"AudioImporter:\n"
        f"  serializedVersion: 6\n"
        f"  defaultSettings:\n"
        f"    loadType: 0\n"          # Decompress on Load
        f"    sampleRateSetting: 0\n"
        f"    sampleRateOverride: 44100\n"
        f"    compressionFormat: 1\n"  # Vorbis
        f"    quality: 0.5\n"
        f"    conversionMode: 0\n"
        f"  forceToMono: 1\n"
        f"  normalize: 1\n"
        f"  preloadAudioData: 0\n"
        f"  ambisonic: 0\n",
        encoding="utf-8",
    )


# ── Envelope helpers ────────────────────────────────────────────────────────────

def _adsr(n: int, a: float, d: float, s_level: float, r: float) -> list[float]:
    """Return an ADSR amplitude envelope of length n (times in seconds)."""
    env = []
    a_n  = int(a * SR)
    d_n  = int(d * SR)
    r_n  = int(r * SR)
    sus_n = max(0, n - a_n - d_n - r_n)
    for i in range(a_n):
        env.append(i / a_n)
    for i in range(d_n):
        env.append(1.0 - (1.0 - s_level) * (i / d_n))
    for _ in range(sus_n):
        env.append(s_level)
    for i in range(r_n):
        env.append(s_level * (1.0 - i / r_n))
    # Trim or pad to exact n
    env = env[:n]
    while len(env) < n:
        env.append(0.0)
    return env


def _fade(n: int, fade_sec: float) -> list[float]:
    """Linear fade-in and fade-out window."""
    fade_n = min(int(fade_sec * SR), n // 2)
    win = [1.0] * n
    for i in range(fade_n):
        win[i]       = i / fade_n
        win[n-1-i]   = i / fade_n
    return win


# ── Synthesis primitives ────────────────────────────────────────────────────────

def _sine(freq: float, t: float, phase: float = 0.0) -> float:
    return math.sin(TAU * freq * t + phase)


def _noise(rng: random.Random) -> float:
    return rng.gauss(0.0, 1.0)


def _lowpass(samples: list[float], coeff: float = 0.15) -> list[float]:
    """Simple single-pole IIR low-pass filter."""
    out = [0.0] * len(samples)
    prev = 0.0
    for i, s in enumerate(samples):
        prev = prev + coeff * (s - prev)
        out[i] = prev
    return out


def _highpass(samples: list[float], coeff: float = 0.92) -> list[float]:
    """Simple single-pole IIR high-pass filter."""
    out = [0.0] * len(samples)
    prev_in = prev_out = 0.0
    for i, s in enumerate(samples):
        out[i] = coeff * (prev_out + s - prev_in)
        prev_in  = s
        prev_out = out[i]
    return out


# ── DRONE synthesis ─────────────────────────────────────────────────────────────
#
# Each drone is 6 seconds at 44100 Hz — long enough for Unity to loop smoothly.
# Architecture: root + 4 harmonics, slow LFO amplitude breathing, slight vibrato.
# The last 0.5 s = first 0.5 s to support seamless loop.

def _gen_drone(
    root_hz: float,
    duration: float = 6.0,
    lfo_hz: float   = 0.09,
    lfo_depth: float = 0.28,
    vib_hz: float   = 0.13,
    vib_depth: float = 0.0018,
    harmonic_amps: tuple = (0.55, 0.22, 0.11, 0.07, 0.05),
    detune_cents: float = 0.0,
    filters: bool = True,
) -> list[float]:
    rng  = random.Random(int(root_hz * 100))
    n    = int(duration * SR)
    fade_win = _fade(n, 0.4)
    detuned  = root_hz * (2 ** (detune_cents / 1200.0))
    samples  = []
    phase_offsets = [rng.uniform(0, TAU) for _ in harmonic_amps]

    for i in range(n):
        t   = i / SR
        lfo = 1.0 - lfo_depth + lfo_depth * _sine(lfo_hz, t)
        vib = 1.0 + vib_depth * _sine(vib_hz, t + 0.3)

        s = 0.0
        for k, amp in enumerate(harmonic_amps):
            harm_mult = k + 1
            freq = detuned * harm_mult * (vib if k == 0 else 1.0)
            s += amp * _sine(freq, t, phase_offsets[k])

        # Very subtle noise floor (like tape hiss)
        s += 0.006 * _noise(rng)
        s *= lfo * fade_win[i]
        samples.append(s)

    if filters:
        samples = _lowpass(samples, 0.35)  # warm low-pass to remove harshness

    return samples


# ── SFX synthesis ───────────────────────────────────────────────────────────────

def _gen_panel_restored(duration: float = 1.4) -> list[float]:
    """Satisfying bell-like resolution chord (D5 = 587 Hz root)."""
    n   = int(duration * SR)
    env = _adsr(n, 0.005, 0.12, 0.0, 1.1)
    root = 587.3
    partials = [
        (root,       0.60, 3.5),   # fundamental
        (root*2.76,  0.25, 5.5),   # 2nd inharmonic
        (root*5.40,  0.10, 9.0),   # 3rd inharmonic (bell upper)
        (root*2,     0.18, 4.0),   # octave
        (root*3,     0.08, 7.0),   # fifth above octave
    ]
    rng = random.Random(42)
    samples = []
    for i in range(n):
        t   = i / SR
        s   = sum(amp * math.exp(-dec * t) * _sine(f, t, rng.uniform(0, 0.1))
                  for f, amp, dec in partials)
        s  *= env[i]
        samples.append(s)
    return samples


def _gen_lens_unlock(duration: float = 1.6) -> list[float]:
    """Shimmering ascending sweep + chime cluster."""
    n   = int(duration * SR)
    env = _adsr(n, 0.02, 0.2, 0.6, 0.6)
    rng = random.Random(7)
    samples = []
    for i in range(n):
        t     = i / SR
        prog  = t / duration
        # Sweep: 300 → 1800 Hz
        sweep_f = 300.0 * (6.0 ** prog)
        s  = 0.40 * _sine(sweep_f, t)
        # Shimmer: random short-lived sine pings
        s += 0.20 * _sine(880 + 440 * prog, t)
        s += 0.12 * _sine(1320, t) * math.exp(-4 * t)
        s += 0.08 * _sine(2200, t) * math.exp(-6 * t)
        s += 0.05 * _noise(rng) * math.exp(-8 * t)
        s *= env[i]
        samples.append(s)
    return samples


def _gen_page_flip(duration: float = 0.32) -> list[float]:
    """Soft paper-noise whoosh."""
    n   = int(duration * SR)
    rng = random.Random(3)
    noise_raw = [_noise(rng) for _ in range(n)]
    # Apply gentle LP filter for "paper" warmth
    lp = _lowpass(noise_raw, 0.25)
    # Attack-decay shape: peak at 15% then fast decay
    t_peak = int(0.15 * n)
    samples = []
    for i in range(n):
        if i <= t_peak:
            env = i / t_peak
        else:
            env = math.exp(-7.0 * (i - t_peak) / SR)
        samples.append(lp[i] * env)
    return samples


def _gen_corruption_flash(duration: float = 0.45) -> list[float]:
    """Glitchy static burst with harsh mid-frequency character."""
    n   = int(duration * SR)
    rng = random.Random(13)
    samples = []
    for i in range(n):
        t = i / SR
        # Square wave at 440 Hz mixed with harsh noise
        sq    = 0.5 if _sine(440.0, t) > 0 else -0.5
        sq   += 0.3 if _sine(880.0, t) > 0 else -0.3
        noise = 0.8 * _noise(rng)
        s     = (0.5 * sq + 0.5 * noise)
        env   = math.exp(-12.0 * t) * (1.0 - math.exp(-60.0 * t))
        samples.append(s * env * 1.2)
    return _highpass(samples, 0.88)


def _gen_gutter_entity(duration: float = 2.2) -> list[float]:
    """Sub-bass rumble + high eerie tone — "something between the panels"."""
    n   = int(duration * SR)
    rng = random.Random(99)
    fade_win = _fade(n, 0.3)
    samples = []
    for i in range(n):
        t   = i / SR
        prog = t / duration
        # Sub-bass (40 Hz) with slight pitch drift
        sub =  0.50 * _sine(40.0 + 3.0 * _sine(0.3, t), t)
        # Eerie sine tone rising slowly (600→900 Hz)
        eerie = 0.15 * _sine(600.0 + 300.0 * prog, t) * math.sin(math.pi * prog) ** 0.5
        # Sparse noise stabs
        noise = 0.08 * _noise(rng) * math.exp(-2 * t)
        s = (sub + eerie + noise) * fade_win[i]
        samples.append(s)
    return _lowpass(samples, 0.8)


def _gen_knowledge_key_collected(duration: float = 0.9) -> list[float]:
    """Crystalline two-note chime — first knowledge key found."""
    n    = int(duration * SR)
    # First note: E5 = 659 Hz at t=0, second note: A5 = 880 Hz at t=0.18
    notes = [(0.00, 659.3, 3.0), (0.18, 880.0, 4.5), (0.32, 1046.5, 6.0)]
    samples = []
    for i in range(n):
        t = i / SR
        s = 0.0
        for note_t, freq, decay in notes:
            dt = t - note_t
            if dt >= 0:
                s += 0.6 * _sine(freq,      dt) * math.exp(-decay * dt)
                s += 0.2 * _sine(freq*2.76, dt) * math.exp(-decay*1.8 * dt)
        samples.append(s)
    return samples


def _gen_t5_unlock(duration: float = 2.8) -> list[float]:
    """Grand chord swell — Tier 5 convergence unlocked."""
    n    = int(duration * SR)
    env  = _adsr(n, 0.08, 0.5, 0.7, 1.5)
    # A major chord: A3/E4/A4/C#5 (220, 330, 440, 554 Hz)
    chord = [
        (220.0, 0.40), (330.0, 0.30), (440.0, 0.30),
        (554.4, 0.20), (880.0, 0.12), (110.0, 0.28),
    ]
    rng = random.Random(77)
    phases = [rng.uniform(0, TAU) for _ in chord]
    samples = []
    for i in range(n):
        t = i / SR
        s = sum(amp * _sine(f, t, phases[j]) for j, (f, amp) in enumerate(chord))
        # Shimmer layer: gentle vibrato
        s += 0.08 * _sine(660.0, t) * math.exp(-1.0 * t)
        s *= env[i]
        samples.append(s)
    return samples


def _gen_djed_bar_activated(duration: float = 0.55) -> list[float]:
    """Mechanical snap then resonant 'pillar' thud."""
    n    = int(duration * SR)
    snap_n = int(0.04 * SR)   # first 40 ms: sharp click
    rng  = random.Random(21)
    samples = []
    for i in range(n):
        t = i / SR
        if i < snap_n:
            # High click
            s = _noise(rng) * (1.0 - i / snap_n) * 0.9
            s += 0.4 * _sine(3200.0, t) * (1.0 - i / snap_n)
        else:
            dt = t - snap_n / SR
            # Deep resonant thud at 80 Hz
            s  = 0.70 * _sine(80.0, dt) * math.exp(-8.0 * dt)
            s += 0.30 * _sine(160.0, dt) * math.exp(-10.0 * dt)
        samples.append(s)
    return samples


def _gen_circuit_close(duration: float = 0.65) -> list[float]:
    """Electronic beep (1000 Hz) with resonant ring."""
    n   = int(duration * SR)
    env = _adsr(n, 0.003, 0.05, 0.5, 0.45)
    samples = []
    for i in range(n):
        t = i / SR
        s  = 0.55 * _sine(1000.0, t)
        s += 0.25 * _sine(2000.0, t) * math.exp(-5.0 * t)
        s += 0.12 * _sine(3000.0, t) * math.exp(-9.0 * t)
        s += 0.08 * _sine(500.0, t)
        s *= env[i]
        samples.append(s)
    return samples


# ── Phase 5 SFX ───────────────────────────────────────────────────────────────

def _gen_paper_rustle(duration: float = 0.55) -> list[float]:
    """Foley paper-crinkle: slow noise burst with soft low-frequency body.
    Richer and slower than pageFlip — sounds like a hand pressing a page flat."""
    n   = int(duration * SR)
    env = _adsr(n, 0.03, 0.10, 0.45, 0.40)
    rng = random.Random(0xC0FF_EE00)
    samples = []
    for i in range(n):
        t = i / SR
        # White noise (broad paper hiss)
        noise = (rng.random() * 2.0 - 1.0) * 0.55
        # Low-frequency papery body softens the noise
        body  = 0.14 * _sine(280.0, t) + 0.08 * _sine(560.0, t) + 0.05 * _sine(140.0, t)
        # Brief mid-frequency crinkle transient at the crinkle onset
        crinkle = (rng.random() * 2.0 - 1.0) * 0.22 * math.exp(-14.0 * abs(t - 0.07))
        s = (noise + body + crinkle) * env[i]
        samples.append(s)
    return samples


def _gen_capstone_placed(duration: float = 2.1) -> list[float]:
    """Stone-on-stone seating impact: low thud followed by long resonant ring.
    Used for the E4 final capstone placement in Issue 12."""
    n = int(duration * SR)
    samples = []
    for i in range(n):
        t = i / SR
        # Impact thud — deep low-frequency sine transient (A1 55 Hz)
        thud  = 0.70 * _sine(55.0,  t) * math.exp(-18.0 * t)
        thud += 0.35 * _sine(110.0, t) * math.exp(-22.0 * t)
        # Resonant ring — stone surface vibration (A3 220 Hz + partials)
        ring  = 0.28 * _sine(220.0, t) * math.exp(-2.8 * t)
        ring += 0.14 * _sine(330.0, t) * math.exp(-3.6 * t)
        ring += 0.08 * _sine(440.0, t) * math.exp(-5.0 * t)
        ring += 0.05 * _sine(660.0, t) * math.exp(-6.8 * t)
        # Brief stone-surface scratch transient (very fast decay, high frequency)
        scratch = 0.18 * _sine(4400.0, t) * math.exp(-90.0 * t)
        # Master: instant attack, long natural exponential decay
        s = (thud + ring + scratch) * math.exp(-1.1 * t)
        samples.append(s)
    return samples


def _gen_finale_chord(duration: float = 8.0) -> list[float]:
    """Resolution Circuit chord — all five lens root frequencies held together.
    Roots: D2 73.4 Hz, F2 87.3 Hz, A2 110.0 Hz, C3 130.8 Hz, G3 196.0 Hz.
    1.5 s slow attack → 4 s sustain → 2.5 s slow fade.
    The capstone resolution that closes the Archive of Echoes narrative."""
    n       = int(duration * SR)
    attack  = int(1.5 * SR)
    release = int(2.5 * SR)
    # (root_hz, [amp per harmonic k=1..4])
    voices  = [
        (73.4,  [0.50, 0.22, 0.12, 0.07]),   # D2 — Political: heavy, imposing
        (87.3,  [0.48, 0.20, 0.11, 0.06]),   # F2 — Symbolic: warm depth
        (110.0, [0.52, 0.24, 0.13, 0.08]),   # A2 — Mythic: ancient resonance
        (130.8, [0.46, 0.18, 0.10, 0.06]),   # C3 — Technologic: clean digital
        (196.0, [0.50, 0.22, 0.12, 0.07]),   # G3 — Spiritual: ascending shimmer
    ]
    per_voice = 0.14      # per-root amplitude — sum peaks ~0.70
    samples   = []
    for i in range(n):
        t = i / SR
        if i < attack:
            env_val = (i / attack) ** 1.6
        elif i >= n - release:
            env_val = ((n - i) / release) ** 1.2
        else:
            env_val = 1.0
        s = 0.0
        for root, amps in voices:
            for k, amp in enumerate(amps, start=1):
                s += per_voice * amp * _sine(root * k, t)
        s *= env_val
        samples.append(s)
    return samples


# ── Placeholder motif ──────────────────────────────────────────────────────────

def _gen_motif_placeholder(duration: float = 1.8) -> list[float]:
    """Simple pentatonic rising phrase — to be replaced with real composition."""
    n    = int(duration * SR)
    # C major pentatonic: C4 E4 G4 A4 C5 (261, 329, 392, 440, 523 Hz)
    notes = [(0.00, 261.6), (0.30, 329.6), (0.60, 392.0), (0.90, 440.0), (1.20, 523.3)]
    gate  = 0.25
    samples = []
    for i in range(n):
        t = i / SR
        s = 0.0
        for note_t, freq in notes:
            dt = t - note_t
            if 0 <= dt < gate:
                env_n = int(gate * SR)
                idx   = int(dt * SR)
                local_env = min(idx / (0.01 * SR),
                                1.0,
                                (env_n - idx) / (0.06 * SR))
                s += 0.55 * _sine(freq, dt) * max(0.0, local_env)
                s += 0.20 * _sine(freq * 2, dt) * max(0.0, local_env) * 0.5
        samples.append(s)
    return samples


# ── Asset catalogue ─────────────────────────────────────────────────────────────
#
# Each entry: id, path, category, and a generator callable.

def _make_catalogue() -> list[dict]:
    return [
        # ── Drones (one per lens, loopable) ──────────────────────────────────────
        {
            "id": "drone_mythic",
            "path": "Assets/Audio/Drones/drone_mythic.wav",
            "category": "drones",
            "gen": lambda: _gen_drone(
                root_hz=110.0,   # A2 — warm ancient resonance
                lfo_hz=0.07, lfo_depth=0.30,
                harmonic_amps=(0.55, 0.24, 0.12, 0.07, 0.04),
                detune_cents=-3.0,
            ),
        },
        {
            "id": "drone_technologic",
            "path": "Assets/Audio/Drones/drone_technologic.wav",
            "category": "drones",
            "gen": lambda: _gen_drone(
                root_hz=130.8,   # C3 — clinical digital
                lfo_hz=0.12, lfo_depth=0.15,
                harmonic_amps=(0.50, 0.30, 0.15, 0.05, 0.03),
                detune_cents=+7.0,   # slight machine detuning
                vib_depth=0.0005,    # almost no vibrato (mechanical)
            ),
        },
        {
            "id": "drone_symbolic",
            "path": "Assets/Audio/Drones/drone_symbolic.wav",
            "category": "drones",
            "gen": lambda: _gen_drone(
                root_hz=87.3,    # F2 — introspective, inner depth
                lfo_hz=0.10, lfo_depth=0.35,
                harmonic_amps=(0.50, 0.20, 0.14, 0.10, 0.06),
                vib_depth=0.0025,
            ),
        },
        {
            "id": "drone_political",
            "path": "Assets/Audio/Drones/drone_political.wav",
            "category": "drones",
            "gen": lambda: _gen_drone(
                root_hz=73.4,    # D2 — heavy, imposing power
                lfo_hz=0.06, lfo_depth=0.20,
                harmonic_amps=(0.60, 0.20, 0.10, 0.06, 0.04),
                detune_cents=0.0,
                vib_depth=0.0008,
            ),
        },
        {
            "id": "drone_spiritual",
            "path": "Assets/Audio/Drones/drone_spiritual.wav",
            "category": "drones",
            "gen": lambda: _gen_drone(
                root_hz=196.0,   # G3 — ascending, ethereal shimmer
                lfo_hz=0.14, lfo_depth=0.38,
                harmonic_amps=(0.45, 0.28, 0.16, 0.08, 0.03),
                vib_depth=0.003,
                detune_cents=+2.0,
            ),
        },

        # ── Motifs ────────────────────────────────────────────────────────────────
        {
            "id": "motif_placeholder",
            "path": "Assets/Audio/Motifs/motif_placeholder.wav",
            "category": "motifs",
            "gen": _gen_motif_placeholder,
        },

        # ── SFX ───────────────────────────────────────────────────────────────────
        {
            "id": "sfx_panel_restored",
            "path": "Assets/Audio/SFX/sfx_panel_restored.wav",
            "category": "sfx",
            "gen": _gen_panel_restored,
        },
        {
            "id": "sfx_lens_unlock",
            "path": "Assets/Audio/SFX/sfx_lens_unlock.wav",
            "category": "sfx",
            "gen": _gen_lens_unlock,
        },
        {
            "id": "sfx_page_flip",
            "path": "Assets/Audio/SFX/sfx_page_flip.wav",
            "category": "sfx",
            "gen": _gen_page_flip,
        },
        {
            "id": "sfx_corruption_flash",
            "path": "Assets/Audio/SFX/sfx_corruption_flash.wav",
            "category": "sfx",
            "gen": _gen_corruption_flash,
        },
        {
            "id": "sfx_gutter_entity",
            "path": "Assets/Audio/SFX/sfx_gutter_entity.wav",
            "category": "sfx",
            "gen": _gen_gutter_entity,
        },
        {
            "id": "sfx_knowledge_key_collected",
            "path": "Assets/Audio/SFX/sfx_knowledge_key_collected.wav",
            "category": "sfx",
            "gen": _gen_knowledge_key_collected,
        },
        {
            "id": "sfx_t5_unlock",
            "path": "Assets/Audio/SFX/sfx_t5_unlock.wav",
            "category": "sfx",
            "gen": _gen_t5_unlock,
        },
        {
            "id": "sfx_djed_bar_activated",
            "path": "Assets/Audio/SFX/sfx_djed_bar_activated.wav",
            "category": "sfx",
            "gen": _gen_djed_bar_activated,
        },
        {
            "id": "sfx_circuit_close",
            "path": "Assets/Audio/SFX/sfx_circuit_close.wav",
            "category": "sfx",
            "gen": _gen_circuit_close,
        },
        {
            "id": "sfx_paper_rustle",
            "path": "Assets/Audio/SFX/sfx_paper_rustle.wav",
            "category": "sfx",
            "gen": _gen_paper_rustle,
        },
        {
            "id": "sfx_capstone_placed",
            "path": "Assets/Audio/SFX/sfx_capstone_placed.wav",
            "category": "sfx",
            "gen": _gen_capstone_placed,
        },
        {
            "id": "sfx_finale_chord",
            "path": "Assets/Audio/SFX/sfx_finale_chord.wav",
            "category": "sfx",
            "gen": _gen_finale_chord,
        },
    ]


# ── Main ────────────────────────────────────────────────────────────────────────

def main() -> None:
    parser = argparse.ArgumentParser(
        description="Procedurally synthesise Archive of Echoes audio assets"
    )
    parser.add_argument("--dry-run",  action="store_true")
    parser.add_argument("--force",    action="store_true")
    parser.add_argument("--list",     action="store_true")
    parser.add_argument("--category", choices=["drones", "motifs", "sfx"])
    parser.add_argument("--id",       metavar="ASSET_ID")
    args = parser.parse_args()

    catalogue = _make_catalogue()

    if args.list:
        for a in catalogue:
            print(f"{a['id']:40s}  {a['category']:8s}  {a['path']}")
        return

    targets = catalogue[:]
    if args.category:
        targets = [a for a in targets if a["category"] == args.category]
    if args.id:
        targets = [a for a in targets if a["id"] == args.id]
        if not targets:
            import sys
            sys.exit(f"ERROR: no asset '{args.id}'")

    if not args.force:
        pending = [a for a in targets if not (PROJECT_ROOT / a["path"]).exists()]
        skipped = len(targets) - len(pending)
        if skipped:
            print(f"[skip] {skipped} existing (use --force to overwrite)")
        targets = pending

    if not targets:
        print("[done] Nothing to generate.")
        return

    print(f"[plan] {len(targets)} assets to synthesise (no API calls — pure Python stdlib)")

    if args.dry_run:
        for a in targets:
            print(f"  [dry]  {a['id']:40s} → {a['path']}")
        return

    generated, failed = [], []
    manifest: dict[str, str] = {}

    for i, asset in enumerate(targets, 1):
        out_path = PROJECT_ROOT / asset["path"]
        print(f"[{i:>2}/{len(targets)}] {asset['id']}", end=" … ", flush=True)
        try:
            samples = asset["gen"]()
            _write_wav(out_path, samples)
            _write_unity_meta(out_path)
            dur_ms = len(samples) * 1000 // SR
            print(f"✓  {dur_ms}ms  {len(samples) // 1024}KB")
            generated.append(asset)
            manifest[asset["id"]] = asset["path"].replace("\\", "/")
        except Exception as exc:
            print(f"✗  FAILED — {exc}")
            failed.append((asset, str(exc)))

    # Merge into manifest
    existing: dict[str, str] = {}
    if MANIFEST.exists():
        try:
            existing = json.loads(MANIFEST.read_text(encoding="utf-8"))
        except Exception:
            pass
    existing.update(manifest)
    MANIFEST.parent.mkdir(parents=True, exist_ok=True)
    MANIFEST.write_text(json.dumps(existing, indent=2), encoding="utf-8")

    print(f"\n{'─'*60}")
    print(f"  Generated : {len(generated)}")
    print(f"  Failed    : {len(failed)}")
    print(f"  Manifest  : {MANIFEST.relative_to(PROJECT_ROOT)}")
    if failed:
        for a, err in failed:
            print(f"    ✗ {a['id']}: {err}")
    print("\n  Next step in Unity: Tools → Archive of Echoes → 6 – Import Audio Assets")


if __name__ == "__main__":
    main()
