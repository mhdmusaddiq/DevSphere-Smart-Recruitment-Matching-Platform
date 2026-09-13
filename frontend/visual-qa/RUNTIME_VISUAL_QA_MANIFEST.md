# AptLens Runtime Visual QA Manifest

Date: 2026-09-13
Branch: `design/frontend-final-visual-20260913`
Runtime: Angular development server at `http://127.0.0.1:4200`

## Responsive checks

Browser automation verified `/`, `/login`, and `/jobs` at 1440×1024, 1024×768,
768×1024, 390×844, and 320×720. The document did not overflow horizontally at
any checked width. The 403 and 404 states were also checked at the safety width.

The guest-session route smoke verified that seeker, employer, and admin protected
routes preserve their safe encoded `returnUrl` while redirecting to `/login`.
Authenticated data states remain covered by the Angular component and guard suite;
no QA credentials or seeded backend state were supplied for this pass.

## Captures

| Capture | Bytes | SHA-256 |
|---|---:|---|
| `404-mobile-390x844.png` | 15,243 | `7ec0f68c9411b32ba37590312eecde9964989510f1a5b4b24e52b887edf2cc2f` |
| `jobs-safety-320x720.png` | 54,047 | `25111b9b9d3186a193d31ae99d7ec1e2447cd87163641991a81dbb2e4b77be27` |
| `jobs-tablet-768x1024.png` | 93,843 | `c46ac7b804b3da83ec9b9fcb5e854b6736501cad57bd3774f5792a7c8d42ff1d` |
| `landing-desktop-1440x1024.png` | 436,343 | `ffe363bf67e9296f8677c131e4f8c81f3084445b8b4fd29898252e7d459c9a48` |
| `landing-mobile-390x844.png` | 57,020 | `7f21e3d91036c953240f181050e8cf14fefb4a42fc5209fcbcca5d25c17ddee2` |
| `login-desktop-1440x1024.png` | 816,633 | `aaa8bc641db3f59b8501554cd8a7f2dcfa375e9bdec940746a58d41e904c26e2` |

## QA closeout

- No horizontal overflow at checked widths.
- Mobile information order remains task-first.
- Public and auth text remains readable over approved media.
- Protected routes retain guard and return URL behavior.
- 403/404 state cards remain centered and unclipped.
- Reduced-motion override remains global.
