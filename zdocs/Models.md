# Modele i relacje (minimum 2 tabele 1:N)
Proponowane encje (domena BuildMaX)

## Variant (wariant 1/2/3)
- VariantId
- Name
- Price
- Description
- IncludesPdf
- IncludesPercentDetails
- IncludesSitePlan

## AnalysisRequest (zlecenie analizy działki)
- AnalysisRequestId
- ApplicationUserId (FK do użytkownika)
- VariantId (FK do Variant)
- Address
- PlotAreaM2
- ModuleAreaM2 (np. moduł magazynowy)
- CreatedAt
- Status (np. New/Processing/Done/Rejected)
- Result (nawigacja 1:1 lub po prostu pola wynikowe w tej tabeli)

## LegalDocument (lista dokumentów / podstaw prawnych)
- LegalDocumentId
- VariantId (FK) albo AnalysisRequestId (FK) – zależy od podejścia
- Title
- Url
- Category

### Relacje 1:N (spełnią warunek):
- Variant (1) -> AnalysisRequest (N)
- ApplicationUser (1) -> AnalysisRequest (N)
- (opcjonalnie) Variant (1) -> LegalDocument (N) lub AnalysisRequest (1) -> LegalDocument (N)

## Więzy integralności danych (FK + OnDelete)
W EF Core ustawiam:
- AnalysisRequest wymaga istniejącego Variant i User.
- sensownie: usunięcie Variant blokowane, jeśli są zlecenia (Restrict), a usunięcie usera może usuwać zlecenia (Cascade) lub też blokować – zależnie od wymagań.