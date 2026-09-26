# Art Pipeline

## Authority order

1. **Canonical large character reference**  
   Source of truth for identity: face, proportions, clothing, colors, accessories, scooter/equipment, silhouette, and distinctive details.

2. **Canonical master sprite sheet**  
   Once approved, source of truth for pixel treatment: sprite scale, palette, outlines, shading, frame geometry, baseline, and animation construction.

3. **Production animation sheets**  
   Derived from both canonical references. They may extend motion and expression, but must not redesign the character.

## Planned production order

1. master sprite sheet;
2. idle and blink;
3. locomotion / scooter movement;
4. interaction reactions;
5. expression states;
6. special behaviors;
7. transitions between states.

## Consistency rules

Production sprites should preserve:

- head and facial structure;
- body proportions;
- clothing and accessories;
- scooter geometry;
- palette and outline treatment;
- pixel density;
- character scale;
- frame dimensions within an animation family;
- baseline alignment;
- physical left/right details rather than blindly mirroring asymmetric features.

## File organization

`assets/reference/` is reserved for approved reference material.

`assets/sprites/` is reserved for approved production sprite sheets and extracted frames.

Experimental generations should not be promoted into the canonical asset folders until reviewed. The supplied production candidate is under `assets/sprites/candidate/`; its generated source images remain separate under `assets/source/generated/`.

## Current status

The original character reference and a 24-frame production candidate are imported. `manifest.json` records 20 `approved_master` frames and four `repaired_bottom_strip` frames. They remain keyframes; face, hands, scooter, props, and inbetweens need later artist review before treating this as a finished animation library.
