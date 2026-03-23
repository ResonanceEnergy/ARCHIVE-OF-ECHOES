# Archive of Echoes — Changelog

## v1.0.0 — Initial Release

### Features
- Twelve-issue graphic novel with five epistemic lenses (Mythic, Technologic, Symbolic, Political, Spiritual)
- Interactive panel puzzles: B-series Stabilise and A-series Reorder
- Knowledge Key collection system (24 keys across Issues 00–11)
- Procedurally synthesised ambient drone per lens (additive synthesis + LFO)
- SFX library: panel restore, lens unlock, page flip, gutter entity,
  T5 unlock, Djed bar activation, circuit close
- Phase 5 foley: paper rustle and capstone-placed one-shot SFX
- Issue 12 Resolution Circuit: all-five-frequency capstone held chord (8 s)
- Art polish overlays: paper grain, vignette, ink scan-lines, warm glow
- Accessibility: font size multiplier (1–2×), contrast modes
  (Normal / High Contrast / Monochrome), haptics toggle, reduce-motion flag
- App Store metadata and screenshot specifications

### Technical
- Unity 6000.3.6f1, iOS target
- Procedural audio synthesis pipeline (Python stdlib only — no external libs)
- ScriptableObject-driven data model
- Pipeline automation: art → audio → Unity batchmode in one command
- Fully offline — no network required after initial download
