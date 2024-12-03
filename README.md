# 223-ma-app

Willkommen beim Ük 223. Dieses Repository enthält den Code für die Applikation, die wir im Modul 223 erstellen werden, sowie die Lerninhalte.

Es dient jedoch lediglich als Ablage dieser Informationen. Gewisse Informationen befinden sich auch im Learningview, ebenfalls wird wie gewohnt im Learningview die Dokumentation der Arbeit abgegeben, sowie der Progress dokumentiert.

## Last Tests

![1733232327432](image/README/1733232327432.png)

![1733232394706](image/README/1733232394706.png)

In den obenstehenden Abbildungen sind erfolgreiche Lasttests dargestellt, die sicherstellen, dass während des Tests keine Gelder verloren gehen und dass die Systemintegrität gewahrt bleibt. Der Test prüft, ob der Gesamtbetrag auf allen Konten (Ledgers) vor und nach den Testbuchungen gleich bleibt.

#### Ablauf des Tests:

1. **Login und Token-Abfrage:**
   - Zuerst wird ein Login durchgeführt, um ein **Bearer Token** zu erhalten, das für die Authentifizierung erforderlich ist, um auf geschützte Endpunkte zuzugreifen (Abbildung 1).
2. **Gesamtguthaben vor dem Test:**
   - Vor Beginn des Tests wird das gesamte Guthaben im System abgefragt, um eine Ausgangsbasis zu schaffen (Abbildung 1). Man sieht das gesamte Guthaben vor dem Test ist: **1000000235896.00**
3. **Testaufruf der API:**
   - Ein API-Aufruf an `/api/v1/lbankinfo` wird durchgeführt, um sicherzustellen, dass die Verbindung zum System funktioniert und die Authentifizierung korrekt ist (Abbildung 1).
4. **Durchführung des Lasttests mit NBomber:**
   - Der Lasttest wird mit **NBomber** durchgeführt. Zwei Szenarien werden simuliert:
     - **HTTP-Szenario:** Einfache API-Anfragen werden simuliert.
     - **Booking-Szenario:** Buchungen zwischen zwei Konten werden simuliert (Abbildung 2).
5. **Gesamtguthaben nach dem Test:**
   - Nach dem Test wird das Guthaben erneut abgefragt (Abbildung 2). Wie ersischtlich hat sich der Betrag nicht verändert und ist immer noch **1000000235896.00.**
6. **Überprüfung der Konsistenz:**
   - Es wird überprüft, ob das Gesamtguthaben vor und nach dem Test übereinstimmt. Wenn dies der Fall ist, bedeutet es, dass keine Gelder verloren gegangen sind (Abbildung 2).

#### Warum ist dieser Test wichtig?

- **Konsistenzprüfung:** Der Test stellt sicher, dass das Gesamtguthaben im System vor und nach dem Test gleich bleibt, auch wenn während des Tests Transaktionen durchgeführt werden.
- **Fehlererkennung:** So werden mögliche Fehlerquellen wie verlorenes Geld oder Inkonsistenzen im System durch Lasttests erkannt.

#### Fazit:

Dieser Test gewährleistet, dass das System unter Last korrekt funktioniert, ohne dass Geld verloren geht. Er hilft, die Stabilität und Integrität der Finanzoperationen zu bestätigen, auch bei hoher Belastung.
