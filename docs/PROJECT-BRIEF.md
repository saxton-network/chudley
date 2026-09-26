# Project Brief

## Purpose

Build Chudley as a small, sprite-driven desktop pet.

The repository starts with art and behavioral requirements first. Implementation architecture is deliberately not preselected so the coding agent can propose an appropriate stack rather than inheriting one by accident.

## Initial MVP

The first working release should eventually:

- launch locally;
- display Chudley cleanly on the desktop;
- play a stable idle animation;
- support multiple animation states;
- react to basic user interaction;
- load production art from an explicit asset structure;
- avoid broken frame alignment or scaling;
- expose a clear path for adding future behaviors.

## Engineering priorities

Favor:

- readable structure;
- deterministic animation/state handling;
- low idle resource use;
- local operation;
- straightforward packaging;
- configuration that is inspectable rather than magical;
- tests around behavior that would otherwise regress silently.

Avoid:

- network dependencies unless they become an explicit feature;
- hard-coding animation data into unrelated UI logic;
- architecture designed for hypothetical scale the project does not need;
- silently modifying canonical art assets.

## Current phase

Windows MVP candidate implemented with the 24 supplied keyframes. See the root README for build, controls, and current art limitations. Human Windows runtime smoke testing and artist continuity review remain release gates.
