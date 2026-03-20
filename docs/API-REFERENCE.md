# Finitech API Reference

Base URL: `http://localhost:5000/api`

## Modules

### PartyRegistry
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/partyregistry` | Créer un party (Individual/Organization) |
| GET | `/partyregistry/{id}` | Consulter un party |
| POST | `/partyregistry/{id}/roles` | Assigner un rôle |

### Identity & Compliance
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/compliance/kyc` | Soumettre eKYC |
| POST | `/compliance/kyc/{id}/review` | Approuver/Rejeter KYC |
| POST | `/compliance/strong-actions` | Action forte (Freeze/Unfreeze) |

### Ledger
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/ledger/accounts/{id}/balances` | Balances multi-devise |
| POST | `/ledger/accounts/{id}/history` | Historique des écritures |

### FX (Foreign Exchange)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/fx/rate` | Obtenir un taux |
| POST | `/fx/quote` | Créer une quote de conversion |
| POST | `/fx/convert` | Exécuter la conversion |

### Payments
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/payments/transfer` | Virement intra-devise |
| POST | `/payments/cross-currency-transfer` | Virement cross-devise |
| POST | `/payments/standing-orders` | Ordre permanent |

### Banking
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/banking/savings` | Créer un compte épargne |
| POST | `/banking/accounts/{id}/calculate-interest` | Calculer les intérêts |
| POST | `/banking/loans` | Demande de prêt |
| POST | `/banking/loans/{id}/approve` | Approuver un prêt |
| POST | `/banking/accounts/{id}/overdraft` | Découvert autorisé |

### Wallet
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/wallet/p2p/send` | Envoi P2P |
| POST | `/wallet/split` | Split payment |
| POST | `/wallet/{id}/loyalty/redeem` | Redeem loyalty points |

### WalletFMCG (Agents & Distribution)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/walletfmcg/cash-in` | Cash-In via agent |
| POST | `/walletfmcg/commissions/calculate` | Calculer commission |

### Merchant Payments
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/merchantpayments/qr/generate` | Générer QR EMVCo |
| POST | `/merchantpayments/qr/pay` | Payer par QR |

### Disputes
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/disputes/refund` | Remboursement (partiel/total) |
| POST | `/disputes/chargeback` | Initier un chargeback |

### Notifications
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/notifications/send` | Envoyer (SMS/Email/Push) |

### Budgeting
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/budgeting/budgets` | Définir un budget |
| GET | `/budgeting/analytics` | Analytics de dépenses |

### Documents
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/documents/upload` | Upload document (multipart) |

## Authentication

Toutes les requêtes (sauf `/auth/login` et `/auth/register`) nécessitent un header:
```
Authorization: Bearer {access_token}
```

## Responses

### Success
```json
{ "data": {...}, "success": true }
```

### Error
```json
{ "error": "Description", "code": "ERROR_CODE", "success": false }
```

## Status Codes
| Code | Signification |
|------|--------------|
| 200 | Succès |
| 201 | Créé |
| 400 | Requête invalide |
| 401 | Non authentifié |
| 403 | Non autorisé |
| 404 | Non trouvé |
| 409 | Conflit (idempotency) |
| 429 | Rate limit |
| 500 | Erreur serveur |
