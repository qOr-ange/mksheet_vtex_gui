-----

# VTF\_RSRC\_SHEET - Reference

  - **Resource ID**: 0x10
  - **Version**: 1
  - **Byte Order**: Little-endian
  - **Purpose**: Describes animated "sheet" playback (sequences, frames, timing, UVs)
  - **Applies To**: VTF files built via `mksheet.exe` (SpriteCard textures)

-----

## 1\. High-Level Model

A **Sheet** contains N sequences. Each sequence contains M frames. Each frame has:

  - a display duration (seconds)
  - up to 4 UV rects (`uMin`, `vMin`, `uMax`, `vMax`) addressing subregions of the sheet

**Notes:**

  - C++ writer emits 4 UV sets per frame. Simpler sheets may leave 3 sets empty.
  - Sequences may **clamp** (stop on the last frame) or **loop**.
  - Packing modes in the build step can split RGB vs. A usage across sequences.

-----

## 2\. Binary Layout (V1)

All integers are 32-bit unless otherwise stated. Floats are 32-bit IEEE-754.

| Offset | Type | Field |
| :--- | :--- | :--- |
| `0x00` | `int` | Version (must be 1) |
| `0x04` | `int` | NumSequences |
| `0x08` | `...` | Sequence[`NumSequences`] |

**Sequence**

| Offset (rel.) | Type | Field |
| :--- | :--- | :--- |
| `+0x00` | `int` | SequenceId |
| `+0x04` | `int` | Clamp (`1`=clamp, `0`=loop) |
| `+0x08` | `int` | NumFrames |
| `+0x0C` | `float` | TotalDuration (sum of frames) |
| `+0x10` | `...` | Frame[`NumFrames`] |

**Frame**

| Offset (rel.) | Type | Field |
| :--- | :--- | :--- |
| `+0x00` | `float` | Duration (seconds) |
| `+0x04` | `float[4]` | UV0: `uMin`,`vMin`,`uMax`,`vMax` |
| `+0x14` | `float[4]` | UV1: `uMin`,`vMin`,`uMax`,`vMax` |
| `+0x24` | `float[4]` | UV2: `uMin`,`vMin`,`uMax`,`vMax` |
| `+0x34` | `float[4]` | UV3: `uMin`,`vMin`,`uMax`,`vMax` |

**Sizes (per V1):**

  - Sequence header: 16 bytes before frames
  - Frame: 4 + (4 UV sets \* 16) = 68 bytes
  - UV set: 16 bytes (4 floats)

-----

## 3\. Semantics & Mapping

**`SequenceId`**

  - Arbitrary integer key for engine lookups.

**`Clamp`**

  - `1` = play through and stop on the last frame.
  - `0` = loop from the last back to the first.

**`NumFrames` / `TotalDuration`**

  - `TotalDuration` should equal the sum of all `Frame.Duration`. Treat mismatch as `source-of-truth` = per-frame durations.

**UV Sets (`UV0`..`UV3`)**

  - `UV0` is commonly used for basic sheets.
  - Additional sets accommodate multi-texturing or mode-dependent sampling.
  - Empty sets may be zeros when not used by a given sequence/frame.

**Pack Modes** (build-time; informs playback intent)

  - **`FLAT`** (default): Each frame occupies the full RGBA region (`SQM_RGBA`).
  - **`RGB+A`**: Sequences can be split so some sample only RGB (`SQM_RGB`) and others sample only A (`SQM_ALPHA`). Frame sizes and counts can differ between RGB and A inputs at build time; the sheet stores only the post-build UVs/timing.

**Sequence “Mode”** (engine-facing concept; not serialized in V1)

  - `SQM_RGBA`: sequence samples full RGBA.
  - `SQM_RGB` : sequence samples only RGB.
  - `SQM_ALPHA`: sequence samples only A.
    *(Engines infer this from authoring/build context; V1 does not persist it.)*

-----

## 4\. Authoring vs. Sheet Data (What's Lost at Build Time)

`mksheet.exe` authoring (`.mks`) supports macros and multi-input wiring:

  - **Example**: `frame base.tga add.tga 1` (two images per frame for multi-texture)
  - **Example**: `frame f0.tga{g=a},f1.tga{a=a} 1` (channel remap macro)
  - These are compile-time transforms. The VTF texture + `VTF_RSRC_SHEET` contain only the baked result (UVs, durations). Original multi-input relationships and channel ops are not recoverable from the sheet.

**`Packmode`**

  - `packmode rgb+a` authoring can yield sequences that *intend* RGB-only vs. Alpha-only sampling. V1 sheet does not explicitly tag sequences with that mode; engines typically know from material/shader usage and authoring data.

-----

## 5\. Validation & Edge Cases

  - **Version** must be `1`. Reject unknown versions or gate behind feature flags.
  - **`NumSequences`** `>= 0`. Each sequence must have **`NumFrames`** `>= 1`.
  - **`TotalDuration`** `≈` `sum(Frame.Duration)`. If off, prefer per-frame values.
  - UV bounds are expected within `[0..1]`; engines may tolerate a slight epsilon.
  - Unused UV sets may be all zeros. Do not fail parsing on that basis.
  - **`Clamp`** must be `0` or `1`; treat any nonzero as `1` for robustness.

-----

## 6\. Lookup & Playback

  - Playback time `t` maps into the selected sequence:
      - If `Clamp=1`: clamp `t` to `[0, TotalDuration]`
      - If `Clamp=0`: wrap `t` modulo `TotalDuration`
  - Accumulate frame durations to choose the active frame; sample `UV0` unless a shader or material expects additional sets (e.g., dual sampling).

-----

## 7\. Relation to Authoring (`.MKS`)

Minimal conceptual mapping:

| `.mks` | `VTF_RSRC_SHEET` |
| :--- | :--- |
| `sequence N` | `SequenceId = N` |
| `loop / (no loop)` | `Clamp = 0 / Clamp = 1` |
| `frame ... RATE` | `Frame.Duration = RATE * base_time` |
| `packmode rgb+a` | Authoring hint; sheet remains UV/timing only |

**Reference**: Animated Particles (Valve Developer Community).
*Authoring features like per-channel remaps and multi-input frames are compile-time only.*

-----

## 8\. Related Engine Types (Reference)

**`PackingMode_t`** (build-time; informs intent)

  - `PCKM_INVALID` = 0
  - `PCKM_FLAT` = 1
  - `PCKM_RGB_A` = 2

**`Sequence::SeqMode_t`** (engine concept; not stored in V1)

  - `SQM_RGBA` = 0
  - `SQM_RGB` = 1
  - `SQM_ALPHA` = 2

-----

## 9\. Example (Human-Readable Summary)

```
Sheet
  Version: 1
  NumSequences: 2

  SequenceId: 0
    Clamp: 0 (loop)
    NumFrames: 2
    TotalDuration: 0.1000
      Frame[0]: Duration=0.0500, UV0=(0.00,0.00,0.50,0.50), UV1..UV3=empty
      Frame[1]: Duration=0.0500, UV0=(0.50,0.00,1.00,0.50), UV1..UV3=empty

  SequenceId: 1
    Clamp: 1 (clamp)
    NumFrames: 1
    TotalDuration: 0.1000
      Frame[0]: Duration=0.1000, UV0=(0.00,0.50,0.50,1.00), UV1..UV3=empty
```
