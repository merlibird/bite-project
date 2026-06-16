## 1. Für welches Datenmodell haben Sie sich entschieden? Erklären Sie das ER-Diagramm und etwaige Besonderheiten: Welche Entscheidungen mussten Sie treffen, wofür (und wogegen) haben Sie sich entschieden und warum?

Wir haben uns für ein relationales SQL-Modell entschieden, da es ACID-Transaktionen und referenzielle Integrität bietet – essenziell für Bestellungen und Preiskalkulationen. 

Zentrale Entscheidungen: Wir haben Adressen normalisiert, um Redundanz zu vermeiden und Geodaten für Routenoptimierung zu ermöglichen. Menüpunkte und -kategorien verknüpften wir über eine Junction-Tabelle m:n, damit ein Gericht mehreren Kategorien zugeordnet werden kann (z.B. Pommes bei Kinderkarte und bei Beilagen). Liefergebühren legten wir staffelbar nach Bestellwert an. Den Bestellstatus begrenzten wir per CHECK-Constraint auf gültige Werte. Für sichere Status-Updates setzen wir Einmal-Token ein, für zuverlässige Webhook-Zustellung das Outbox-Pattern mit Wiederholungslogik.


## 2. Dokumentieren Sie auf Request-Ebene den gesamten Workflow anhand eines durchgängigen Beispiels (von der Registrierung eines Restaurants bis zur Abfrage des Bestellstatus). Sie können ein Tool Ihrer Wahl einsetzen, z. B. Postman Workflows, VS Code etc. – inklusive HTTP-Requests mit HTTP-Verb, URL, Parametern, Body und Headern. 


## 3. Wie stellen Sie sicher, dass manche Requests nur mit einem gültigen API-Key aufgerufen werden können?

Wir haben einen **zentralen Authentifizierungsmechanismus** in Form eines **`ApiKeyAuthAttribute`** implementiert, das als **IAsyncAuthorizationFilter** fungiert. Dieser wird bei allen geschützten Endpunkten über das `[ApiKeyAuth]`-Attribut deklariert (z. B. bei `PATCH /api/Orders/{orderCode}` oder `PUT /api/Restaurants/{id}/menu`).

Der Ablauf im Detail: Der API-Key wird aus dem **HTTP-Header `X-Api-Key`** ausgelesen – nicht aus dem Body, da dies der REST-Konvention entspricht und die Header-Ebene für Authentifizierungsdaten vorgesehen ist. Der Key wird dann über den `IApiKeyService` **gehasht** und mit dem in der Datenbank gespeicherten Hash abgeglichen. Existiert das Restaurant, wird dessen `Id` im `HttpContext.Items`-Dictionary hinterlegt, sodass nachgelagerte Controller-Methoden den authentifizierten Restaurant-Kontext nutzen können (z. B. zur Autorisierung, dass ein Restaurant nur eigene Bestellungen oder sein eigenes Menü ändert). Fehlt der Key oder ist er ungültig, antwortet der Filter sofort mit **401 Unauthorized**, ohne dass die Controller-Logik erreicht wird. So stellen wir sicher, dass geschützte Routen ausschließlich für authentifizierte Restaurants zugänglich sind – eine zusätzliche Berechtigungsprüfung (z. B. `if (id != authenticatedRestaurantId) return Forbid()`) verhindert zudem den Zugriff auf fremde Ressourcen.

## 4. Bei welchen Teilen Ihres Systems ist eine korrekte Funktionsweise aus Sicht von BITE am wichtigsten? Welche Maßnahmen haben Sie getroffen, um sie zu gewährleisten?

## 5. Wie stellen Sie sicher, dass Ihre API bei der Berechnung des Gesamtpreises für eine Bestellung inklusive Lieferkosten in allen Fällen ein korrektes Ergebnis liefert?

## 6. Welche Maßnahmen haben Sie getroffen, um sicherzustellen, dass alle Bestellungen tatsächlich beim Restaurant ankommen, auch wenn dessen API kurzfristig nicht erreichbar ist?

## 7. Ein Restaurant behauptet, keine Bestellungen übermittelt zu bekommen. Wie analysieren Sie das Problem?

## 8. Wie haben Sie sichergestellt, dass die Lieferbedingungen (Mindestpreis und Versandbedingungen) möglichst einfach erweiterbar sind? Beispielsweise könnten die Versandkosten von der Postleitzahl abhängig werden. Beschreiben Sie die relevanten Stellen des Designs (Klassendiagramm).

## 9. Denken Sie an die Skalierbarkeit Ihres Projekts: Plötzlich verwenden tausende Restaurants und zehntausende Studierende das Produkt. Was macht Ihnen am meisten Kopfzerbrechen?

## 10. Haben Sie im Zuge der Projektarbeit Erfahrungen mit dem Einsatz von KI-Agenten zur Generierung von Sourcecode gesammelt? Wenn ja, welche? Was hat gut funktioniert, wo sind Sie gescheitert?

Wir haben im Rahmen der Projektarbeit KI-Werkzeuge (insbesondere ChatGPT, Claude, Gemini) im Frage-Modus eingesetzt.

Konkret nutzten wir KI für Bereiche mit hohem Boilerplate-Anteil, etwa grundlegende CRUD-Operationen, DTO-Mappings und Validierungslogik. Auch bei der Erstellung von Testdaten und Seed-Skripten sowie beim Debugging komplexer Fehlermeldungen (z. B. LINQ-Ausdrücke) war die KI hilfreich.

Gut funktioniert hat die schnelle Bereitstellung strukturierter Code-Grundgerüste und die Erklärung von Framework-Konzepten wie ASP.NET Core Middleware oder Webhook-Konzpten. 

Gescheitert sind wir hingegen bei domänenspezifischen Geschäftslogiken wie der korrekten Staffelung von Liefergebühren oder dem Outbox-Pattern – hier lieferte die KI oberflächliche Lösungen ohne Verständnis für unseren Architekturkontext, da es auch nicht möglich war ausreichend Dateien hochzuladen, um ihm den notwendigen Kontext zu geben.




## 11. Wenn Sie das Projekt neu anfangen würden – was würden Sie anders machen?

Rückblickend würden wir einige Entscheidungen anders treffen. Zunächst würden wir mehr Zeit in die initiale Architektur- und API-Schnittstellendefinition investieren. Gerade bei der Kommunikation zwischen den einzelnen Modulen (z. B. zwischen Restaurant-, Menü- und Order-Service) haben wir im Laufe des Projekts wiederholt Anpassungen vornehmen müssen, weil Abhängigkeiten nicht ausreichend durchdacht waren.

Auch beim Datenmodell würden wir uns m:n Beziehungen genauso wie soft_deletes besser überöegen, da das im Laufe des Entwicklungsprozesses immer wieder zu Problemen geführt hat. 

Zu guter Letzt würden wir mehr automatisierte Tests von Anfang an schreiben. Unit-Tests für die Geschäftslogik und Integrationstests für die Datenbankzugriffe kamen im Projektverlauf zu kurz und wurden erst spät nachgeholt – das führte zu unnötigem manuellem Testaufwand bei Änderungen.

Zusammenfassend: Wir würden Architektur und Schnittstellen gründlicher vorab definieren, das Datenmodell flexibler gestalten, ein konsistentes Fehlerhandling etablieren und Testautomatisierung von Beginn an priorisieren.