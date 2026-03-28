# LogLens — Claude Code Project Setup
#
# PASTE THIS ENTIRE FILE INTO CLAUDE CODE
# It will create the complete .claude folder structure
# and ensure none of it gets committed to git.
#
# Run from your LogLens repo root folder.

## WHAT THIS DOES

Creates:
  CLAUDE.md                          (repo root — gitignored)
  .claude/settings.json              (gitignored)
  .claude/settings.local.json        (gitignored)
  .claude/rules/dotnet-rules.md      (gitignored)
  .claude/rules/nextjs-rules.md      (gitignored)
  .claude/rules/testing-rules.md     (gitignored)
  .claude/rules/git-rules.md         (gitignored)
  .claude/rules/security-rules.md    (gitignored)
  .claude/skills/phase-runner/SKILL.md       (gitignored)
  .claude/skills/pr-creator/SKILL.md         (gitignored)
  .claude/skills/migration-runner/SKILL.md   (gitignored)
  .claude/skills/health-checker/SKILL.md     (gitignored)
  .claude/agents/code-reviewer.md    (gitignored)
  .claude/agents/test-writer.md      (gitignored)
  .claude/agents/security-auditor.md (gitignored)

Then updates .gitignore to exclude all of the above.

## EXECUTE THESE STEPS IN ORDER

### STEP 1 — Update .gitignore

Add these entries to .gitignore (append to end of existing file):

```
# Claude Code configuration (local only — never commit)
CLAUDE.md
.claude/
```

### STEP 2 — Create CLAUDE.md in repo root

Create file: CLAUDE.md

```markdown
# LogLens.io

AI-powered log analysis — paste logs, get errors grouped
and AI root cause suggestions in seconds.

## Stack
- Backend: .NET 9 Minimal API → LogLens.Api/
- Core engine: LogLens.Core/ (.NET class library)
- Frontend: Next.js 15 App Router → loglens-web/
- Database: PostgreSQL 16 + EF Core 9 (code first)
- Auth: JWT 15min access + 7day refresh tokens (BCrypt passwords)
- AI: Claude API claude-sonnet-4-20250514 (Pro tier only)
- Payments: Stripe subscriptions

## Phase Status
Phase 1 — LogLens.Core parser engine          COMPLETE
Phase 2 — .NET 9 Minimal API                 COMPLETE
Phase 3 — Next.js 15 frontend                NEXT
Phase 4 — AI integration                     PENDING
Phase 5 — Stripe billing frontend            PENDING
Phase 6 — Deploy + launch                    PENDING

All phase prompts are in: LOGLENS_PHASES.md (local only)
Full architecture reference: CONTEXT.md (local only)

## Essential Commands
```bash
# Backend
dotnet build                                    # build solution
dotnet test LogLens.Core.Tests                  # unit tests
dotnet test LogLens.Api.Tests                   # integration tests
dotnet run --project LogLens.Api                # API on :5000
docker-compose up -d db                         # start postgres
dotnet ef migrations add NAME --project LogLens.Api
dotnet ef database update --project LogLens.Api

# Frontend
cd loglens-web
npm run dev          # dev server on :3000
npm run build        # production build
npm run type-check   # TypeScript check
npm run lint         # ESLint
npm test             # Vitest
```

## Project Structure
```
LogLens/
├── LogLens.Core/          parser engine (5 formats, error grouper)
├── LogLens.Core.Tests/    xUnit tests
├── LogLens.Api/           .NET 9 Minimal API
├── LogLens.Api.Tests/     integration tests
├── loglens-web/           Next.js 15 frontend
└── docker-compose.yml     local PostgreSQL
```

## Branching (GitFlow)
main     → production (protected, no direct push)
develop  → staging (protected, no direct push)
feature/phase-X-name → PR to develop
release/vX.X.X       → PR to main
hotfix/description   → PR to main + develop

## Commit Convention
feat: fix: test: ci: security: refactor: docs: chore:
Always lowercase. No period at end.

## API Endpoints
POST /api/analyze          free tier text analysis
POST /api/analyze/upload   free tier file upload (10MB max)
GET  /api/sessions         auth required
GET  /api/sessions/{id}    auth required
POST /api/auth/register    { email, password }
POST /api/auth/login       { email, password }
POST /api/auth/refresh     { refreshToken }
POST /api/auth/logout      { refreshToken }
GET  /api/auth/me          auth required
POST /api/billing/checkout auth required
POST /api/billing/webhook  Stripe events
GET  /health               Railway health check
GET  /metrics              Prometheus scraping

## Free Tier Limits
3 sessions/day per IP/user
500 lines max per analysis
No AI analysis (Pro only)

## Critical Rules
- NEVER commit secrets or .env files
- NEVER push directly to main or develop
- NEVER use synchronous DB calls (always async)
- NEVER hardcode API keys anywhere
- Always use gh pr create for PRs
- appsettings.Development.json is gitignored (local only)
- CONTEXT.md and LOGLENS_PHASES.md are gitignored (local only)
- CLAUDE.md and .claude/ are gitignored (local only)

---

## Session Start Protocol
At the start of every session automatically:
1. Run /mem-search LogLens — recall stored memory context
2. Read CONTEXT.md — current architecture and phase status
3. Read tasks/lessons.md — past corrections and rules
4. Report current phase and what will be worked on

## Global Skills Available (invoke these by name)
Memory:
  /mem-search LogLens    — recall stored context (use at session start)
  /make-plan             — create plan and store in memory
  /smart-explore         — intelligently explore the codebase
  /timeline-report       — show timeline of completed work
  /do                    — execute task with full memory context

Obsidian (sync to vault):
  /obsidian-cli          — read/write Obsidian vault directly
  /obsidian-markdown     — create formatted Obsidian notes
  /obsidian-bases        — work with Obsidian databases

UI/UX (auto-fires on frontend tasks):
  /ui-ux-pro-max         — force-invoke for UI review

GSD project management:
  /gsd:next              — what to work on next
  /gsd:progress          — current progress summary
  /gsd:execute-phase     — execute phase autonomously
  /gsd:ship              — prepare to ship
  /gsd:health            — project health check
  /gsd:debug             — debug a problem
  /gsd:review            — review current work
  /gsd:autonomous        — fully autonomous execution
  /gsd:fast              — fast mode
  /gsd:verify-work       — verify work is complete
  /gsd:session-report    — end of session summary

## How I Work (Workflow Principles)

### Planning
- Enter plan mode for ANY non-trivial task (3+ steps or architectural decisions)
- Write plan to tasks/todo.md with checkable items before writing any code
- Check in with user before starting implementation on complex tasks
- If something goes sideways mid-task: STOP, re-plan, then continue
- Use plan mode for verification steps, not just building

### Task Management
- Mark todo.md items complete as I go
- Give high-level summary at each major step
- Add review section to tasks/todo.md when done
- After ANY user correction: update tasks/lessons.md with the pattern
- Review tasks/lessons.md at the start of each session

### Self-Improvement
- After any correction: write a rule in tasks/lessons.md to prevent recurrence
- Ruthlessly iterate on lessons until mistake rate drops
- Never repeat the same class of mistake twice

### Verification
- NEVER mark a task complete without proving it works
- Run tests, check logs, demonstrate correctness before saying done
- Ask myself: "Would a staff engineer approve this?"
- Diff changes against main when relevant

### Code Quality
- Simplicity first: make every change as simple as possible
- Minimal impact: only touch what is necessary — no side effects
- Find root causes — no temporary hacks — senior developer standards
- For non-trivial changes: pause and ask "is there a more elegant way?"
- If a fix feels hacky: implement the elegant solution instead
- Skip over-engineering for simple obvious fixes

### Autonomy
- When given a bug: just fix it — no hand-holding needed
- Point at logs, errors, failing tests — then resolve them
- Fix failing CI tests without being asked how
- Zero context switching required from the user

### Subagents
- Use subagents to keep main context window clean
- Offload research, exploration, and parallel analysis to subagents
- One focused task per subagent
- For complex problems: throw more compute at it via subagents
```

### STEP 3 — Create .claude/settings.json

Create file: .claude/settings.json

```json
{
  "model": "claude-sonnet-4-5",
  "permissions": {
    "allow": [
      "Bash(dotnet *)",
      "Bash(npm *)",
      "Bash(npx *)",
      "Bash(git *)",
      "Bash(gh *)",
      "Bash(docker-compose *)",
      "Bash(docker *)",
      "Bash(infisical *)",
      "Bash(stripe *)",
      "Bash(curl http://localhost*)",
      "Bash(curl https://api.loglens.io*)",
      "Read(**)",
      "Write(**)"
    ],
    "deny": [
      "Bash(rm -rf /)",
      "Bash(curl * | bash)",
      "Bash(wget * -O- | bash)",
      "Bash(git push origin main)",
      "Bash(git push --force *)"
    ]
  }
}
```

### STEP 4 — Create .claude/settings.local.json

Create file: .claude/settings.local.json

```json
{
  "note": "Add your personal env overrides here. This file is gitignored.",
  "env": {}
}
```

### STEP 5 — Create rules files

Create file: .claude/rules/dotnet-rules.md

```markdown
# .NET 9 Coding Rules for LogLens

## Project conventions
- Minimal API pattern only — no controllers ever
- Primary constructors: MyService(ILogger<MyService> logger)
- C# 12 collection expressions: [item1, item2] not new List<>()
- Records for all request/response DTOs
- Namespace always matches folder path

## Async rules
- All DB calls must be async: FirstOrDefaultAsync, SaveChangesAsync
- Always accept CancellationToken ct = default parameter
- Use Task.WhenAll for parallel independent operations
- Never .Result or .Wait() — always await

## Error handling
- All endpoints return typed Results: Results.Ok(), Results.BadRequest()
- Error responses always: Results.BadRequest(new { error = "message" })
- Never throw unhandled exceptions from endpoints
- Log errors with logger.LogError(ex, "message {Param}", param)

## EF Core
- Always use .AsNoTracking() for read-only queries
- Never load full entities when projections suffice — use .Select()
- Avoid N+1: use .Include() or split queries
- Migrations go in LogLens.Api/Data/Migrations/

## Code style
- var only when type is obvious from right-hand side
- Prefer expression-bodied members for simple one-liners
- Guard clauses at top of methods — early returns
- XML docs on all public API surface
```

Create file: .claude/rules/nextjs-rules.md

```markdown
# Next.js 15 Coding Rules for LogLens

## Architecture
- App Router only — never use pages/ directory
- Route groups: (marketing) for SSG, (app) for interactive, (auth) for auth pages
- Server components by default — only add 'use client' when needed
- Data fetching in server components — never useEffect for data loading

## TypeScript
- Strict mode always — no any types ever
- Explicit return types on all functions
- Interfaces for objects, type aliases for unions
- Never use non-null assertion (!) — handle nulls properly

## API calls
- ALL API calls go through lib/api.ts — never inline fetch or axios
- Always handle loading state and error state
- Use TanStack Query for data that needs caching or refetching

## Styling
- Tailwind CSS only — no CSS modules, no styled-components
- Dark theme: bg-gray-950 body, bg-gray-900 panels, bg-gray-800 hover
- Accent: emerald-400 (text/borders), emerald-600 (buttons)
- Error: red-400, Warning: yellow-400, Info: blue-400
- Always handle hover, focus, and disabled states

## Components
- Named exports for all components
- Default export only for page.tsx files
- Props interfaces defined above the component
- Components under 200 lines — split if larger

## State
- Zustand for global state (auth store)
- useState for local UI state
- Never prop-drill more than 2 levels — use context or store
```

Create file: .claude/rules/testing-rules.md

```markdown
# Testing Rules for LogLens

## .NET xUnit Tests (LogLens.Core.Tests, LogLens.Api.Tests)

### Naming
- Test class: {ClassUnderTest}Tests
- Test method: {Method}_{Scenario}_{ExpectedResult}
- Example: Parse_ValidSerilogFormat_ReturnsCorrectLevel

### Structure
- Always use Arrange / Act / Assert pattern with comments
- One assertion concept per test (multiple asserts for same thing ok)
- Use [Theory] + [InlineData] for parameterized tests
- Descriptive failure messages on Assert calls

### Coverage targets
- LogLens.Core: 85%+ coverage (it is the critical engine)
- LogLens.Api: 70%+ coverage on endpoint logic
- Mock all external dependencies (DB, HTTP, Stripe, Claude)

## Next.js Vitest Tests (loglens-web)

### What to test
- Component renders without errors
- User interactions (click, type, submit)
- Error states display correctly
- API calls are made with correct params

### What NOT to test
- Implementation details (internal state)
- Third party library behavior
- CSS/styling

### Mocking
- Always mock lib/api.ts: vi.mock('@/lib/api')
- Mock Next.js router: vi.mock('next/navigation')
- Use MSW for complex API mocking scenarios

## Coverage requirements (CI blocks below these)
- New code coverage: 70% minimum (SonarCloud gate)
- Do not lower existing coverage
```

Create file: .claude/rules/git-rules.md

```markdown
# Git Rules for LogLens

## Branch rules
- NEVER push directly to main or develop
- ALWAYS create a feature branch from develop
- Branch naming: feature/phase-X-description (lowercase, hyphens)
- One feature per branch — do not mix concerns

## Commit rules
- Format: type: short description (lowercase, no period, under 72 chars)
- Types: feat fix test ci security refactor docs chore
- Body: explain WHY not WHAT when non-obvious
- Never commit: secrets, .env files, appsettings.Development.json,
  CLAUDE.md, .claude/, CONTEXT.md, LOGLENS_PHASES.md, .obsidian/

## Before every commit
1. git status — verify only intended files are staged
2. dotnet build — must pass for .NET changes
3. npm run type-check — must pass for frontend changes
4. Check no secrets in diff

## PR rules
- Always use: gh pr create
- Target: feature/* → develop, release/* → main, hotfix/* → main+develop
- PR title matches commit convention
- Wait for CI to pass before merging
- Squash and merge (not regular merge)

## After merging
- Delete the feature branch locally and remotely
- git checkout develop && git pull origin develop
- git branch -d feature/branch-name
```

Create file: .claude/rules/security-rules.md

```markdown
# Security Rules for LogLens

## Secrets — NEVER do these
- Never hardcode API keys, passwords, or tokens in any file
- Never commit .env, appsettings.Development.json, or settings.local.json
- Never log sensitive data (passwords, tokens, card numbers)
- Never put secrets in git commit messages

## API security
- All endpoints that modify data require authentication
- JWT validation must check: issuer, audience, lifetime, signature
- Refresh tokens must be rotated on every use (revoke old, issue new)
- Rate limiting must be applied to all unauthenticated endpoints
- File uploads must validate: size (max 10MB), type (text only)

## Input validation
- Validate all user input server-side — never trust client
- Log file contents are untrusted — handle parse errors gracefully
- SQL injection impossible with EF Core parameterized queries — keep it that way
- XSS prevention via security headers middleware (already configured)

## Response security
- Security headers are set in SecurityHeadersMiddleware — never remove them
- Never return stack traces to clients in production
- Error messages must not leak internal implementation details
- CORS only allows known origins from config (never wildcard in prod)

## Stripe
- Never log Stripe keys or webhook secrets
- Validate webhook signature on every webhook event
- Never store card data — Stripe handles it
```

### STEP 6 — Create skills

Create file: .claude/skills/phase-runner/SKILL.md

```markdown
---
name: phase-runner
description: >
  Execute a LogLens build phase completely. Use this skill when the
  user says "start phase X", "run phase X", "build phase X", or
  "start work on phase X". Reads LOGLENS_PHASES.md and executes
  every step of the requested phase including automated git and PR.
allowed-tools: Bash, Read, Write, Glob
---

# Phase Runner

When invoked, do the following:

1. Read the file LOGLENS_PHASES.md from the project root.
   If it does not exist, tell the user to download it from
   their previous Claude chat session and place it in the repo root.

2. Ask the user which phase to run if not already specified.
   Valid phases: 3 (Next.js frontend), 4 (AI integration),
   5 (Stripe billing), 6 (Deploy + launch).
   Phases 1 and 2 are already complete.

3. Find the section between PHASE X START and PHASE X END markers.

4. Execute every step in that section exactly as written.
   Report progress after each major step completes.
   If a step fails, stop and report the error clearly.

5. At the end, run the AUTOMATED GIT AND PR section exactly
   as written in the phase prompt.

6. Report the PR URL when complete.
   Tell the user to check GitHub Actions to verify CI passes.
```

Create file: .claude/skills/pr-creator/SKILL.md

```markdown
---
name: pr-creator
description: >
  Create a properly formatted GitHub PR following LogLens conventions.
  Use when the user says "create a PR", "open a PR", "make a pull
  request", or "push and PR this".
allowed-tools: Bash(git *), Bash(gh *)
---

# PR Creator

1. Run: git status
   Confirm there are no uncommitted changes.
   If there are, ask the user if they want to commit them first.

2. Run: git branch --show-current
   Confirm you are on a feature branch (not main or develop).
   If on main or develop, stop and tell the user to switch branches.

3. Check current branch has been pushed:
   Run: git log origin/$(git branch --show-current)..HEAD --oneline
   If there are unpushed commits, run:
   git push origin $(git branch --show-current)

4. Ask the user:
   - What does this PR do? (1-2 sentences)
   - What type of change? (feat/fix/ci/refactor/docs)
   - What was tested?

5. Run: gh pr create with this format:
   --base develop
   --head (current branch)
   --title "type: description"
   --body using this template:
   ## What does this PR do?
   {user answer}

   ## Type of change
   - [x] {type}

   ## Changes
   {brief list of what was changed}

   ## Checklist
   - [x] Self-reviewed the diff
   - [x] Build passes (dotnet build / npm run build)
   - [x] Tests pass
   - [x] No secrets committed

6. Report the PR URL.
   Remind user to check GitHub Actions for CI status.
```

Create file: .claude/skills/migration-runner/SKILL.md

```markdown
---
name: migration-runner
description: >
  Safely create and apply EF Core database migrations for LogLens.
  Use when the user says "add migration", "create migration",
  "run migrations", "update database", or "apply migrations".
allowed-tools: Bash(dotnet *), Bash(docker-compose *)
---

# Migration Runner

1. Ensure PostgreSQL is running:
   Run: docker-compose up -d db
   Wait 3 seconds for it to be ready.

2. Ask for migration name if not provided.
   Migration name must be PascalCase describing the change.
   Example: AddUserStripeCustomerId, CreateSessionsTable.

3. Create the migration:
   Run: dotnet ef migrations add {name} --project LogLens.Api
   Show the generated migration file content for review.

4. Ask the user: "Apply this migration to the database? (yes/no)"
   Do NOT proceed without explicit confirmation.

5. If confirmed, apply:
   Run: dotnet ef database update --project LogLens.Api

6. Verify success:
   Run: dotnet ef migrations list --project LogLens.Api
   Confirm the new migration shows as Applied.

7. Report completion. Remind user to commit the migration files:
   git add LogLens.Api/Data/Migrations/
   git commit -m "chore: add {name} migration"
```

Create file: .claude/skills/health-checker/SKILL.md

```markdown
---
name: health-checker
description: >
  Verify that the LogLens API and frontend are running correctly.
  Use when the user says "check health", "is everything running",
  "verify the app", or before running end-to-end tests.
allowed-tools: Bash(curl *), Bash(dotnet *), Bash(docker-compose *)
---

# Health Checker

Run each check and report pass/fail clearly.

## 1. Docker / PostgreSQL
Run: docker-compose ps
Check: db container is running and healthy.

## 2. API health endpoint
Run: curl -s http://localhost:5000/health
Expected: JSON with status "Healthy"
If fail: Check if dotnet run --project LogLens.Api is running.

## 3. API metrics endpoint
Run: curl -s http://localhost:5000/metrics | head -5
Expected: Prometheus metrics output starting with #

## 4. API analyze endpoint
Run:
curl -s -X POST http://localhost:5000/api/analyze \
  -H "Content-Type: application/json" \
  -d '{"rawLog": "2024-03-14 02:14:33 [ERROR] SqlException: timeout"}'
Expected: JSON with detectedFormat, errorGroups, errorCount fields.

## 5. Frontend
Run: curl -s http://localhost:3000 | grep -o "LogLens" | head -1
Expected: "LogLens" found in response.
If fail: Check if npm run dev is running in loglens-web/.

## Summary
Report: PASS/FAIL for each check.
If all pass: "All systems healthy. Ready to develop."
If any fail: Show exact error and how to fix it.
```

### STEP 7 — Create agents

Create file: .claude/agents/code-reviewer.md

```markdown
---
name: code-reviewer
description: >
  Expert code reviewer for LogLens. Reviews code for bugs, security
  issues, performance problems, and convention violations. Use
  PROACTIVELY when reviewing PRs, checking implementations before
  merging, or when the user asks "review this code" or "check this PR".
model: claude-sonnet-4-5
tools: Read, Grep, Glob
---

You are a senior .NET 9 and Next.js 15 engineer reviewing code for
LogLens.io — a production SaaS product. Be thorough but practical.

## Review checklist

### Security (CRITICAL — block merge if found)
- No hardcoded secrets, API keys, or tokens
- JWT claims validated correctly (sub, plan, email)
- No SQL injection (EF Core parameterized — verify)
- File uploads: size and type validated
- No sensitive data logged (passwords, tokens, keys)
- CORS not set to wildcard
- Rate limiting applied to public endpoints

### Correctness (HIGH — should fix before merge)
- Error handling on all async operations
- Null checks where needed (no null reference risk)
- EF Core queries won't cause N+1
- CancellationToken passed through correctly
- Free tier limits enforced (3/day, 500 lines)
- Pro tier check correct (HasClaim "plan" "pro" or "team")

### Conventions (MEDIUM — fix if easy, note otherwise)
- Matches rules in .claude/rules/
- Naming follows project conventions
- No magic numbers — use constants
- No dead code or commented-out code

### Tests (MEDIUM)
- New logic has corresponding tests
- Tests follow naming convention
- No flaky time-dependent tests

## Report format
CRITICAL: {issue} → {fix}
WARNING:  {issue} → {suggestion}
NOTE:     {observation}
APPROVED: Ready to merge (if no critical/warning issues)
```

Create file: .claude/agents/test-writer.md

```markdown
---
name: test-writer
description: >
  Writes comprehensive tests for LogLens. Use when the user says
  "write tests", "add tests for this", "test this function",
  or "improve coverage". Writes both xUnit (.NET) and Vitest (React).
model: claude-sonnet-4-5
tools: Read, Write, Bash(dotnet test *), Bash(npm test *)
---

You are a test engineer for LogLens.io. Write high-quality,
meaningful tests that catch real bugs — not just coverage padding.

## For .NET (xUnit) tests

Target projects:
- LogLens.Core.Tests — unit tests for parser engine
- LogLens.Api.Tests — integration tests for endpoints

Naming:
- Class: {ClassUnderTest}Tests
- Method: {Method}_{Scenario}_{ExpectedResult}

Structure every test:
```csharp
[Fact]
public void MethodName_Scenario_ExpectedResult()
{
    // Arrange
    var input = "...";

    // Act
    var result = system.Method(input);

    // Assert
    Assert.Equal(expected, result.Property);
}
```

Use Theory for multiple inputs:
```csharp
[Theory]
[InlineData("input1", "expected1")]
[InlineData("input2", "expected2")]
public void Parse_MultipleFormats_DetectsCorrectly(string input, string expected)
```

Mock dependencies with NSubstitute or Moq.
Target 85%+ coverage for LogLens.Core, 70%+ for LogLens.Api.

## For React (Vitest) tests

File location: src/__tests__/{ComponentName}.test.tsx

Always mock API client:
```typescript
vi.mock('@/lib/api', () => ({
  analyzeApi: { text: vi.fn(), file: vi.fn() },
  authApi: { login: vi.fn(), me: vi.fn() }
}));
```

Test user behavior not implementation:
- What the user sees (rendered text, elements)
- What happens when user interacts (click, type)
- Error states when API fails
- Loading states during async operations

After writing tests, run them:
- dotnet test (for .NET)
- npm test (for frontend)
Report pass/fail and coverage if available.
```

Create file: .claude/agents/security-auditor.md

```markdown
---
name: security-auditor
description: >
  Security audit specialist for LogLens. Use when the user says
  "security audit", "check for vulnerabilities", "scan for secrets",
  or before creating a release. Checks for common security issues.
model: claude-sonnet-4-5
tools: Read, Grep, Glob, Bash(git *)
---

You are a security engineer auditing LogLens.io before production.
Be thorough. A missed vulnerability in production is worse than
a false positive in review.

## Audit checklist

### 1. Secret scanning
Search all files for patterns:
- sk-ant- (Anthropic API key)
- sk_live_ sk_test_ (Stripe keys)
- whsec_ (Stripe webhook)
- password = "..." (hardcoded passwords)
- Any string matching: [A-Za-z0-9]{32,}

Run: git log --all --full-diff -p | grep -E "sk-ant-|sk_live_|password"

### 2. .gitignore audit
Verify these are gitignored:
- .env
- appsettings.Development.json
- CLAUDE.md
- .claude/
- CONTEXT.md
- LOGLENS_PHASES.md
- .obsidian/

### 3. Authentication audit
- JWT validation parameters set correctly in Program.cs
- Refresh tokens revoked on logout
- Refresh tokens have expiry date
- BCrypt used for password hashing (never MD5/SHA1)
- No plaintext password storage

### 4. API authorization audit
- Every protected endpoint has .RequireAuthorization()
- Pro-only features check correct claim
- Free tier limits enforced server-side (not just client-side)

### 5. Input validation audit
- File upload size limit enforced (10MB)
- File type validated (text only)
- rawLog input not used in any SQL directly
- No path traversal possible in file handling

### 6. Dependency audit
Run: dotnet list package --vulnerable
Run: npm audit (in loglens-web/)
Report any HIGH or CRITICAL vulnerabilities.

## Report format
CRITICAL VULNERABILITY: {description} in {file:line} → {fix immediately}
HIGH RISK: {description} → {fix before launch}
MEDIUM RISK: {description} → {fix soon}
LOW RISK: {description} → {note for future}
CLEAN: {area} — no issues found
```

### STEP 8 — Verify .gitignore is correct

Read the current .gitignore file and confirm it contains:
```
CLAUDE.md
.claude/
```

If those lines are missing, add them.
If they are already there (from STEP 1), confirm and move on.

### STEP 8b — Create tasks folder

Create file: tasks/todo.md

```markdown
# LogLens — Task Tracker

## Current Session
- [ ] (tasks go here as work begins)

## Completed
(moved here when done)

## Review
(summary of what was done and any issues)
```

Create file: tasks/lessons.md

```markdown
# LogLens — Lessons Learned

Rules I follow to avoid repeating mistakes.
Updated after every user correction.

## .NET
(lessons learned about .NET patterns in this project)

## Next.js / React
(lessons learned about frontend patterns)

## Git / CI
(lessons learned about workflow)

## General
(other lessons)
```

Add to .gitignore:
```
# Task tracking (local only)
tasks/
```

### STEP 9 — Verify all files were created

Run this check:
```bash
echo "=== CLAUDE.md ===" && ls -la CLAUDE.md
echo "=== .claude/ ===" && find .claude -type f | sort
echo "=== .gitignore check ===" && grep -n "CLAUDE\|\.claude" .gitignore
```

Expected output: All files listed, .gitignore shows CLAUDE.md and .claude/ as entries.

### STEP 10 — Run git status to confirm nothing is tracked

```bash
git status
```

CLAUDE.md and .claude/ should NOT appear in the output.
If they do appear, run:
```bash
git rm --cached CLAUDE.md 2>/dev/null || true
git rm --cached -r .claude/ 2>/dev/null || true
```

### STEP 11 — Final confirmation

Report:
1. List of all files created
2. Confirmation that .gitignore excludes them
3. Local-only folders and files (gitignored):
   CLAUDE.md, .claude/, tasks/, CONTEXT.md, LOGLENS_PHASES.md
4. Available slash commands (from skills):
   /phase-runner     — run a phase from LOGLENS_PHASES.md
   /pr-creator       — create a GitHub PR
   /migration-runner — create and apply EF migrations
   /health-checker   — verify API + frontend are running
4. Available agents:
   code-reviewer    — auto-invoked on PR reviews
   test-writer      — auto-invoked when writing tests
   security-auditor — invoke before releases
5. How to use: "Start Phase 3" or "/phase-runner" to begin building

## HOW TO USE AFTER SETUP

Starting a phase:
  Just say: "Start Phase 3" or "Run Phase 3"
  The phase-runner skill reads LOGLENS_PHASES.md automatically.

Creating a PR:
  Just say: "Create a PR" or "Push and PR this"
  The pr-creator skill handles the rest.

Running migrations:
  Just say: "Add a migration called AddUserStripeId"
  The migration-runner skill handles it safely.

Checking health:
  Just say: "Check if everything is running"
  The health-checker skill verifies API + frontend.

Code review:
  Just say: "Review this code" or "Check this before I PR"
  The code-reviewer agent runs an audit.

Security audit:
  Just say: "Security audit" or "Check for vulnerabilities"
  The security-auditor agent scans everything.

---

## USING YOUR GLOBALLY INSTALLED SKILLS

You have powerful skills already installed globally. Use these in every session:

### Memory (claude-mem) — solves context window problem
At the start of EVERY session run:
  /mem-search LogLens
This recalls stored LogLens context before starting work.

Other memory commands:
  /make-plan        — creates a plan and stores it in memory
  /do               — executes with memory context
  /smart-explore    — intelligently explores codebase
  /timeline-report  — shows timeline of completed work

### Obsidian (obsidian plugin) — sync to your vault
  /obsidian-cli     — read/write Obsidian vault directly
  /obsidian-markdown — create formatted Obsidian notes

Example — update your vault after a session:
  /obsidian-markdown create a session note for LogLens Phase 3 progress

### UI/UX (ui-ux-pro-max) — auto-invoked for frontend work
Fires automatically when building Next.js components.
Force invoke for review: /ui-ux-pro-max review the analyze dashboard layout

### GSD skills — project management
  /gsd:next         — what to work on next
  /gsd:progress     — show current progress
  /gsd:execute-phase — execute current phase autonomously
  /gsd:debug        — debug a problem
  /gsd:ship         — prepare to ship
  /gsd:health       — project health check
  /gsd:review       — review current work
  /gsd:autonomous   — fully autonomous execution mode
  /gsd:fast         — fast execution mode

### Invocation rules
  Automatic:  Skills fire when your request matches their description
  Manual:     Type /skill-name to force-invoke any skill
  Agents:     Spin up automatically for complex tasks