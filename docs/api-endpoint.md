# API-Endpoints

## 1

* Post: `/restaurants`

  (Registriert ein neues Restaurant und liefert API-Key retour)

## 2

## 3

* Put: `/restaurants/{id}/delivery-conditions`

  (erstellen und updaten der lieferbedingungen)

## 4

* Put: `/restaurants/{id}/menu`

  (erstellen / updaten einer Speisekarte, Api erforderlich)

## 5

* Get: `/restaurants`

  (query params für geöffnete Restaurants, Sortierung GPS, anzahl der restaurants(?) )

## 6

* Get: `/restaurants/{id}/menu`

  (abfragen der Speisekarte)

## 7

* Post: `/restaurants/{id}/orders/price`

  (im Body dann die ver. MenuItems, Adresse)

## 8

* Post: `/restaurants/{id}/orders`

  (erstellen einer bestellung retourgeben von Code, damit Status abgefragt werden kann)

## 9

externe API

## 10

* PATCH: `orders/{id}`

  (trotzdem API-Key abfragen, Status im http-body)

## 11

* Get: `orders/{id}/status-change/{token}`

  (API key erforderlich)

## 12

* Get: `/orders/{orderCode}/status`
