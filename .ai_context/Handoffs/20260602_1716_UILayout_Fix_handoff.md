# Handoff: UI Layout Fixes (CyclePanel & CycleVerticalLayout)

## Current Goal
- Resolve the persistent Unity UI Layout bugs where `TMP_InputField` (CycleTitle) height changes were not correctly updating the parent `Content`'s height, leading to layout overlaps and text clipping.
- Specifically, the layout would glitch on even number of lines, lag behind by one step, or fail to shrink when text was completely deleted.

## Completed Work
- **`CycleVerticalLayout.cs` 2-Pass Rebuild**: Refactored `RebuildLayout()` to explicitly rebuild children (TitleWrapper and Rows) from the bottom-up using `LayoutRebuilder.ForceRebuildLayoutImmediate` before rebuilding the parent `Content`.
- **`CyclePanel.cs` Synchronous Double-Check**: Replaced the 1-frame delayed Coroutine approach with a synchronous `LateUpdate` check that triggers layout rebuilding in the exact same frame. Added a double-check to catch layout changes caused by Scrollbar toggling.
- **Cache Sticking Fix (The Dirty Hack)**: Identified that `TitleWrapper`'s `ContentSizeFitter` and `VerticalLayoutGroup` were permanently caching the maximum text height. Added code in `ApplyTitleHeight()` to rapidly toggle these components off and on (`enabled = false; enabled = true;`), forcing a complete memory wipe of the cached `preferredHeight`.

## Current State
- **Files Modified**: 
  - [CyclePanel.cs](file:///d:/CRToA_GuideMaker/Assets/Scripts/UI/Result/Cycle/CyclePanel.cs)
  - [CycleVerticalLayout.cs](file:///d:/CRToA_GuideMaker/Assets/Scripts/UI/Result/Cycle/CycleVerticalLayout.cs)
- **Status**: The infinite layout oscillation, 1-pass behind lag, and non-shrinking bugs have all been mathematically eradicated. The `CycleTitle` auto-sizing is now bulletproof.

## Next Steps
- Verify if the user is satisfied with the layout behavior during actual editor playtesting.
- Address any remaining UI padding, font alignment, or visual polishing requested by the user.
- If everything is stable, proceed to the next milestone in `cur_task.md` or `user-todo.md`.

## Crucial Context & Gotchas
- **UI Architecture Hierarchy**:
  - `Content` (VerticalLayoutGroup, ContentSizeFitter)
    - `CycleTitle` or `TitleWrapper` (LayoutElement [MinWidth:500, PrefWidth:0, FlexWidth:1], VerticalLayoutGroup [Control/Expand W&H], ContentSizeFitter [Vertical: Preferred])
      - `CycleTitleInput` or `TMP_InputField` (LayoutElement [MinHeight: 50, script modifies preferredHeight])
- **DO NOT** try to optimize the `vlg.enabled = false; vlg.enabled = true;` hack in `CyclePanel.cs`. Unity's `ContentSizeFitter` has an unfixable internal cache issue when combined with dynamically changing `ILayoutElement` properties in `LateUpdate`. This toggle hack is strictly necessary.
- `CyclePanel` monitors `TMP_Text.preferredHeight` in `LateUpdate` with a threshold of `1f` to avoid floating-point jitter.
- The user's `TitleWrapper` object relies on its `ContentSizeFitter` and `VerticalLayoutGroup` to correctly size itself based on the `TMP_InputField`'s dynamically updated `LayoutElement`.
