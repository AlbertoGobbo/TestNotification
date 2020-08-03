<div align="center"> 
<img src="Images/_icon.png" alt="Immagine dell'icona"/>

# DESCRIZIONE DEL PROTOTIPO SOFTWARE SVILUPPATO
</div>

TODO

## Apple

https://docs.microsoft.com/it-it/azure/notification-hubs/xamarin-notification-hubs-ios-push-notification-apns-get-started#generate-the-certificate-signing-request-file

## Mobile app

Casi d'uso limite:

- app disinstallata --> Infine, quando FCM tenta di recapitare un messaggio al dispositivo e l'app � stata disinstallata, FCM elimina immediatamente quel messaggio e annulla il token di registrazione.
I tentativi futuri di inviare un messaggio a quel dispositivo generano un NotRegisterederrore.
 
---

## Come inviare una notifica

Lo stagista ha creato una web application che permette di inviare una notifica da un'interfaccia web semplice, presente nel progetto *TestNotificationWebApp*.

In alternativa si pu� utilizzare [Postman](https://www.postman.com/), un programma adatto per inoltrare richieste a determinati endpoint indipendentemente dalla posizione del server.

La prima cosa da fare � creare una nuova richiesta, rispettando i seguenti passaggi:
- nella barra in alto, selezionare il metodo **POST** e inserire l'URL relativo all'inoltro della notifica https://serverpushnotification.azurewebsites.net/api/notifications/requests (oppure avviare il back end locale all'indirizzo http://localhost:5000/api/notifications/requests).
- selezionare la voce **Headers** presente nel tab, e:
    - Attivare la checkbox della key **Content-Type** e scrivere come value **application/json**.
- selezionare la voce **Body** presente nel tab, attivare il radio button **raw** e selezionare **JSON** nel menu a tendina. Nella text box sottostante, scrivere il corpo del messaggio di notifica:
```
{
    "text": "Hey man! How are things?"
    "tags": [
        "guid_user",
    ]
}
```
1) Il parametro **text** indica il testo che verr� inserito nella notifica. Il titolo � fissato di default come il nome dell'applicazione corrente.

2) Il parametro **tags** invece indica un insieme di valori che specificano quali device, registrati con i relativi tag, possono essere raggiunti. � possibile inserire 0 tag (che equivale a raggiungere tutti i dispositivi registrati in Azure Hub Notification),
e il numero massimo inseribile � di 10 tag. Questa limitazione � dovuta al fatto che la *tagExpression*, che viene costruita ed elaborata dal back end, � un'operazione logica di soli AND (&&). In merito a questa operazione, la documentazione � chiara, infatti utilizzando solo && 
� possibile inserire al massimo 10 tag.

> Nel caso d'uso specifico di Hunext, il tag da inserire � il GUID al quale si vuole inviare la notifica.
Va evidenziato che, dopo vari tentativi di test, non � possibile inserire pi� tag diversi all'interno del parametro **tags**.
La best practice da tenere � inviare una notifica per ogni utente, quindi N notifiche per N utenti.

- cliccare il bottone **Send** e inviare la notifica.

### Note a margine

#### HTTP Response 422

Come descritto nella issue [#8](https://github.com/HunextSoftware/TestNotification/issues/8), l'invio della notifica comporta una risposta HTTP 422. 

Questo errore semantico � dovuto al fatto che � stato configurato FCM (Google) ma non APN (Apple), infatti il codice back end � gi� predisposto per l'invio di notifiche a dispositivi Apple.

� stato testato che, rimuovendo il codice specifico di TestNotification.Backend in Model/PushTemplates e Services/NotificationHubsService, l'invio della notifica comporta una risposta HTTP 200, con avvenuta ricezione della notifica.

Ergo, questo problema verr� risolto non appena verr� ultimata la configurazione dell'APN, con il codice back end che deve rimanere intatto.

#### Testare le notifiche

Per testare l'invio delle notifiche, � possibile scaricare il file che si trova, a partire dalla radice di questo repository, in *Archive/PostmanTestNotificationRequests*.

> In alternativa, � possibile scaricare il suddetto file tramite questo [link](https://drive.google.com/file/d/1oDxEGqBFsqdU1l6WnUaa6LrwBHqt1HEP/view?usp=sharing).

Successivamente, aprire l'applicativo Postman, cliccare il bottone **Import** presente in alto sotto il menu e selezionare il file appena scaricato.

Il file contiene due richieste HTTP avente lo stesso body ed indirizzate al tag dell'utente *Mario.Rossi*. La prima richiesta � indirizzata all'URL del back end locale che dev'essere avviato manualmente, mentre la seconda all'URL dello stesso back end ma caricato su Azure ed accessibile pubblicamente.

**Pre-condizione**: almeno un device deve essere autenticato come *Mario.Rossi*.

**Post-condizione**: una volta che la richiesta HTTP viene inoltrata, tutti i device che sono autenticati come *Mario.Rossi* ricevono una notifica.

<div align="right">

[Torna su](#descrizione-del-prototipo-software-sviluppato)
</div>