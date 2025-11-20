# 📋 MODULI DA SISTEMARE - MIGRAZIONE SQL SERVER

## ✅ GIÀ SISTEMATI (funzionanti):
1. ✅ LoginViewModel
2. ✅ DashboardViewModel
3. ✅ SistemaViewModel
4. ✅ MainViewModel
5. ✅ ClientiViewModel (20/11/2025)
6. ✅ ProfessionistiViewModel (20/11/2025)
7. ✅ TipoPraticaViewModel (20/11/2025)
8. ✅ UtentiViewModel (20/11/2025)

## ❌ DA SISTEMARE (usano ancora LiteDbContext):

### Priorità ALTA (causano crash se usati):
1. ❌ UtentiViewModel - PRIORITÀ PROSSIMA
2. ✅ **ClientiViewModel - COMPLETATO 20/11/2025**
3. ✅ **ProfessionistiViewModel - COMPLETATO 20/11/2025**
4. ✅ **TipoPraticaViewModel - COMPLETATO 20/11/2025**

### Priorità MEDIA (moduli bilanci):
5. ❌ BilancioContabileViewModel
6. ❌ BilancioDettaglioViewModel
7. ❌ BilancioDialogViewModel
8. ❌ BilancioTemplateViewModel
9. ❌ BilancioTemplateDettaglioViewModel
10. ❌ ImportBilancioViewModel
11. ❌ StatisticheBilanciViewModel
12. ❌ StatisticheBilanciCEViewModel
13. ❌ StatisticheBilanciSPViewModel
14. ❌ IndiciDiBilancioViewModel
15. ❌ ConfigurazioneIndiciViewModel
16. ❌ IndicePersonalizzatoDialogViewModel

### Priorità MEDIA (moduli banche):
17. ❌ GestioneBancheViewModel
18. ❌ BancaDettaglioViewModel
19. ❌ RiepilogoBancheViewModel
20. ❌ IncassoDialogViewModel
21. ❌ PagamentoDialogViewModel
22. ❌ PagamentoMensileDialogViewModel
23. ❌ AnticipoDialogViewModel

### Priorità MEDIA (altri moduli):
24. ❌ TodoStudioViewModel
25. ❌ ArgomentiViewModel
26. ❌ RicercaCircolariViewModel
27. ❌ ImportaCircolareViewModel
28. ❌ ModificaCircolareDialogViewModel
29. ❌ AssociazioniMastriniViewModel
30. ❌ AssociazioneMastrinoDialogViewModel
31. ❌ GraficiViewModel
32. ❌ LicenseManagerViewModel

## 🔧 STRATEGIA:

Per ora DISABILITIAMO tutti i moduli non sistemati mostrando:
"⚠️ MODULO IN MIGRAZIONE A SQL SERVER - Disponibile a breve"

Poi li migrerete uno alla volta seguendo la guida.

