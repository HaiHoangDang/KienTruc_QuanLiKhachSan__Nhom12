# Security Controls Overview

## 1) Authentication and Session-Based Authorization

- Customer/Admin/Staff authentication is implemented via login controllers and session state.
- Session keys:
  - Customer: `KhachHang`, `KhachHangId`, `KhachHangTen`
  - Staff/Admin: `AdminUser`, `AdminId`, `AdminName`
- Protected routes use custom attributes:
  - `CustomerAuthorizeAttribute`
  - `AdminAuthorizeAttribute`
  - `StaffAuthorizeAttribute`
- Unauthorized access is redirected to appropriate login pages with safe return path handling.

## 2) Password Security (PBKDF2 + Salt + Fixed-Time Comparison)

- Password hashing:
  - Algorithm: PBKDF2 with SHA-256 (`Rfc2898DeriveBytes`)
  - Salt: 16 bytes random
  - Hash: 32 bytes
  - Iterations: 100000
- Stored format: `$DKS$PBKDF2$<iterations>$<salt-base64>$<hash-base64>`
- Verification uses fixed-time byte comparison to reduce timing attack signal.
- Legacy plaintext compatibility exists; successful legacy login triggers re-hash migration.

## 3) CSRF Protection

- Forms render anti-forgery tokens via `@Html.AntiForgeryToken()`.
- State-changing POST actions use `[ValidateAntiForgeryToken]`.
- Requests missing or forging token are rejected by MVC validation pipeline.

## 4) Brute-Force Mitigation (Login Lockout)

- `LoginSecurityService` tracks failed attempts per `(username, ip)` key.
- Policy:
  - Window: 10 minutes
  - Max failures: 5
  - Lockout: 15 minutes
- Applied in customer/staff/admin login flows.

## 5) Chatbot Abuse Mitigation (Rate Limit + Input Constraints)

- `SecurityRateLimiter` throttles `/ChatBot/Ask`.
- Policy:
  - Window: 1 minute
  - Max: 20 requests
  - Block: 3 minutes
- Message controls:
  - Remove control characters (except line breaks/tab)
  - Max message length: 500 chars
  - Empty message blocked

## 6) Open Redirect Protection

- Login redirection validates destination with `Url.IsLocalUrl(...)`.
- External return URLs are rejected and fallback redirect is applied.
- Blocked attempts are logged for audit evidence.

## 7) IDOR Protection in Account Booking Flows

- Booking read/update/cancel operations filter by current session customer ownership.
- Requests targeting existing but non-owned booking ids are denied.
- Suspicious blocked attempts are logged as `idor_blocked`.

## 8) Input Validation and Model Binding Safety

- ViewModels use `DataAnnotations` (`Required`, `Compare`, `Phone`, `Range`, etc.).
- Controllers enforce `ModelState.IsValid` before persistence.
- Several actions use `[Bind(...)]`/prefix binding to reduce unwanted model over-posting.

## 9) SQL Injection Mitigation

- Primary data access is Entity Framework LINQ (parameterized SQL generation).
- Raw SQL usage includes parameter placeholders (`@p0`) instead of string concatenation.

## 10) Cookie and Transport Hardening

- Web cookie policy in `Web.config`:
  - `httpOnlyCookies="true"`
  - `requireSSL="true"`
  - `sameSite="Lax"`
- Remember-me cookie is set `HttpOnly`, `Secure`, `SameSite=Lax`.
- Security headers:
  - `X-Content-Type-Options: nosniff`
  - `X-Frame-Options: SAMEORIGIN`
  - `Referrer-Policy: strict-origin-when-cross-origin`
  - `Content-Security-Policy: ...`
  - `Strict-Transport-Security: max-age=31536000; includeSubDomains`

## 11) Secret Management

- Secrets are read via `SecurityConfig.GetSecret(...)`:
  - Priority 1: environment variables
  - Priority 2: appSettings non-placeholder values
- Placeholder appSettings are intentionally ignored.

## 12) Security Audit Logging

- `SecurityAuditLogger` writes JSON-line events into `App_Data/security-audit.log`.
- Logged security domains include:
  - Login success/failure/lockout
  - Password changes
  - Chatbot rate-limit blocks/errors
  - Open redirect blocks
  - IDOR blocks

## 13) Registration Hardening

- CAPTCHA challenge on registration entry.
- Email confirmation code flow:
  - Numeric code generated and emailed
  - Code stored in session with expiry (10 minutes)
  - Invalid/expired code blocked

