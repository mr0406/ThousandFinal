# Dokumentacja CSS

Nie wszystkie klasy/identyfikatory mają swój styl, niektóre są nadane "na przyszłość" w razie gdyby były potrzebne, takie klasy będę zaznaczał pogrubieniem.

## Kolory
Kolory są zapisane na początku pliku app.css w zmiennych, dzięki czemu nie trzeba za każdym razem wartości rgb, a wystarczy nazwę zmiennej.
Kolory są zaczerpnięte z palety: https://coolors.co/00a878-d8f1a0-fdf7ec-fe5234-0b0500
Dostępne kolory: --green, --lghtgreen, --white, --red, --black
Zastosowanie koloru, przykład: background-color: var(--white);


## Klasy oraz identyfikatory pliku Game.razor

### Identyfikatory
mandatory -> Okienko w lewym górnym rogu z nickiem oraz meldunkiem

resultTable -> Okienko na górze na środku z graczami oraz ich wynikiem.

tableContainer -> Kontener zawierający graczy po lewej, prawej, oraz stół

gameContainer -> Kontener zawierający wszystkie elementy gry, a więc nie zawiera czatu.

rightPlayer, leftPlayer -> Gracze po prawej oraz lewej stronie.

table -> Stół z kartami.

playerCards -> Kontener kart gracza.
### Klasy

otherPlayer -> Klasa zawierająca gracza po lewej oraz po prawej

## Klasy oraz identyfikatory pliku Chat.razor

### Identyfikatory

chatContainer -> Kontener zawierający cały czat.

**usersHeader** -> Napis "Users:"  

**messagesHeader** -> Napis "Messages:"

messages -> Okno z wiadomościami.

serverMessage -> Wiadomość wysłana przez serwer.

myMessage -> Wiadomość wysłana przez gracza.

othersMessage -> Wiadomość wysłana przez innych graczy   // NAZWA DO ZMIANY???

messageForm -> Pole do wpisywania wiadomości na czat.

### Klasy

**playerList** -> Klasa zawierająca liste graczy.

message -> Klasa wiadomości gracza, innych graczy oraz serwera.

## Klasy oraz identyfikatory pliku Login.razor

Może kiedyś coś tu będzie.


## Pozostałe klasy oraz identyfikatory

### Identyfikatory

gameFrame -> Kontener zawierający gre oraz czat.

currentMandatory -> Obecny meldunek, ikonka w oknie "mandatory".

cardContainer -> Kontener pojedyńczej karty w ręce gracza.

cardsOnTable -> Stół. Kontener kart położonych na stole.

raisingPoints -> Kontener z przyciskami do aukcji.

giveupButton -> Przycisk do pasowania w trakcie aukcji i po aukcji.

## Klasy

**otherPlayerHand** -> Kontener zawierający karty, które w ręce trzyma inny gracz. Zrobiłem z tego klase bo może kiedys będzie trzeba zrobić oddzielny styl na lewego i prawego gracza.

**playerPoints** -> Znacznik <Span> zawierający punkty graczy w tabeli wyników resultTable.

smallImg -> Mały obrazek kolory karty w lewym górnym i prawym dolnym rogu karty w ręce gracza

no-drop, can-drop -> DO OPISANIA !!!!!

cardColorContainer -> Kontener w którym jest napis oraz mała ikonka koloru karty na kartach na ręce gracza.

smallCardColorImage -> Mały obrazek koloru karty w lewym górnym i prawym dolnym rogu karty na ręce gracza.

bigCardColorImage -> Obrazek koloru na środku karty w ręce gracza.

**cardColorText** -> Tekst nad/pod małym obrazkiem symbolizującym kolor karty na ręce gracza.

raiseButton -> Klasa obejmująca przyciski z liczbami podczas aukcji.

# PLANY

Na każdej stronie pasek na dole, nie duży, rok, autorzy, coś takiego chuj wie wymyśli sie.
Guziki - polewej stronie ikonka, po prawej tekst.

## Poczekalnia

Poczekalnia dzieli się na 3 kolumny
Po lewej stronie na górze miejsce na dodanie pokoju
Po lewej poniżej jest opis o nas
Jeszcze niżej są zasady gry i tym podobne pierdoły

Po prawej są pokoje, 4 pokoje w rzędzie, 4 rzędy.
Pokoje to karty. W kartach jest nazwa pokoju, liczba graczy, guzik dołącz, (ZAOKRĄGLONY JAK WSZYSTKO MARCIN GRATULUJE)
Po kliknięciu guzika odpala się okno na środku, tył się przyciemnia (50%?) i pojawia sie okienko na wpisanie nicku.

![okno loginu](https://i.imgur.com/Len9kD0.png)

## Rozgrywka

Po lewej na górze jest przycisk do wyjścia, w miejscu obecnego meldunku.
W miejscu tablicy wyników będzie meldunek.
Dodajemy rewersy do kart.
Tworzymy karty innych graczy.
W miescu listy graczy będą panele troche jak z kurnika, z wynikiem, nickiem i co tam sie wymysli.
Usuwamy napis "messages".
