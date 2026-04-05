# Security Testing Playbook

## 1) Test Matrix (With Evidence)

| TC ID | Group | Test Case | Execution Steps | Expected | Actual | Evidence |
|---|---|---|---|---|---|---|
| SEC-01 | CSRF | Submit login without anti-forgery token | Open `/Authentication/Login`, replay POST without `__RequestVerificationToken` | Request blocked (typically HTTP 400) | ... | Network screenshot + response |
| SEC-02 | CSRF | Submit login with valid anti-forgery token | Normal login flow | Login success (redirect to local page) | ... | Network screenshot + redirect URL |
| SEC-03 | Brute Force | Wrong password repeated on same account | Submit invalid password 6 times | Account temporarily locked, retry time displayed/logged | ... | UI message + `security-audit.log` |
| SEC-04 | IDOR | Customer A edits booking of customer B | Login A, call `/Account/EditBooking/{bookingOfB}` | Denied and logged as `idor_blocked` | ... | URL/result + log + DB no-change |
| SEC-05 | IDOR | Customer A cancels booking of customer B | Login A, call `/Account/CancelBooking` with id of B | Denied and logged as `idor_blocked` | ... | UI/result + log + DB no-change |
| SEC-06 | SQLi | SQL injection payload in login/search input | Input `' OR 1=1 --` and submit | No authentication bypass / no abnormal data return | ... | Request/response screenshots |
| SEC-07 | Open Redirect | External `returnUrl` at login | Open `/Authentication/Login?returnUrl=https://evil.com`, login | Redirect remains local, blocked action logged | ... | Final URL + `open_redirect_blocked` log |
| SEC-08 | Rate Limit | Chatbot flood requests | Send >20 requests/minute to `/ChatBot/Ask` | Later requests return HTTP 429 | ... | Console output + response JSON + log |
| SEC-09 | Input Validation | Chatbot message > 500 chars | POST `/ChatBot/Ask` with 501 chars | HTTP 400 + length warning | ... | Response screenshot |
| SEC-10 | Input Validation | Empty/control-char chatbot message | POST message with only `\r\n\t` | Validation error response, no processing | ... | Response screenshot |
| SEC-11 | Session AuthZ | Access protected routes without login | Visit `/Account`, `/Admin/Dashboard`, `/Staff/Operations` | Redirect to corresponding login pages | ... | URL redirect screenshots |
| SEC-12 | Headers/Cookies | Verify hardening headers and cookie flags | Open DevTools > Network + Application/Cookies | Presence of CSP/HSTS/XFO/nosniff + HttpOnly/Secure/SameSite | ... | DevTools screenshots |
| SEC-13 | Registration Security | CAPTCHA + confirmation code flow | Wrong captcha, wrong code, expired code | Registration blocked appropriately | ... | UI screenshots + optional log |
| SEC-14 | Password Storage | Register new account then inspect DB | Query `KHACHHANG.MatKhau` for new user | Stored value starts with `$DKS$PBKDF2$`, no plaintext | ... | SSMS screenshot |

## 2) Before/After Fix Analysis

### A. Open Redirect

- Vulnerability type: Unvalidated redirect.
- Before fix:
  - Attacker can craft login URL with external `returnUrl` and trick user to authenticate.
  - Potential phishing pivot after successful login.
- Fix applied:
  - Centralized safe redirect method checks `Url.IsLocalUrl(...)`.
  - External URL is rejected and audit event is logged (`login_return_url_rejected` / `google_login_state_rejected`).
- After fix:
  - Login always lands on local pages only.
  - Blocking attempts are visible in `App_Data/security-audit.log`.

### B. IDOR (Edit/Cancel Booking)

- Vulnerability type: Insecure Direct Object Reference.
- Before fix:
  - If only booking id is trusted, user A may attempt booking actions on user B.
- Fix applied:
  - Booking queries already constrained by owner (`ct.KHACH == customerId`).
  - Added explicit `idor_blocked` audit log when target booking exists but is not owned by requester.
- After fix:
  - Unauthorized booking access is denied.
  - Security log contains traceable evidence (customerId, targetBookingId, operation, ip).

## 3) Execution Script

Run:

```powershell
.\Scripts\security-evidence.ps1 `
  -BaseUrl "https://localhost:44380" `
  -ValidUsername "customer1" `
  -ValidPassword "YourPassword123!" `
  -BruteForceUsername "customer1"
```

After run:

1. Capture terminal output.
2. Capture browser/DevTools screenshots for relevant cases.
3. Capture `App_Data/security-audit.log` lines generated during tests.

