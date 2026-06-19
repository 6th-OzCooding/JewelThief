# JewelThief Codex Collaboration Rules

> This file is a restored Codex-side copy based on the previous
> `JEWELTHIEF_COLLABORATION_RULES.md` summary kept in Codex memory.
> If the team convention document changes later, that document overrides this file.

## 1. Priority Order

For JewelThief work, apply rules in this order:

1. Team convention document, if available.
2. `AGENTS.md` instructions provided for this project.
3. This Codex collaboration rules file.
4. The user's general Unity/C# preferences.
5. The current code structure in the repository.

If these conflict, the higher item wins.

## 2. Communication Style

- Explain Unity/C# concepts in Korean, in a beginner-friendly way.
- Connect explanations to the real JewelThief project structure instead of giving generic Unity advice.
- Use concrete file and class names when discussing implementation.
- Explain responsibility, reference direction, and runtime flow before suggesting code.
- Avoid compressed expert-only terms unless they are also explained plainly.

## 3. Approval Before Code Changes

Before editing code, first explain the implementation direction and wait for explicit approval when the work touches:

- Architecture
- Manager responsibility split
- Input flow
- Pause or movement-lock flow
- UI state management
- Shared/common systems
- Data loading or resource routing used by multiple systems

The explanation should include:

- Which class or manager should own the responsibility.
- Which object should reference or call which other object.
- Why that shape fits JewelThief's current structure.
- How existing systems can be reused.
- What files are expected to change.

## 4. Active Project Baseline

JewelThief's active script baseline is under:

- `Assets/02_Scripts/Common`
- `Assets/02_Scripts/Data`
- `Assets/02_Scripts/Utils`
- Other feature folders under `Assets/02_Scripts`

Do not assume older `BaseCodes` patterns are the active implementation for this repo.
They may be useful as background, but JewelThief's current code and team convention take priority.

Important existing baseline scripts include:

- `GameManager`
- `ResourceManager`
- `PoolManager`
- `SoundManager`
- `DataTable`
- `SingletonBehaviour`
- `Data`
- `Utils`
- `UIManager`
- `UIManagerExtension`

## 5. Current UI Ownership Rule

The current UI flow is manager-owned:

- `UIManager` creates, caches, opens, and closes UI instances.
- `UIManagerExtension` provides convenient feature-specific open/close helpers.
- UI prefabs are expected to follow the `Resources/Prefabs/UI/{UIRootType}/{UIType}` path rule.
- Open state should stay synchronized with `UIManager`; do not let UI objects secretly bypass manager state.

For UI-related work, first decide whether the UI belongs to:

- `BackgroundUI`
- `MainUI`
- `ContentUI`
- `PopupUI`
- `VeryFrontUI`

Then route creation/open/close through the existing `UIManager` structure unless there is a clearly explained reason not to.

## 6. Coding Style Notes

Follow the team's style rules when they are known:

- Public classes, public methods, and public properties should have XML documentation comments when they are part of a reusable API.
- Constants use `SCREAMING_SNAKE_CASE`.
- Events use `On`-prefixed names.
- Coroutines use `Co` or `Routine` naming.
- Prefer explicit types over `var` for local variables, especially when the type is apparent.
- Use braces consistently.
- Keep classes and methods small enough that their responsibility is clear.
- Match the naming style already used near the code being changed.

## 7. Change Boundary

- Do not touch unrelated files.
- Do not edit scenes, prefabs, `.meta` files, or project settings unless the user asked for it or the change is clearly necessary.
- Keep direct edits narrow, usually `.cs` files first.
- For local or small changes, prefer giving copy-paste-ready code in chat if that is enough.
- Do not introduce a large general-purpose framework for a small requested feature.

## 8. Data and Resource Routing

When adding systems that use data or assets:

- Check the existing data-loading route first.
- Prefer existing manager or utility paths over direct ad hoc loading.
- For Addressables-backed assets, use address strings or the existing resource abstraction rather than raw local file paths.
- Paid or gitignored local assets should not be hardcoded as teammate-specific filesystem paths.

## 9. Git Workflow

The remembered JewelThief workflow is:

- Do not push directly to `main`.
- Treat `develop` as the integration branch.
- Work on a personal or feature branch.
- Explain branch state in plain language for the user.
- Use team-style commit prefixes such as:
  - `feat`
  - `fix`
  - `docs`
  - `refactor`
  - `chore`

Before risky Git work, check the current state with:

```powershell
git status --short --branch
```

Do not discard user changes unless the user explicitly asks.

## 10. Future Chat Startup Prompt

For future JewelThief chats, use this kind of startup instruction:

```text
C:\NRUnityprojects\JewelThief 프로젝트 작업이야.
먼저 .codex/JEWELTHIEF_COLLABORATION_RULES.md를 읽고,
그 규칙을 최우선으로 적용해 줘.
코드를 수정하기 전에는 구현 방향과 책임 분리를 먼저 설명하고,
내 승인을 받은 뒤에 실제 수정으로 넘어가 줘.
```

