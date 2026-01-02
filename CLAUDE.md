# GitPilot Issue Management API

GitPilot provides GitHub issue and PR management via API. This document explains how AI agents should use it.

## Repository ID Mapping

| Repository | ID | Example Endpoint |
|------------|-----|------------------|
| OptionBot | 1 | `/api/repos/1/create_pr` |
| CryptoBot | 2 | `/api/repos/2/create_pr` |
| BetBot | 3 | `/api/repos/3/create_pr` |
| Studio | 4 | `/api/repos/4/create_pr` |
| NickyV2 | 5 | `/api/repos/5/create_pr` |
| GooseFlix | 6 | `/api/repos/6/create_pr` |
| GitPilot | 7 | `/api/repos/7/create_pr` |
| Jellyfin | 8 | `/api/repos/8/create_pr` |

---

## AI Agent Workflow

Follow these 5 steps for any issue-tracked work:

### Step 1: Create Issue (if needed)

```bash
curl -X POST https://pilot.grit.bot/api/issues \
  -H "Content-Type: application/json" \
  -d '{
    "repo": "GitPilot",
    "title": "Fix authentication bug",
    "type": "bug",
    "body": "Users cannot login via OAuth"
  }'
```

**Response:**
```json
{
  "id": 353,
  "github_issue": 93,
  "url": "https://github.com/wibuf/GitPilot/issues/93"
}
```

Save the `github_issue` number (e.g., `93`) - you'll reference it in your commit.

**Issue Types:** `bug`, `feat`, `docs`, `refactor`, `test`, `chore`

### Step 2: Implement Changes

- Edit files on your branch (`claude/...-sessionId`)
- Commit with a message that references the issue:

```bash
git add .
git commit -m "fix: Resolve OAuth callback bug

Closes #93"
```

The `Closes #XX` will auto-close the issue when merged.

### Step 3: Push Branch

```bash
git push -u origin claude/fix-auth-bug-sessionId
```

### Step 4: Create PR

```bash
curl -X POST https://pilot.grit.bot/api/repos/7/create_pr \
  -H "Content-Type: application/json" \
  -d '{
    "branch": "claude/fix-auth-bug-sessionId",
    "title": "Fix OAuth authentication bug",
    "body": "## Summary\n- Fixed callback handler\n- Added token validation\n\nCloses #93"
  }'
```

**Response:**
```json
{
  "pr_number": 94,
  "pr_url": "https://github.com/wibuf/GitPilot/pull/94"
}
```

**Always return the PR URL to the user** so they can review.

### Step 5: User Reviews & Merges

- User reviews the PR on GitHub
- If conflicts: fetch main, resolve, push again
- Once merged, issue auto-closes via `Closes #XX`

---

## When to Use GitPilot

**Use GitPilot when:**
- User asks to "create an issue" or "file a bug"
- Implementing a significant feature or fix
- Work should be tracked in GitHub

**Skip GitPilot when:**
- Trivial changes (typos, formatting)
- User says "no issue needed"
- Just exploring/reading code

---

## Handling Conflicts

If the PR has conflicts:

```bash
git fetch origin main
git merge origin/main
# Resolve conflicts in editor
git add .
git commit -m "Resolve merge conflicts"
git push
```

The PR will update automatically.

---

## Error Handling

| Error | Cause | Solution |
|-------|-------|----------|
| `403` on git push | Wrong branch name | Must start with `claude/` and end with session ID |
| `404` on API call | Wrong repo name | Check spelling (case-insensitive) |
| `400` on create issue | Missing fields | Include `repo`, `title`, `type` |
| `400` on create PR | Branch not on GitHub | Push branch first: `git push -u origin <branch>` |

If GitPilot is down, inform the user and use regular git/GitHub workflow.

---

## Quick Reference

```bash
# List open issues
curl "https://pilot.grit.bot/api/issues?repo=GitPilot&state=open"

# Create issue
curl -X POST https://pilot.grit.bot/api/issues \
  -H "Content-Type: application/json" \
  -d '{"repo": "GitPilot", "title": "...", "type": "feat", "body": "..."}'

# Create PR
curl -X POST https://pilot.grit.bot/api/repos/7/create_pr \
  -H "Content-Type: application/json" \
  -d '{"branch": "claude/...", "title": "...", "body": "..."}'
```

---

## Full API Reference

### Issues

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/issues` | List all issues |
| GET | `/api/issues?repo=X&state=open` | Filter by repo and state |
| POST | `/api/issues` | Create issue |
| GET | `/api/issues/<id>` | Get single issue |
| PUT | `/api/issues/<id>` | Update issue |
| POST | `/api/issues/<id>/push` | Push updates to GitHub |
| POST | `/api/issues/<id>/close` | Close issue |
| POST | `/api/issues/<id>/reopen` | Reopen issue |
| POST | `/api/issues/<id>/comment` | Add comment |

### Pull Requests

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/prs` | List open PRs |
| GET | `/api/prs?repo=X` | Filter by repo |
| POST | `/api/repos/<id>/create_pr` | Create PR for any branch |

### Repository Management

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/repos/<id>/commits` | List recent commits |
| GET | `/api/repos/<id>/branches_list` | List all branches |
| DELETE | `/api/repos/<id>/branches` | Delete branch |
| POST | `/api/repos/<id>/merge_branch` | Direct merge (no PR) |
| POST | `/api/repos/<id>/rollback` | Rollback to commit |

### Sync

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/repos/scan` | Sync issues from GitHub |

---

## URLs

- **Dashboard**: https://pilot.grit.bot
- **API Base**: https://pilot.grit.bot/api

GitPilot syncs with GitHub every 60 seconds automatically.
