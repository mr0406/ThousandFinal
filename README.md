# Dokumentacja CSS

Nie wszystkie klasy/identyfikatory mają swój styl, niektóre są nadane "na przyszłość" w razie gdyby były potrzebne, takie klasy będę zaznaczał pogrubieniem.

## Klasy oraz identyfikatory pliku Game.razor

### Identyfikatory
mandatory -> Okienko w lewym górnym rogu z nickiem oraz meldunkiem

resultTable -> Okienko na górze na środku z graczami oraz ich wynikiem.

tableContainer -> Kontener zawierający graczy po lewej, prawej, oraz stół

gameContainer -> Kontener zawierający wszystkie elementy gry, a więc nie zawiera czatu.

**rightPlayer, leftPlayer** -> Kontener gracza po prawej oraz lewej stronie.

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

## Klasy

cardsOnTable -> Są to karty położone na stole. Jest to klasa bo nie wiem do końca czy to jest generowane oddzielnie dla każdej karty czy nie i nie wiem jak zachowywałyby się 2 elementy o tym samym ID, więc zrobiłem klase.

**otherPlayerHand** -> Kontener zawierający karty, które w ręce trzyma inny gracz. Zrobiłem z tego klase bo może kiedys będzie trzeba zrobić oddzielny styl na lewego i prawego gracza.

**playerPoints** -> Znacznik <Span> zawierający punkty graczy w tabeli wyników resultTable.

smallImg -> Mały obrazek kolory karty w lewym górnym i prawym dolnym rogu karty w ręce gracza

no-drop, can-drop -> DO OPISANIA !!!!!

cardColorContainer -> Kontener w którym jest napis oraz mała ikonka koloru karty na kartach na ręce gracza.

smallCardColorImage -> Mały obrazek koloru karty w lewym górnym i prawym dolnym rogu karty na ręce gracza.

bigCardColorImage -> Obrazek koloru na środku karty w ręce gracza.

**cardColorText** -> Tekst nad/pod małym obrazkiem symbolizującym kolor karty na ręce gracza.
