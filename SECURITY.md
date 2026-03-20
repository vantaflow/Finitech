# Security Policy

## Supported Versions

| Version | Supported |
|---------|-----------|
| 1.x     | ✅        |

## Reporting a Vulnerability

If you discover a security vulnerability, please report it responsibly:

1. **DO NOT** create a public GitHub issue
2. Email: khalilbenaz@protonmail.com
3. Include: description, steps to reproduce, impact assessment
4. Expected response time: 48 hours

## Security Features

### Authentication
- JWT with RSA-2048 signing
- Access tokens: 15 min TTL
- Refresh tokens: 7 days with rotation
- MFA/TOTP support (Google/Microsoft Authenticator)

### Data Protection
- Argon2id password hashing (OWASP recommendation)
- AES-256-GCM encryption for PII
- PCI-compliant card tokenization
- TLS 1.2+ required

### Rate Limiting
- Global: 100 requests/minute
- Auth endpoints: 5 requests/5 minutes
- Transfer endpoints: 20 requests/minute

### Headers
- Content-Security-Policy
- Strict-Transport-Security
- X-Frame-Options: DENY
- X-Content-Type-Options: nosniff
