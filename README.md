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

### 3. Wie stellen Sie sicher, dass manche Requests nur mit einem gültigen API-Key aufgerufen werden können?

### 4. Bei welchen Teilen Ihres Systems ist eine korrekte Funktionsweise aus Sicht von BITE am wichtigsten? Welche Maßnahmen haben Sie getroffen, um sie zu gewährleisten?

### 5. Wie stellen Sie sicher, dass ihre API bei der Berechnung des Gesamtpreises für eine Bestellung inkl. Lieferkosten in allen Fällen ein korrektes Ergebnis liefert?

### 6. Welche Maßnahmen haben Sie getroffen, um sicherzustellen, dass alle Bestellungen tatsächlich beim Restaurant ankommen, auch wenn dessen API kurzfristig nicht erreichbar ist?

### 7. Ein Restaurant behauptet, keine Bestellungen übermittelt zu bekommen. Wie analysieren Sie das Problem?

### 8. Wie haben Sie sichergestellt, dass die Lieferbedingungen (Mindestpreis und Versandbedingungen) möglichst einfach erweiterbar sind? Beispielsweise könnten die Versandkosten von der Postleitzahl abhängig werden. Beschreiben Sie die relevanten Stellen des Designs (Klassen-Diagramm).

### 9. Denken Sie an die Skalierbarkeit Ihres Projekts: Plötzlich verwenden tausende Restaurants und zehntausende Studierende das Produkt. Was macht Ihnen am meisten Kopfzerbrechen?

### 10. Haben Sie im Zuge der Projektarbeit Erfahrungen mit dem Einsatz von KI-Agenten zur Generierung von Sourcecode gesammelt? Welche? Was hat gut funktioniert, wo sind Sie gescheitert?

### 11. Wenn Sie das Projekt neu anfangen würden – was würden Sie anders machen?
