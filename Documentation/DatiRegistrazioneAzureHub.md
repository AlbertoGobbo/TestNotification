# DATI SALVATI IN AZURE NOTIFICATION HUB DURANTE IL PROCESSO DI INSTALLAZIONE DEL DISPOSITIVO

I seguenti dati possono essere visualizzati in tempo reale aprendo Visual Studio 2019 e, dal menu, seguire il seguente percorso:
> View -> Server Explorer -> Azure (mailoutlook@outlook.com) -> Notification Hubs -> *NomeNotificationHub*

**Pre-condizione**: l'utente ha un account Azure e ha creato un Notification Hubs con il suo spazio di nomi.

**Post-condizione**: qualunque dispositivo pu� registrarsi al hub di notifica di Azure.

---

- **Platform**: identifica il tipo di piattaforma utilizzata nella registrazione mediante un'apposita sigla 
    - GCM => Google 
    - APN => Apple
    - WNS => Windows
    - MPNS => Windows Phone
    - ADM => Amazon
    - BCP => Baidu
- **Type**: indica il nome del modello salvato al momento della registrazione, ovvero il formato della notifica che si vuole inviare all'utente. Se � contrassegnato come *Native*, significa che <span style="text-decoration: underline;">non</span> � stato personalizzato e si utilizza il modello standard. 
- **Tags**: etichetta/e che identificano l'utente secondo una o pi� categorie di appartenenza. I tag non sono obbligatori, ma sono molto utili per indirizzare le notifiche a gruppi di utenti. Se non � presente alcun tag, le notifiche saranno indirizzate a tutti gli utenti registrati in Azure Hub.
- **PNS Identifier**: token che funge da identificativo per un specifico dispositivo, che viene recuperato nell'operazione di PNS handle, eseguito in prima istanza tra mobile app e la piattaforma di notifica specifica. Come descritto nella documentazione, non � garantita l'univocit�.
    - Per i dispositivi Android, l'identificativo � l'ID di registrazione di Firebase (*onNewToken()*).
    - Per i dispositivi Apple, l'identificativo � il token del dispositivo.
    - Per i dispositivi Windows, l'identificativo � un URI che identifica il canale. 
- **Registration ID**: ID generato automaticamente da Azure per identificare una singola registrazione, l'utente non ha la responsabilit� di doverlo generare.
- **Expiration date**: data di scadenza della registrazione in Azure. � impostata di default alla data 31/12/9999, in modo che la registrazione non possa mai scadere.

# DATI INSTALLAZIONE DEVICE

Il termine *installazione* indica una registrazione pi� avanzata ed al momento della scrittura � il metodo pi� recente ed avanzato. 

L'installazione comprende le seguenti operazioni:
- associare il PNS handle per un dispositivo con tag ed, eventualmente, dei modelli di registrazione. Il PNS handle pu� essere recuperato solo a livello client.
- creare e aggiornare un'installazione in modo idempotente, evitando registrazioni duplicate.
- il modello di installazione supporta un formato di tag speciale ( *$InstallationId:{INSTALLATION_ID}* ) che consente l'invio di una notifica direttamente al device specifico.
- eseguire aggiornamenti parziali delle registrazioni. 

---

I seguenti dati corrispondono alla classe DeviceInstallation che si trova in TestNotification.Backend/Models:

- **InstallationId**: identifica un device specifico associato all'applicazione dalla quale � partito il processo di installazione. Il suo utilizzo � legato soprattutto alla cancellazione della registrazione dal hub di notifica.
    - In Android 8.0 e superiori, il codice ```Secure.GetString(Application.Context.ContentResolver, Secure.AndroidId)``` consente di generare una stringa di 64 bit, espressa come stringa esadecimale, ottenuta dalla combinazione di: chiave firmata della mobile app, utente e dispositivo.
    - Per le restanti versioni di Android, lo stesso codice consente di generare un stringa random di 64 bit, espressa come stringa esadecimale, che rimane unica nel ciclo di vita del dispositivo dell'utente.
  
  > Il recupero di questa informazione pu� avvenire solo lato app.
- **Platform**: identifica la piattaforma nella quale il dispositivo si registra. 

  > Il recupero di questa informazione pu� avvenire solo lato app.
- **PushChannel**: � il token recuperato dal PNS handle che identifica il collegamento tra un dispositivo e la piattaforma di notifica apposita (vedere sopra *PNS Identifier*).
  Questo parametro � strettamente legato a *Platform*, tanto che ci sono modi diversi per recuperare il token in base al sistema operativo del dispositivo.

  > Il recupero di questa informazione avviene lato app, va ancora studiato un modo per capire se � possibile recuperarla tramite back end, in modo da salvaguardare la sicurezza dell'utente.  

- **Tags**: array di etichette che identificano una serie di categorie alla quale l'utente appartiene (es. se il contesto dell'applicazione � lo sport, l'utente che tifa X e segue anche la squadra Y ricever� le notifiche sia della squadra X che della squadra Y).
  Nel caso d'uso specifico dell'applicazione *Hunext Mobile*, il tag specifico da utilizzare � il GUID utente che viene recuperato dal layer di persistenza del server aziendale. In questo modo, una notifica pu� essere indirizzata a specifici utenti.
  Il numero di tag possibili va da 0 a 10.

  > Per questioni di sicurezza e di elaborazione, il recupero di questa informazione avviene lato back end. In questo modo, l'utente � svincolato da ogni responsabilit� e non ha libert� di scelta, libert� che � concessa solamente al back end.
