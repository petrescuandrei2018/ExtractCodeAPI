ExtractCodeAPI este un API construit pentru a procesa arhive care conțin fișiere sursă, extrăgând codul din acestea și oferindu-l într-un format ușor de utilizat. Scopul principal este de a automatiza procesul de analiză a arhivelor, permițând utilizatorilor să încarce arhivele lor printr-o interfață API, să extragă fișierele sursă și să genereze un fișier consolidat care conține tot codul relevant din arhivă.

Funcționalități principale:

Încărcarea unei arhive:
Utilizatorii pot trimite o arhivă comprimată (de exemplu, .zip, .7z) către API folosind un endpoint specific.

Extragerea fișierelor:
După încărcare, API-ul extrage toate fișierele sursă din arhivă, ignorând directoare și fișiere nedorite (cum ar fi .git sau node_modules).

Generarea unui fișier text consolidat:
API-ul compilează conținutul tuturor fișierelor sursă relevante într-un singur fișier text (export_cod.txt), pentru a facilita analiza ulterioară.

Descărcarea fișierului generat:
Odată procesarea completă, utilizatorii pot descărca fișierul consolidat folosind un endpoint dedicat.

Tehnologii utilizate:

ASP.NET Core Web API:
Framework-ul principal utilizat pentru implementarea serviciilor API.
SevenZipExtractor:
O bibliotecă .NET utilizată pentru extragerea fișierelor din arhive.
Microsoft.Extensions.Logging:
Instrumentul de logare folosit pentru a monitoriza procesul de încărcare, extracție și generare a fișierului consolidat.
Task-uri asincrone și Parallel.ForEachAsync:
Pentru procesarea paralelă și asincronă a fișierelor extrase, în vederea îmbunătățirii performanței.
Git pentru versionare:
Proiectul este urmărit în controlul versiunilor folosind Git, ceea ce permite dezvoltatorilor să gestioneze mai eficient modificările și să colaboreze.
Procesul de lucru:

Utilizatorii trimit o arhivă la un endpoint de tip POST.
API-ul validează arhiva, o extrage și procesează fișierele relevante.
Fișierele sursă extrase sunt consolidate într-un singur fișier text.
Utilizatorii pot descărca fișierul rezultat dintr-un endpoint GET.
Toate operațiunile sunt logate pentru o mai bună trasabilitate și diagnosticare.
Rezultate:
Proiectul facilitează procesarea automată a arhivelor conținând fișiere sursă, reducând timpul necesar pentru extragere și analiză manuală. Codul rezultat este livrat într-un format compact, simplu de analizat, făcându-l util pentru proiecte de audit de cod, centralizare de surse sau alte scopuri similare.
