[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)](https://classroom.github.com/a/BfRaPdoe)

# Dokumentation BITE
### Bestell- und Informationssystem für Takeaway-Essen

#### Das Projekt.
Der Trend, Essen online zu bestellen, ist ungebrochen. Deshalb entwickeln Sie eine App, die alle
derzeit am Markt befindlichen Systeme (zumindest technologisch) alt aussehen lässt. Restaurants veröffentlichen ihre Speisekarte, hungrige Studierende bestellen ihre Lieblingsspeisen. 

## Setup 

#### Starten der Db
Analog zu Übung mit dem Befehl "docker compose up" im Order /db-sqlserver/

#### tbc

## Technischer Aufbau

  - Backend: .NET-Projektstruktur mit Bite.Api, Bite.Domain, Bite.Dal, Bite.Services, Bite.Tests und
    Bite.ConsoleTester (aktuell noch als Testprojekt dabei, wird in der finalen Abgabe noch entfernt)
  - API: ASP.NET Core Web API mit Grundsetup; Controller sind aktuell noch nicht umgesetzt
  - Domain-Modell: Klassen für Restaurant, Address, MenuItem, MenuCategory, OpeningHourSlot, DeliveryZone und DeliveryFeeRule
  - Datenzugriff: ADO.NET-basierte DAO-Schicht mit Interfaces und Implementierungen für Restaurants,
    Adressen, Menüs, Öffnungszeiten und Lieferregeln
  - Datenbank: Microsoft SQL Server über Docker Compose
  - Tests: xUnit-Tests für DAO-Funktionalität, z. B. Restaurants, Adressen und Menüs
  - CI/CD: GitHub Actions Workflow 

### Datenbankmodell

  Die Datenbank BiteTestDb enthält Tabellen für Restaurants, Adressen, Speisekategorien, Speisen,
  Öffnungszeiten, Lieferzonen und Liefergebühren. Beziehungen werden über Foreign Keys abgesichert;
  Kategorien sind über eine Zwischentabelle verknüpft. Trigger aktualisieren
  updated_at bei Änderungen an Restaurants und Speisen.

### Testdaten

  Das SQL-Setup legt Beispielrestaurants wie Restaurant Nimmersatt, Burger Bude Wien und Sakura Sushi
  mit Speisen, Kategorien, Öffnungszeiten, Lieferzonen und Liefergebühren an.

### Projektstand
  Das Projekt enthält bereits ein solides Datenmodell, SQL-Setup, Domain-Klassen und eine DAO-
  Schicht. Die eigentliche Web-API, Service-Logik, etc. sind jedoch noch
  nicht umgesetzt (siehe Ausbaustufe 2).

## Fragen 
###### werden in Ausbaustufe 2 beantwortet 

### 1. Für welches Datenmodell haben Sie sich entschieden? ER-Diagramm, etwaige Besonderheiten erklären: Welche Entscheidungen mussten Sie treffen, wofür (und wogegen) haben Sie sich entschieden und warum?

### 2. Dokumentieren Sie auf Request-Ebene den gesamten Workflow anhand eines durchgehenden Beispiels (von der Registrierung eines Restaurants bis zur Abfrage des Bestellstatus). Sie können ein Tool Ihrer Wahl einsetzen, z. B. Postman Workflows, VS Code, etc. HTTP-Requests inkl. HTTP-Verb, URL, Parametern, Body und Headern.

Der vollständige, ausführbare Workflow – mit allen URLs, Parametern, Bodies und Headern – ist als lesbare HTTP-Datei im Repo hinterlegt: [`Bite/Bite.Api/Bite.Api.http`](Bite/Bite.Api/Bite.Api.http) (ausführbar in Visual Studio bzw. VS Code mit der „REST Client"-Extension). Basis-URL: `http://localhost:5126`; geschützte Requests benötigen den Header `X-Api-Key`.

Kurzerläuterung des durchgängigen Beispiels (Registrierung → Bestellstatus):

1. **POST `/api/Restaurants`** (US 1, multipart/form-data) – Restaurant registrieren. → liefert `restaurantId` + `apiKey` (Key für alle folgenden geschützten Requests).
2. **PUT `/api/Restaurants/{restaurantId}/delivery-conditions`** (US 3, `X-Api-Key`) – Lieferzonen & -kosten festlegen.
3. **PUT `/api/restaurants/{restaurantId}/menu`** (US 4, `X-Api-Key`) – Speisekarte anlegen/ersetzen.
4. **GET `/api/Restaurants?latitude=…&longitude=…&openNow=…&count=…`** (US 5) – Restaurants in der Nähe suchen (Kundensicht).
5. **GET `/api/restaurants/{restaurantId}/menu`** (US 6) – Speisekarte abrufen.
6. **POST `/api/restaurants/{restaurantId}/orders/price`** (US 7) – Preisvorschau inkl. Lieferkosten.
7. **POST `/api/restaurants/{restaurantId}/orders`** (US 8) – Bestellung verbindlich aufgeben. → liefert `orderCode`; zusätzlich geht ein Webhook (US 9) an das Restaurant, der die **Status-Änderungs-Links** enthält (je ein einmalig gültiges Token pro Folgestatus).
8. **GET `/api/Orders/{orderCode}/status`** (US 12) – Bestellstatus abfragen.
9. **Status ändern** – zwei Wege, beide mit `X-Api-Key`:
   - **PATCH `/api/Orders/{orderCode}`** (US 10) mit Body `{ "status": "SENT_TO_RESTAURANT" }`, oder
   - **GET `/api/Orders/{orderCode}/status-change/{token}`** (US 11) – Link aus dem Webhook (Token aus Schritt 7).

   Erlaubte Reihenfolge: `RECEIVED → SENT_TO_RESTAURANT → IN_PREPARATION → OUT_FOR_DELIVERY → DELIVERED` (oder `CANCELLED`); ungültige Sprünge werden mit `422` abgelehnt.
10. **GET `/api/Orders/{orderCode}/status`** – erneute Abfrage zeigt den geänderten Status.

Datenfluss zwischen den Schritten: `apiKey` ← Schritt 1, `orderCode` ← Schritt 7, `token` ← Webhook (Schritt 7). In der `.http`-Datei laufen die Requests gegen das vorbefüllte Restaurant `id = 1` (Key `nimmersatt-api-key-2026`) und sind damit ohne vorherige Registrierung direkt ausführbar.

### 3. Wie stellen Sie sicher, dass manche Requests nur mit einem gültigen API-Key aufgerufen werden können?

### 4. Bei welchen Teilen Ihres Systems ist eine korrekte Funktionsweise aus Sicht von BITE am wichtigsten? Welche Maßnahmen haben Sie getroffen, um sie zu gewährleisten?

### 5. Wie stellen Sie sicher, dass ihre API bei der Berechnung des Gesamtpreises für eine Bestellung inkl. Lieferkosten in allen Fällen ein korrektes Ergebnis liefert?

### 6. Welche Maßnahmen haben Sie getroffen, um sicherzustellen, dass alle Bestellungen tatsächlich beim Restaurant ankommen, auch wenn dessen API kurzfristig nicht erreichbar ist?

### 7. Ein Restaurant behauptet, keine Bestellungen übermittelt zu bekommen. Wie analysieren Sie das Problem?

### 8. Wie haben Sie sichergestellt, dass die Lieferbedingungen (Mindestpreis und Versandbedingungen) möglichst einfach erweiterbar sind? Beispielsweise könnten die Versandkosten von der Postleitzahl abhängig werden. Beschreiben Sie die relevanten Stellen des Designs (Klassen-Diagramm).

### 9. Denken Sie an die Skalierbarkeit Ihres Projekts: Plötzlich verwenden tausende Restaurants und zehntausende Studierende das Produkt. Was macht Ihnen am meisten Kopfzerbrechen?

### 10. Haben Sie im Zuge der Projektarbeit Erfahrungen mit dem Einsatz von KI-Agenten zur Generierung von Sourcecode gesammelt? Welche? Was hat gut funktioniert, wo sind Sie gescheitert?

### 11. Wenn Sie das Projekt neu anfangen würden – was würden Sie anders machen?
