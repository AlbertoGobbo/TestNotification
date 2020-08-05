<div align="center"> 
<img src="Images/_icon.png" alt="Immagine dell'icona"/>

# DESCRIZIONE DEL PROTOTIPO SOFTWARE SVILUPPATO
</div>

L'obiettivo richiesto da parte dell'azienda � implementare un prototipo *ex novo* (chiamato *TestNotification*) per l'implementazione del sistema di notifiche push per l'applicazione mobile *Hunext Mobile*.

Prima ancora dello sviluppo del prototipo, � stato necessario configurare le piattaforme di notifica apposite. Come anticipato nel documento *1_Descrizione-generale-progetto*, non � stata
considerata la configurazione della piattaforma Apple (APNS) e tanto meno lo sviluppo specifico in *TestNotification.iOS*, ma sar� fornita la dovuta documentazione per gli sviluppatori che si faranno 
carico di integrare il prototipo nelle applicazioni aziendali.

Il prototipo comprende esattamente tre componenti, delle quali due fondamentali ai fini della realizzazione del progetto:
- un backend scritto in ASP.NET Core, con l'obiettivo di gestire le installazioni dei dispositivi e le richieste di notifica, obbligatoria ai fini del progetto.
- un'applicazione mobile scritta in Xamarin.Forms, con l'obiettivo di ricevere notifiche mirate, obbligatoria ai fini del progetto.
- una web application scritta in ASP.NET Core Razor Pages, con l'obiettivo di inviare notifiche personalizzate. Non obbligatoria ai fini del progetto, ma molto utile in quanto esegue una specifica parte 
del lavoro che dovrebbe essere in gestione del backend.

La posizione delle tre componenti nell'elenco segue l'ordine cronologico di sviluppo. Da evidenziare che backend e applicazione mobile sono le uniche componenti che hanno avuto uno sviluppo parallelo, 
rappresentando quasi il 90% del tempo di sviluppo del prototipo. La web application � stata sviluppata dopo la conclusione di queste due componenti e ha rappresentato il restante 10%.

Il documento � strutturato nelle seguenti sezioni:
- [Configurazione di Firebase ed Azure Notification Hubs](#configurazione-di-firebase-ed-azure-notification-hubs)
- [Sviluppo backend](#sviluppo-backend)
- [Sviluppo applicazione mobile](#sviluppo-applicazione-mobile)
- [Sviluppo web application](#sviluppo-web-application)

---

## Configurazione di Firebase ed Azure Notification Hubs

Prima di iniziare, � necessario accertarsi di avere un account Google e una sottoscrizione in [Azure](https://portal.azure.com/). Una volta effettuati gli accertamenti del caso, il primo passo � inizializzare la configurazione di Firebase Cloud Messaging (FCM). 

Ecco la sequenza di azioni da effettuare:
1) accedere alla [console di Firebase](https://firebase.google.com/console/), cliccare su *Aggiungi progetto* e inserire il nome del progetto che deve essere univoco (in questo caso � *TestPushNotification*).
2) una volta effettuata la creazione del progetto, selezionare *Aggiungi Firebase alla tua app Android* e si aprir� una pagina con un form da compilare.
    - in *Nome pacchetto Android* inserire un nome per il pacchetto (in questo caso � *com.hunext.testnotification*) e clicca il bottone *Registra app*.
    - cliccare il bottone *Scarica google-services.json.* e copiarlo in locale: successivamente, verr� copiato in *TestNotification.Android*. Cliccare il bottone *Avanti*.
    - infine cliccare il bottone *Vai alla console*.
3) sulla tendina a sinistra, andare sulle impostazioni di *Panoramica del progetto* e selezionare la voce *Impostazioni progetto*.
    - passare alla scheda *Cloud Messaging* e cercare in *Credenziali di progetto* il valore di *Chiave server*. A questo punto, copiare il token e salvarlo in un file per utilizzarlo successivamente.

<div align="center">
<img src="Images/4_Document/Configuration/4.1)Create-Firebase-project.png" alt="Creazione progetto Firebase"/>
</div>

Una volta configurato le varie piattaforme, il passo successivo � creare un hub di notifica in Azure. 

Ecco la sequenza di azioni da effettuare:
1) accedere ad [Azure](https://portal.azure.com/).
2) dalla pagina principale, sotto la voce *Servizi di Azure* selezionare la voce *Crea una risorsa*. 
    - una volta che la pagina � aperta, digitare sulla barra di ricerca *Notification Hub*, selezionarlo e cliccare sul pulsante *Crea*.
3) inserire tutti i campi richiesti per creare correttamente il nuovo hub di notifica.
    - *Sottoscrizione*: scegliere la sottoscrizione di destinazione dall'elenco a discesa.
    - *Gruppo di risorse*: creare un nuovo gruppo di risorse o selezionarne uno esistente.
    - *Spazio dei nomi di Hub di notifica*: inserire un nome univoco globale (__nuovo__) per lo spazio dei nomi, o *namespace*, di *Hub di notifica*.
    - *Hub di notifica*: inserire un nome per il nuovo hub di notifica.
    - *Localit�*: scegliere la zona desiderabile per localizzare il server dall'elenco a discesa.
    - *Piano tariffario*: scegliere il tipo di piano tra gratuito, basic e standard. In questo progetto � stato selezionata la voce *gratuito*.
4) confermare la creazione della risorsa.
5) dalla pagina principale, selezionare l'hub di notifica appena creato.
    - selezionare dal menu della risorsa la voce *Access Policies*.
        - salvare in un file le due *Connection String* inerenti alle *Policy Name*: *DefaultListenSharedAccessSignature* e *DefaultFullSharedAccessSignature*.
6) configurare l'hub di notifica con le informazioni di FCM.
    - selezionare dal menu della risorsa la voce *Google (GCM/FCM)*.
    - inserire la *Chiave server* che � stata copiata nell'ultimo passo nella console di Firebase.
    - cliccare il pulsante *Save*.

<div align="center">
<img src="Images/4_Document/Configuration/4.2)Create-notification-hub.png" alt="Creazione hub di notifica in Azure"/>
</div>
<p></p>

> Per configurare Apple Push Notification Services (APNS) e l'hub di notifica in Azure con le informazioni di APNS, visitare la seguente [sezione](https://docs.microsoft.com/it-it/azure/developer/mobile-apps/notification-hubs-backend-service-xamarin-forms#set-up-push-notification-services-and-azure-notification-hub).

A questo punto, l'hub di notifica e i PNS sono configurati correttamente e possono svolgere la loro funzione.
Il prossimo passo � lo sviluppo del codice, a partire dal backend.

<div align="right">

[Torna su](#descrizione-del-prototipo-software-sviluppato)
</div>

---

## Sviluppo backend

Il progetto *TestNotification.Backend* � stato sviluppato a partire da un progetto ASP.NET Core Web API, con il framework .NET Core 3.1, supportando il design pattern della
*Dependency Injection (DI)*.

Il backend � una parte fondamentale di questo progetto in quanto gestisce le chiamate per la gestione delle installazioni dei dispositivi e le richieste di
invio notifiche destinate all'hub di notifica di Azure, che a sua volta gestir� autonomamente l'invio delle notifiche ai PNS di riferimento.

Il primo passaggio essenziale per lo sviluppatore � inserire valori di configurazione localmente utilizzando lo strumento *Secret Manager*. 
Aprire il terminale ed inserire i seguenti comandi:

```
dotnet user-secrets init
dotnet user-secrets set "NotificationHub:Name" <value>
dotnet user-secrets set "NotificationHub:ConnectionString" <value>
```

Il placeholder *\<value\>* va rimpiazzato in questo modo:
- **NotificationHub:Name** � la voce *Name** che si trova in *Informazioni di base* nella pagina principale dell'hub di notifica appena creato.
- **NotificationHub:ConnectionString** � il valore *DefaultFullSharedAccessSignature* copiato nel passaggio 5) della creazione dell'hub di notifica.

Il passaggio successivo � aggiungere le dipendenze necessarie al backend dal servizio *NuGet* di Visual Studio:
- *Microsoft.Azure.NotificationHubs*, la libreria che permette di effettuare specifiche chiamate ad Azure Notification Hubs.
- *LiteDB*, la libreria che permette di creare e gestire un database locale NoSQL. 

Da questo momento, � importante analizzare le tappe cruciali per la realizzazione del backend.
Il progetto � strutturato in questa sequenza:
- *Properties*, che � la cartella che contiene tutti i parametri per avviare il backend e i file contenenti un riferimento con lo strumento *Secret Manager*.
- *Controllers*, che contiene le classi responsabili della logica di controllo, in particolare
    - in *LoginController* viene gestito un unico endpoint  per l'accesso autenticato degli utenti dall'applicazione mobile.
    - in *NotificationsController* vengono gestiti gli endpoint per l'installazione dei dispositivi, la cancellazione della medesima e l'invio delle notifiche.
- *Models*, che contiene tutte le classi con la logica di business e di convalida.
- *Services*, che contiene tutti i servizi che vengono richiamati dal controller, responsabili delle chiamate alle librerie di progetto.

Prima di spiegare le parti pi� importanti e significative del codice, va fatto un piccolo appunto sul layer di persistenza del backend.
L'utilizzo di LiteDB � fondamentale per simulare le dinamiche del backend aziendale, dove molte informazioni vengono carpite dal database aziendale.
Nel caso di questo progetto il database non ha bisogno di implementazione (essendo NoSQL) ed � talmente leggero che non va a gravare sulle prestazioni del backend.
Viene utilizzato prevalentemente per due motivi:
- per l'autenticazione degli utenti dall'applicazione mobile nella procedura di login.
- una volta che l'utente � autorizzato, si vogliono ottenere dati utili per l'installazione del dispositivo e per la visualizzazione di dati utente nella parte autorizzata 
dell'applicazione mobile.


Ora che � stata fatta chiarezza sull'utilizzo del database, si pu� passare l'attenzione alle classi pi� importanti del backend.

**1) DeviceInstallation.cs**

Questa classe contiene i dati, tutti obbligatori, che sono gestiti e devono essere spediti dall'applicazione mobile dopo il recupero del PNS handle:
- **InstallationId**.
- **Platform**.
- **PushChannel**.

> Per capire il significato di questi dati, andare a vedere la sezione *Approfondimento sul processo di installazione del dispositivo* nel documento *3_Descrizione-funzionamento-piattaforme-notifiche-push*.

Un altro dato che servir� nel processo di installazione del dispositivo � **Tags**, un vettore nel quale verr� salvato il GUID dell'utente che ha effettuato il login, in modo da 
poter indirizzare le richieste ad un utente specifico come richiesto dal tutor aziendale. Lo stesso dato viene utilizzato per essere inserito nell'header delle richieste HTTP(S) 
per certificare che le richieste effettuate dall'applicazione mobile siano autenticate.

> **Tags** verr� inserito nella classe *NotificationsController* all'endpoint PUT *~/installations*, dopo aver fatto la verifica che la chiamata sia autorizzata.

**2) PushTemplates.cs**

Questa classe contiene i payload di notifica nel formato stringa (pronto per essere serializzato in formato JSON) sia per dispositivi Android che per iOS. 
All'interno sono presenti due classi interne:
- *Generic*, che rappresenta la notifica classica che viene ricevuta da tutti i dispositivi. � l'unica classe utilizzata in questo prototipo.
- *Silent*, che rappresenta una notifica silenziosa, ovvero non invasiva a livello visivo in quanto presente solo nel caso in cui l'utente scrolli la tendina delle notifiche. Solitamente � utilizzata per
segnalare servizi attivi in background, come per esempio la navigazione su Google Maps. Non � utile ai fini del progetto, ma se nel futuro l'azienda volesse utilizzare questo tipo di notifica
l'implementazione sarebbe quasi immediata.

*Payload generico Android:*
```
{ 
    "notification": 
    { 
        "title" : "TestNotification", 
        "body" : "$(alertMessage)"
    }
}"
```

*Payload generico iOS:*
```
{ 
    "aps" : 
    {
        "alert" : "$(alertMessage)"
    } 
}
```

Il placeholder *$(alertMessage)* verr� sostituito in *NotificatioHubsService.cs* con il metodo seguente:
```
string PrepareNotificationPayload(string template, string text) => template
    .Replace("$(alertMessage)", text, StringComparison.InvariantCulture); 
```

Utilizzando questo payload non viene registrato alcun modello di notifica, in quanto quello utilizzato � un modello che, nel gergo tecnico, viene chiamato *nativo*.
Per l'installazione del dispositivo � comunque prevista la registrazione di modelli personalizzati, inserendo ulteriori chiavi come analizzato nel documento *3_Descrizione-funzionamento-piattaforme-notifiche-push*.

**3) NotificationRequest.cs**

Questa classe contiene i dati che vengono passati all'invio della notifica (nel caso di questo progetto dalla web application) e rimpiazzano i placeholder nei payload di 
notifica. Nello specifico *Text* va a sostituire $(alertMessage), mentre *Tags* viene elaborato e poi inserito come parametro nella chiamata all'API di Azure Notification Hubs 
che si occupa nello specifico di inviare notifiche con una specifica *tagExpression*.

> La classe *NotificationRequestTemplate.cs* non � stata implementata, ma sar� pronta all'uso nel momento in cui gli sviluppatori implementeranno la logica di invio delle notifiche dal backend anzich� 
dalla web application.
 
**4) NotificationsController.cs**

Qui risiede il fulcro della Web API RESTful: infatti questa classe funge un ruolo fondamentale per la comunicazione e la gestione delle richieste tra i dispositivi e il backend stesso.
Grazie all'inserimento in cima alla classe dell'attributo *[ApiController]*, viene abilitato il routing delle richieste a specifici endpoint e le risposte HTTP sono automatiche.

Gli endpoint che possono essere accettati dal backend sono i seguenti:
- GET *~/api/notifications/users/all*: ritorna una risposta HTTP 200 con annesso oggetto che contiene tutti gli id utente con annesso username. Necessario per creare dinamicamente la tendina selezionabile della web 
application per l'invio di notifiche mirate ad utenti precisi.
- PUT *~/api/notifications/installations*: ritorna una risposta HTTP 200 con i tag del dispositivo se la creazione dell'installazione di un dispositivo o il suo conseguente aggiornamento avviene con successo.
- DELETE *~/api/notifications/installations/\{installationId}*: ritorna una risposta HTTP 200 se la cancellazione dell'installazione del dispositivo specificato dal parametro *installationId* avviene con 
successo. 
- POST *~/api/notifications/requests*: ritorna una risposta HTTP 200 se la richiesta di invio della notifica � andata a buon fine.

**5) NotificationHubsService.cs**

In questa classe sono presenti i metodi che sono stati richiamati dagli endpoint di *NotificationsController*. In particolare la logica � incentrata sulle operazioni da effettuare con Azure, infatti �
alto l'utilizzo dell'API di Azure Notification Hubs.

I metodi principali sono i seguenti:
- *CreateOrUpdateInstallationAsync(DeviceInstallation deviceInstallation, CancellationToken token, string[] tags)*: chiamato dal codice dell'endpoint PUT *~/api/notifications/installations*, viene prima 
creato l'oggetto *Installation* che contiene tutti i dati di *DeviceInstallation* e il vettore stringa *tags*, ed infine effettua una chiamata all'API con 
*_hub.CreateOrUpdateInstallationAsync(installation, token)* per creare oppure aggiornare l'installazione del dispositivo. Se la chiamata va a buon fine, il metodo ritorna il tag salvato nel processo di 
installazione (ovvero l'id utente) che servir� successivamente all'applicazione mobile per essere aggiunto nell'header di specifiche richieste HTTP ed essere gestito come "token" per l'autorizzazione.
- *DeleteInstallationByIdAsync(string installationId, CancellationToken token)*: chiamato dal codice dell'endpoint DELETE *~/api/notifications/installations/\{installationId}*, procede alla cancellazione di 
una specifica installazione (il parametro *installationId*) effettuando una chiamata all'API con * _hub.DeleteInstallationAsync(installationId, token)*. Ritorna true se la cancellazione � andata a buon fine.
- *RequestNotificationAsync(NotificationRequest notificationRequest, CancellationToken token)*: chiamato dal codice dell'endpoint POST *~/api/notifications/requests*, viene prima elaborato  l'oggetto 
*NotificationRequest* per preparare il payload di notifica, sia per Android che per iOS, e l'espressione logica di tag (*tagExpression*) per indirizzare la notifica a categorie di utenti specifici (in questo caso all'id
di uno specifico utente). 
    - Se non sono presenti tag viene richiamato il metodo *SendPlatformNotificationsAsync(androidPayload, iOSPayload, token)* che inoltra la notifica in modalit� broadcast, 
    - altrimenti viene fissato un limite massimo di 10 tag (perch� l'espressione logica contiene solo operatori logici AND (&&)) e viene richiamato il metodo 
    *SendPlatformNotificationsAsync(androidPayload, iOSPayload, tagExpression.ToString(), token)* per inviare notifiche mirate grazie all'inserimento del parametro *tagExpression()*.
    
    I seguenti metodi sono responsabili delle chiamate all'API come si pu� vedere dal seguente codice:
    ```
    Task SendPlatformNotificationsAsync(string androidPayload, string iOSPayload, CancellationToken token)
    {
        var sendTasks = new Task[]
        {
            _hub.SendFcmNativeNotificationAsync(androidPayload, token),
            _hub.SendAppleNativeNotificationAsync(iOSPayload, token)
        };

        return Task.WhenAll(sendTasks);
    }

    Task SendPlatformNotificationsAsync(string androidPayload, string iOSPayload, string tagExpression, CancellationToken token)
    {
        var sendTasks = new Task[]
        {
            _hub.SendFcmNativeNotificationAsync(androidPayload, tagExpression, token),
            _hub.SendAppleNativeNotificationAsync(iOSPayload, tagExpression, token)
        };

        return Task.WhenAll(sendTasks);
    }
    ```

    Il vantaggio che si trae da questi metodi � la gestione contemporanea dell'invio delle notifiche con un modello di notifiche nativo a pi� piattaforme, in questo caso specifico ad FCM e APNS.

**6)LoginController.cs**

Come *NotificationsController*, questa classe � abilitata a ricevere richieste da determinati endpoint grazie alla presenza dell'attributo *[ApiController]*.
Questa classe � stata creata appositamente per separare le operazioni che coinvolgono le chiamate all'API di Azure Notifications Hubs dalle operazioni che interessano l'autenticazione dell'utente.

In questo caso l'endpoint presente � solo uno:
- POST *~/login*: ritorna una risposta HTTP 200 con l'annesso oggetto di risposta *LoginResponse* se le credenziali contenute nel corpo della richiesta sono presenti all'interno del database di LiteDB.

**7) UserManagerService.cs**

In questa classe sono presenti metodi che, in base al loro scopo, vengono utilizzati sia da *LoginController* che da *NotificationsController*. In particolare vengono gestite tutte le operazioni con 
LiteDB.

I metodi principali sono i seguenti:
- *GetAllUsers()*: ritorna la lista di tutti gli utenti, in particolare gli id associati agli username. Viene utilizzato nel codice dell'endpoint GET *~/api/notifications/users/all*
- *GetUserByUsername(string username)*: ritorna un utente in base allo username passato come parametro del metodo. Viene utilizzato nel codice dell'endpoint POST *~/login*.
- *GetUserById(Guid id)*: ritorna un utente in base all'id passato come parametro del metodo. Viene utilizzato nel codice degli endpoint PUT *~/api/notifications/installations* e 
DELETE *~/api/notifications/installations/\{installationId}* per accertarsi che la richiesta HTTP sia autorizzata, verificando che nell'header sia presente l'id utente.
- *AuthenticateUser(string username, string password)*: ritorna true se le credenziali dell'utente sono presenti nel database. Viene utilizzato nel codice dell'endpoint POST *~/login*.

### Attivit� facoltative

#### Creazione dell'App Service su Azure

In questo progetto � risultato necessario creare un'API Application nel servizio di Azure *App Service* per ospitare il backend e averlo a disposizione in rete in qualsiasi momento.
Ovviamente ai fini dell'azienda questo passaggio � inutile in quanto il backend aziendale � gi� ospitato in altri servizi di hosting.

Nonostante questa premessa, si vuole mostrare quali misure ha adottato lo stagista per sopperire a questo problema.
Di seguito vengono elencati in sequenza i passaggi da effettuare per attivare questo servizio:
1) Se si � gi� registrati, accedere ad [Azure](https://portal.azure.com/).
2) Dalla pagina principale, sotto la voce *Servizi di Azure* selezionare la voce *Crea una risorsa*.
    - nella barra di ricerca digitare *API App*, selezionare il risultato ottenuto e cliccare il bottone *Crea*.
3) Inserire i dati nei rispettivi campi:
    - **Nome app**: inserire un nome univo che identifica l'API App.
    - **Sottoscrizione**: selezionare la stessa sottoscrizione scelta durante la creazione dell'hub di notifica.
    - **Gruppo di risorse**: selezionare lo stesso gruppo di risorse scelta durante la creazione dell'hub di notifica.
    - **Localit�**: scegliere la zona desiderabile per localizzare il server dall'elenco a discesa.
    - **Application Insights:**: lasciare le opzioni suggerite da Azure.
4) Una volta inserito i dati del punto 3), confermare la creazione del servizio ed entrare all'interno della risorsa.
5) Salvare in un file a parte l'URL *https://<app_name>.azurewebsites.net* che si trova in *Informazioni di base*, in quanto verr� utilizzato nell'applicazione mobile.
6) Selezionare dal menu della risorsa la voce *Configurazione*.
    - cliccare sul pulsante *Nuova impostazione applicazione* ed inserire le seguenti chiavi con gli annessi valori (salvati in precedenza):
        - *NotificationHub:Name*.
        - *NotificationHub:ConnectionString*.
    - cliccare sul bottone *Salva* e poi su *Continua*.

#### Pubblicare il backend

Questa attivit� � strettamente legata alla creazione dell'App Service su Azure descritta nel paragrafo precedente, e viene svolta dall'ambiente di sviluppo Visual Studio 2019.

Per distribuire il backend e renderlo pubblico su Azure, seguire in sequenza i seguenti passaggi:
1) cambiare la configurazione del progetto da *Debug* a *Release*.
2) fare click destro sul progetto *TestNotificationBackend*.
3) Selezionare la voce *Publish*,
    - selezionare *Azure* come target.
    - selezionare *Azure App Service (Windows)* come target specifico.
    - connettersi all'account di Azure se non � gi� stato fatto.
    - selezionare il gruppo di risorse alla quale appartiene l'App Service creata precedentemente.
    - infine selezionare il pulsante *Finish*.

#### Note a margine

L'utilizzo del backend pu� essere sia locale che remoto. Lo stagista ha preferito creare un App Service in modo da poter testare in una modo pi� fedele alla realt� il funzionamento dell'applicazione 
mobile e dell'intera infrastruttura. D'altro canto Azure non offre strumenti di debug del backend caricato, quindi l'utilizzo del backend in locale si � rivelato molto utile per poter fare debugging 
in profondit� per rilevare e risolvere i problemi del codice backend.

Di default il backend funziona solo su Azure. Nel caso lo sviluppatore sia interessato a farlo funzionare in locale, cambiare le configurazioni nel file *Program.cs* e cambiare l'indirizzo del server 
nell'applicazione mobile.

<div align="right">

[Torna su](#descrizione-del-prototipo-software-sviluppato)
</div>

---

## Sviluppo applicazione mobile



<div align="right">

[Torna su](#descrizione-del-prototipo-software-sviluppato)
</div>

---

## Sviluppo web application

TODO



### Come inviare una notifica con Postman

Lo stagista ha creato una web application che permette di inviare una notifica da un'interfaccia web semplice, presente nel progetto *TestNotificationWebApp*.

Prima di creare la web application si � cimentato nell'utilizzo di [Postman](https://www.postman.com/), un programma adatto per inoltrare richieste a determinati endpoint indipendentemente dalla posizione del server.

In seguito vengono illustrate le due modalit� per l'utilizzo di Postman.

**PRIMA MODALIT�**

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

**SECONDA MODALIT�**

Nel caso non si volesse preparare manualmente la richiesta, � possibile scaricare il file con alcune richieste di default che si trova, a partire dalla radice di questo repository, in *Archive/PostmanTestNotificationRequests*.

> In alternativa, � possibile scaricare il suddetto file tramite questo [link](https://drive.google.com/file/d/1oDxEGqBFsqdU1l6WnUaa6LrwBHqt1HEP/view?usp=sharing).

Successivamente, aprire l'applicativo Postman, cliccare il bottone **Import** presente in alto sotto il menu e selezionare il file appena scaricato.

Il file contiene due richieste HTTP avente lo stesso body ed indirizzate al tag dell'utente *Mario.Rossi*. La prima richiesta � indirizzata all'URL del back end locale che dev'essere avviato manualmente, mentre la seconda all'URL dello stesso back end ma caricato su Azure ed accessibile pubblicamente.

**Pre-condizione**: almeno un device deve essere autenticato come *Mario.Rossi*.

**Post-condizione**: una volta che la richiesta HTTP viene inoltrata, tutti i device che sono autenticati come *Mario.Rossi* ricevono una notifica.

#### Errore HTTP 422

Come descritto nella issue [#8](https://github.com/HunextSoftware/TestNotification/issues/8), l'invio della notifica comporta una risposta HTTP 422 (lo si pu� sia visualizzare da Postman che dalla console della web application). 

Questo errore semantico � dovuto al fatto che � stato configurata la piattaforma FCM ma non APNS, infatti il codice backend � gi� predisposto per l'invio di notifiche a dispositivi Apple.

� stato testato che, rimuovendo il codice specifico di TestNotification.Backend in Model/PushTemplates e Services/NotificationHubsService, l'invio della notifica comporta una risposta HTTP 200, con avvenuta ricezione della notifica.

Ergo, questo problema verr� risolto non appena verr� ultimata la configurazione dell'APN, con il codice backend che deve rimanere intatto.

<div align="right">

[Torna su](#descrizione-del-prototipo-software-sviluppato)
</div>

---
























## Apple

https://docs.microsoft.com/it-it/azure/notification-hubs/xamarin-notification-hubs-ios-push-notification-apns-get-started#generate-the-certificate-signing-request-file

## Mobile app

Casi d'uso limite:

- app disinstallata --> Infine, quando FCM tenta di recapitare un messaggio al dispositivo e l'app � stata disinstallata, FCM elimina immediatamente quel messaggio e annulla il token di registrazione.
I tentativi futuri di inviare un messaggio a quel dispositivo generano un NotRegisterederrore.
 
---





<div align="right">

[Torna su](#descrizione-del-prototipo-software-sviluppato)
</div>

---

> Per maggiori informazioni, visitare il seguente [link](https://docs.microsoft.com/it-it/azure/developer/mobile-apps/notification-hubs-backend-service-xamarin-forms).