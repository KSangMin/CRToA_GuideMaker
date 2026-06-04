---
name: handoff
description: >
  Generates a handoff artifact to transfer agent context from the current session to a new session.
  Triggered when the user types "/handoff", "handoff", or asks to save context for a new session.
---

# Handoff Skill

When invoked, your goal is to seamlessly transfer the current context to a future session or another agent. You will do this by creating a written handoff artifact.

## Steps to Execute
1. Analyze the current conversation, active tasks, and recent modifications.
2. Identify a brief 1-2 word topic for the current session (e.g., "UI_Refactoring", "BugFix").
3. Create the handoff document inside the `.ai_context/Handoffs/` directory (create the directory if it doesn't exist).
4. Name the file using the format `YYYYMMDD_HHMM_Topic_handoff.md` based on the current local time (e.g., `20260529_1930_UI_Refactoring_handoff.md`).
5. The artifact MUST contain the following sections:
   - **Current Goal**: What is the overarching objective?
   - **Completed Work**: What was accomplished in this session?
   - **Current State**: Which files were recently modified? What is the status of the codebase?
   - **Next Steps**: What exactly should the next agent or session do?
   - **Crucial Context & Gotchas**: Any specific errors, architectural decisions, or user preferences the next agent needs to know.
6. Confirm to the user that the handoff artifact has been created and that they can safely start a new session.

## Format of `handoff.md`
- Use a clean, structured markdown format.
- Use bullet points for readability.
- Keep it dense with technical context but free of conversational fluff.
- Link to relevant files where appropriate.
