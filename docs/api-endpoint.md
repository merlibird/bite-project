# API-Endpoints

Basis-Pfad: `/api`
Auth (🔒): API-Key im Header `X-Api-Key`. Restaurants dürfen nur sich selbst ändern.

## Restaurants

### 1 · Restaurant registrieren

* `POST /api/restaurants` — `multipart/form-data`

  Registriert ein Restaurant (inkl. Adresse, Öffnungszeiten, Cover-Bild, Webhook-URL) und liefert `restaurantId` + API-Key retour.

### 2 · Restaurants suchen

* `GET /api/restaurants?latitude=&longitude=&openNow=&count=`

  Liefert Restaurants nach GPS sortiert (Distanz), optional nur geöffnete (`openNow`), Anzahl per `count` (1–100, Default 10).

### 3 · Lieferbedingungen setzen 🔒

* `PUT /api/restaurants/{id}/delivery-conditions`

  Upsert der Lieferzonen + Gebührenregeln (Body: Liste von Zonen mit `maxDistance`, `minOrderValue`, `feeRules`).

## Speisekarte

### 4 · Speisekarte updaten 🔒

* `PUT /api/restaurants/{id}/menu`

  Erstellt / aktualisiert Kategorien + Items (Upsert per `id`).

### 5 · Speisekarte abfragen

* `GET /api/restaurants/{id}/menu`

  Liefert die Speisekarte (Kategorien mit Items).

## Bestellungen

### 6 · Preis berechnen

* `POST /api/restaurants/{restaurantId}/orders/price`

  Body: Items + Lieferadresse. Liefert `subtotal`, `deliveryFee`, `totalPrice`.

### 7 · Bestellung aufgeben

* `POST /api/restaurants/{restaurantId}/orders`

  Body: Items + Lieferadresse. Liefert `orderCode` (für Status-Abfrage).

### 8 · Status abfragen

* `GET /api/orders/{orderCode}/status`

  Liefert aktuellen Status.

### 9 · Status ändern (Restaurant) 🔒

* `PATCH /api/orders/{orderCode}`

  Body: `{ "status": "..." }`. Statusübergänge sind validiert.

### 10 · Status per Token ändern 🔒

* `GET /api/orders/{orderCode}/status-change/{token}`

  Status-Wechsel über Magic-Link-Token (aus Webhook/Mail).

## Externe API (Webhook)

### 11 · Bestelleingang

Beim Aufgeben einer Bestellung wird die `webhookUrl` des Restaurants benachrichtigt (transaktionaler Outbox + Polling-Worker).

---

**Order-Status:** `RECEIVED` → `SENT_TO_RESTAURANT` → `IN_PREPARATION` → `OUT_FOR_DELIVERY` → `DELIVERED`; `CANCELLED` aus jedem Status außer `DELIVERED`/`CANCELLED`.
