# Klient pre chatovací server používajúci IPK24-CHAT protokol
## IPK - Projekt 1

## 1. Úvod
Cieľom tohto projektu bolo vytvorenie klienta k serveru chatovacej konzolovej aplikácie, ktorá na komunikáciu
používa protokol IPK24-CHAT. Naimplementovať bolo treba dve varianty - UDP a TCP. Obidve varianty mali svoje
špecializácie a problémy, ktoré tým vznikli.

## 2. Obsah
1. [Úvod](#1-úvod)
2. [Obsah](#2-obsah)
3. [Ako spustiť projekt](#3-ako-spustiť-projekt)
4. [Základná teória k projektu](#4-základná-teória-k-projektu)\
    4.1 [TCP](#41-tcp-transmission-control-protocol)\
    4.2 [UDP](#42-udp-user-datagram-protocol)\
    4.3 [Socket](#43-socket)
5. [Štruktúra projektu](#5-štruktúra-projektu)
6. [Testovanie](#6-testovanie) \
    6.1 [Unit testy](#61-unit-testy) \
    6.2 [Testovací scenár](#62-testovací-scenár) \
    6.3 [Testovanie aplikácie na referenčnom serveri](#63-testovanie-aplikácie-na-referenčnom-serveri)
7. [Bibliografia](#8-bibliografia)


## 3. Ako spustiť projekt
Projekt je možné zostaviť pomocou príkazu make v koreňovom adresári. Po zostavení binárneho súboru ipk24chat-client je ho možné spustiť s parametrami definovanými v zadani projektu. 
Pre viac informácií použite parameter `-h`.
Ďalšie ciele programu make:
- `make` - zostaví projekt
- `make help` - vypíše užitočné informácie o spustení projektu
- `make test` - spustí unit testy k projektu
- `make udp` - spustí klienta s predvolenými parametrami a protokolom UDP
- `make test` - spustí klienta s predvolenými parametrami a protokolom TCP

## 4. Základná teória k projektu

### 4.1 TCP (Transmission Control Protocol)
TCP poskytuje spoľahlivú službu doručovania dát v poradí akom boli odoslané v podobe prúdu bytov aplikáciám.
Správy sú prenášané cez sieť pomocou TCP segmentov, kde každý segment je odoslaný ako Internet Protocol datagram.
Spoľahlivé doručenie správ si berie väčšiu réžiu na čase prenosu a veľkosti prenosu dát. Používa sa
na prenášanie súborov, zasielanie emailov alebo ho tiež využíva SSH (Secure Shell).

### 4.2 UDP (User Datagram Protocol)
UDP je jednoduchý protokol pre aplikačné programy na odosielanie správ iným programom
s malými nárokmi na protokolové mechanizmy. Na rozdiel od TCP však nie je spoľahlivý a nie je zaručené
doručenie správ alebo ochrana pred duplikovanými správami. Používa sa v prípadoch, kedy je dôležitá latencia,
a nie spoľahlivosť doručenia všetkých dát - napríklad videohovory.

### 4.3 Socket
Sockety sa používajú na interakciu medzi klientom a serverom. V modeli klient-server čaká socket na serveri
na požiadavky od klienta. Server najprv vytvorí adresu, pomocou ktorej je možné server nájsť zo strany klienta.
Keď je adresa vytvorená, server čaká na požiadavku od klienta. Klient sa na server pripojí tiež pomocou socketu,
prebehne výmena dát, server obslúži požiadavku klienta a odošle odpoveď klientovi.

## 5. Štruktúra projektu
Projekt som sa snažil rozdeliť na podproblémy, ktoré som rozdelil do logických celkov alebo tried,
ktoré riešia daný podproblém. Rozdelenie programu a vzájomná spolupráca tried je zobrazená v diagrame tried:

![Chat App Class Diagram](/doc/ChatAppClassDiagram.png)
*(Diagram je dostupný v priečinku `doc/`)*

Vstupný bod programu je v súbore Program spustením metódy `Main()`. Najprv sa vytvorí inštancia triedy CommandLineOptions,
ktorá vo svojom konštruktore invokuje svoju metódu `ParseArguments()`. Tá priradí hodnoty jednotlivých argumentov do svojich
atribútov, alebo ukončí program s chybou. Získané nastavenia z argumentov sa potom predajú inštancii triedy `UserInputHandler`,
ktorá spracúva užívateľské vstupy a príkazy.

### `UserInputHandler`
Táto trieda si podľa zvoleného nastavenia varianty protokolu vytvorí inštanciu triedy `UdpClient` alebo `TcpClient`.
Obidve tieto triedy dedia z abstraktnej triedy `ClientBase`, ktorá deklaruje tri metódy:
- `SendMessageAsync()` - ktorá slúži na asynchrónne posielanie správ
- `ReceiveMessageAsync()` - na asynchrónne prijímanie správ
- `Close()` - uvoľní zdroje a ukončí komunikáciu.

Pri spracovaní užívateľského vstupu, trieda najprv zavolá asynchrónnu metódu `ReceiveMessageAsync()` a potom vo `while` cykle
číta štandardný vstup a spracúva ho. Ak sa vstup začína znakom `/`, pokúsi sa o spustenie príkazu, ak taký existuje. Ak nie,
vypíše užívateľovi varovanie. V inom prípade berie vstup ako obyčajnú správu. Ak je príslušný príkaz alebo poslanie správy neprijateľné
v aktuálnom stave klienta, oznámi to užívateľovi. V prípade, že niektoré správy potrebujú odpoveď alebo potvrdenie nejakej správy,
vstupy od užívateľa si ukladá do fronty a akonáhle príde čakána odpoveď, odošle všetky správy, ktoré čakali vo fronte, kým nenarazí
na správu, ktorá zase potrebuje potvrdenie od serveru. Trieda si tiež drží užívateľské meno, ktoré sa prípadne dá zmeniť príkazom `/rename`.

### `TcpClient`
Trieda slúži na spracovanie správ cez TCP protokol. Vytvorí si `reader`, `writer`, `network stream`, pripojí sa na *endpoint* a len
z tohto `streamu` číta alebo do neho zapisuje správy.

### `UdpClient`
Komunikácia musí fungovať na dynamických portoch, takže trieda si po prvej správe od serveru okrem potvrdenia `confirm` zapamätá port, odkiaľ prišla správa,
a na ten port už bude smerovať všetky svoje správy. Táto trieda pri každej prijatej (okrem správy `confirm`) správe pošle hneď správu confirm naspäť serveru.
Má tiež časovač, ktorý sa spustí pri odoslaní správy, a keď tento časovač vyprší, pokúsi sa danú správu poslať znovu. Maximálne spraví toľko pokusov, koľko je
definovaných cez parameter `MaxRetransmissions`.

### `ClientState`
Táto trieda stavruje konečný automat zo zadania. Pomocou prijatých správ od servera sa prepína do rôznych stavov.
Klient potom umožňuje niektoré akcie len v určitých stavoch.

### `Message`
Abstraktná trieda `Message` má potomkov, ktoré reprezentujú konkrétne typy správ. Má deklarované tri metódy, ktoré musia potom konkrétne podtriedy implementovať:
- `CraftTcp()` - slúži na vytvorenie daného typu správy v správnom formáte pre TCP variantu
- `CraftUdp()` - slúži na vytvorenie daného typu správy v správnom (byte) formáte pre UDP variantu
- `PrintOutput()` - niektoré správy pri prijatí u klienta sa majú zobraziť na výstupe u klienta a na to slúži táto metóda, ktorá to vypíše v správnom formáte
- 
### `MessageGrammar`
Slúži na kontrolovanie správneho formátu správ pomocou *regexov*.

### `MessageParser`
Trieda zpracuje správu a vyhodnotí, o akú správu ide, aké má parametre a či je v správnom formáte.

### `ErrorHandler`
Pomocná trieda na oznámenie chýb užívateľovi a prípadné ukončenie aplikácie.


## 6. Testovanie
Testovanie projektu som robil z väčšej časti ručne, použitím rôznych programov a kontrolovaním výstupu.
Pri testovaní som použil aplikáciu `Wireshark` s pluginom pre IPK24-CHAT protokol, na kontrolovanie prijímaných a odosielnaych správ.
Na testovanie varianty TCP som použil `netcat`, kde som simuloval komunikáciu so serverom.


### 6.1 Unit testy
Triedy, kde mi to dávalo zmysel, som testoval aj pomocou jednotkových testov. Konkrétne u tried `MessageParser`
a `ClientState`. Pri testovaní `ClientState` som vyskúšal všetky možné vstupy v daných stavoch. Pri testovaní triedy
`MessageParser` som skúsil pár vstupov, ktoré by mali prejsť a pár správ, ktoré mali buď zlý počet parametrov, alebo dané časti správy
nezodpovedali gramatike správ používaných v protokole `IPK24-CHAT`. Testy sa dajú spustiť príkazom `make test`.

### 6.2 Testovací scenár
Keďže testovanie prebiehalo väčšinu času ručne, pripravil som si testovací scenár, kde sú vypísané vstupy, ktoré má
používateľ zadať a výstupy, ktoré sa očakávajú na strane serveru. Testovací scenár je možné prezrieť v zložke `tests`
pod názvom `test_communication_scenarios.txt`. Scenár mi uľahčil testovanie, lebo som nemusel pri každom testovaní funkčnosti programu
vymýšľať vstupy a so zadaním kontrolovať výstupy. Vstupy pre server sú v testovacom scenári hlavne pre variantu TCP, keď som používal
`netcat` a dané vstupy som vkladal do terminálu, kde bol spustený.

Tu je ako príklad ukázaný prvý testovací scenár (čísla indikujú poradie posielania a prijímania správ): 
#### Terminál s `ipk24chat-client`:
```
$ ./ipk24chat-client -t tcp -s 127.0.0.1 -p 4567
/auth user1 123 userNick                                            1.
Success: ok      <- received reply                                  4.
Hello                                                               5.
user2: Hello back <- received mocked message from another user      8.
*C-d*                                                               9.
```
*`netcat` musí byť zapnutý ako prvý*

#### Terminál s `netcatom`
```
$ nc -4 -l -C -v 127.0.0.1 4567
Listening on localhost 4567
Connection received on localhost 52806
AUTH user1 AS userNick USING 123                                    2.
reply ok is ok              <- send reply                           3.
MSG FROM userNick IS Hello                                          6.
msg from user2 is Hello back  <- mock another user input            7.
BYE                                                                 10.
```

### 6.3 Testovanie aplikácie na referenčnom serveri

V záverečných fázach projektu som použil aj discord server na overenie správneho riešenia. Pri posielaní a prijímaných správach som mal zapnutý
`Wireshark`, kde som mohol vidieť všetky detaily o správach, ktoré posielam a prijímam. Tu som tiež využil testovací scenár popísaný [vyššie](#62-testovaci-scenar).

![udp_example](/doc/wireshark_example_udp.jpg)

![tcp_example](/doc/wireshark_example_tcp.jpg)

## 7. Bibliografia
[RFC768] Postel, J. User Datagram Protocol [online]. March 1997. [cited 2024-04-01]. DOI: 10.17487/RFC0768. Available at:\
https://datatracker.ietf.org/doc/html/rfc768 \
[RFC9293] Eddy, W. Transmission Control Protocol (TCP) [online]. August 2022. [cited 2024-04-01]. DOI: 10.17487/RFC9293. Available at:\
https://datatracker.ietf.org/doc/html/rfc9293#name-key-tcp-concept \
[IBM] IBM. How Sockets Work [online]. [cited 2024-04-01]. Available at:\
https://www.ibm.com/docs/en/i/7.3?topic=programming-how-sockets-work \
[Microsoft] Microsoft. System.Timers.Timer Class [online]. [used 2024-04-01]. Available at:\
https://learn.microsoft.com/en-us/dotnet/api/system.timers.timer?view=net-8.0 \
[Microsoft] Microsoft. Asynchronous Programming in C# [online]. [used 2024-04-01]. Available at:\
https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/ \
[Git-Cliff] Git-Cliff. Git-Cliff Documentation [online]. [used 2024-04-01]. Available at:\
https://git-cliff.org/docs/ \
[JetBrains] JetBrains. ReSharper Rider Samples Repository [.gitignore file] [online]. [cited 2024-04-01]. Available at:\
https://github.com/JetBrains/resharper-rider-samples/blob/master/.gitignore
