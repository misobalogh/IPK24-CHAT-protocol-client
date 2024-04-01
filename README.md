# Klient pre chatovaci server pouzivajuci IPK24-CHAT protokol
## IPK - Projekt 1

## 1. Uvod
Cielom tohto projektu bolo vytvorenie klienta k serveru chatovacej konzolovej aplikacie, ktora na komunikaciu
pouziva protokol IPK24-CHAT. Naimplementovat bolo treba dve varianty - UDP a TCP. Obidve varianty mali svoje 
specializacie a problemy, ktore tym vznikli.

## 2. Obsah
1. [Uvod](#1-uvod)
2. [Obsah](#2-obsah)
3. [Ako spustit projekt](#2-ako-spustit-projekt)
4. [Zakladna teoria k projektu](#3-zakladna-teoria-k-projektu)\
    4.1 [TCP](#31-tcp-transmission-control-protocol)\
    4.2 [UDP](#32-udp-user-datagram-protocol)\
    4.3 [Socket](#33-socket)
5. [Struktura projektu](#4-struktura-projektu)
6. [Testovanie](#5-testovanie)
7. [Zaver](#6-zaver)
8. [Bibliografia](#7-bibliografia)


## 3. Ako spustit projekt
Projekt je mozne zostavit pomocou prikazu make v korenovom adresari. Po zostaveni binarneho suboru `ipk24chat-client` je ho mozne spustit s parametrami definovanymi v zadani projektu. Pre viac informaci pouzite parameter `-h`.
Dalsie ciele programu make:
- `make` - zostavi projekt
- `make help` - vypise uzitocne informacie o spusteni projektu
- `make test` - spusti unit testy k projektu
- `make udp`  - spusti klienta s predvolenymi parametrami a protokolom UDP
- `make test` - spusti klienta s predvolenymi parametrami a protokolom TCP

## 4. Zakladna teoria k projektu

### 4.1 TCP (Transmission Control Protocol)
TCP poskytuje spoľahlivú službu doručovania dát v poradí akom boli odoslane v podobe prúdu bytov aplikáciám. 
Spravy su prenasane cez sieť pomocou TCP segmentov, kde kazdy segment je odoslany ako Internet Protocol datagram. 
Spolahlive dorucenie sprav si berie vacsiu reziu na case prenosu a velkosti prenosu dat. Pouziva sa
na prenasanie suborov, zasielanie emailov alebo ho tiez vyuziva SSH (Secure Shell).

### 4.2 UDP (User Datagram Protocol)
UDP je jednoduchý protokol pre aplikačné programy na odosielanie správ iným programom 
s malými nárokmi na protokolové mechanizmy. Na rozdiel od TCP vsak nie je spolahlivy a nie je zaistene
doručenia správ alebo ochrana pred duplikovanymi spravami. Pouziva sa v pripadoch, kedy je dolezita latencia, 
a nie spolahlivost dorucenia vsetkych dat - napriklad videohovory.

### 4.3 Socket
Sockety sa pouzivaju na interakciu medzi klientom a serverom. V modeli klient-server caka socket na serveri
na poziadavky od klienta. Server najprv vytvori adresu, pomocou ktorej je mozne server najst zo strany klienta.
Ked je adresa vytvorena, server caka na na poziadavku od klienta. Klient sa na server pripoji tiez pomocou socketu, 
prebehne vymena dat, server obsluzi poziadvaku klienta a odosle odpoved klientovi.

## 5. Struktura projektu
Projekt som sa snazil rozdelit na podproblemy, ktore som rozdelil do logickych celkov alebo tried, 
ktore riesia dany podproblem. Rozdelenie programu a vzajomna spolupraca tried je zobrazena v diagrame tried:

![Chat App Class Diagram](/doc/ChatAppClassDiagram.png)
*(Diagram je dostupny v priecinku `doc/`)*

Vstupny bod programu je v subore Program spustenim metody Main(). Najrpv sa vytvori instancia triedy `CommandLineOptions`,
ktora vo svojom konstruktore invokuje svoju metodu `ParseArguments()`. Ta priradi hodnoty jednotlivych argumentov do svojich
atributov, alebo ukonci program s chybou. Ziskane nastavenia z argumentov sa potom predaju instancii triedy `UserInputHandler`,
ktora spracuva uzivatelske vstupy a prikazy.

### `UserInputHandler`
Tato trieda si podla zvoleneho nastavenia varianty protokolu vytvori instanciu triedy `UdpClient` alebo `TcpClient`.
Obidve tieto triedy dedia z abstraktnej triedy `ClientBase`, ktora deklaruje tri metody:
- `SendMessageAsync()` - ktora sluzi na asynchronne posielanie sprav
- `ReceiveMessageAsync()` - na asynchronne prijmanie sprav
- `Close()` - uvolni zdroje a ukonci komunikaciu.

Pri spracovani uzivatelskeho vstupu, trieda najprv zavola asynchronnu metodu `ReceiveMessageAsync()` a potom vo `while` cykle
cita standardny vstup a spracuva  ho. Ak sa vstup zacina znakom `/`, pokusi sa o spustenie prikazu, ak taky existuje. Ak nie,
vypise uzivatelovi varovanie. V inom pripade berie vstup ako obycajnu spravu. Ak je prislusny prikaz alebo poslanie spravy nepripustne 
v aktualnom stave klienta, oznami to uzivatelovi. V pripade, ze niektore spravy potrebuju odpoved alebo potvrdenie nejakej spravy,
vstupy od uzivatela si uklada do fronty a akonahle pride cakana odpoved, odosle vsetky spravy, ktore cakali vo fronte dokym nenarazi
na spravu, ktora zase potrebuje potvrdenie od serveru. Trieda si taktiez drzi uzivatelske meno, ktore sa pripadne da zmenit prikazom `rename`.

### `TcpClient`
Trieda sluzi na spracovanie sprav cez TCP protokol. Vytvori si `reader`. `writer`, `network stream`, pripoji sa na `endpoint` a len
z tohoto `streamu` cita alebo do neho zapisuje spravy.

### `UdpClient`
Komunikacia musi fungovat na dynamickych portoch, takze trieda si po prvej sprave od serveru okrem potvrdenia `confirm` zapamata port odkial prisla sprava,
a na ten port uz bude smerovat vsetky svoje spravy. Tato trieda pri kazdej prijatej (okrem spravy `confirm`) sprave posle hned spravu `confirm` naspat serveru.
Ma taktiez casovac, ktory sa spusti pri odoslani spravy, a ked tento casovac vyprsi, pokusi sa danu spravu poslat znovu. Maximalne spravi tolko pokusov, kolko je
definovanych cez parameter `MaxRetransmissions`.

### `ClientState`
Tato trieda stvarnuje konecny automat zo zadania. Pomocou prijatych sprav od servera sa prepina do roznych stavov.
Klient potom umoznuje niektore akcie len v urcitych stavoch.

### `Message`
Abstraktna trieda `Message` ma potomkov, ktory reprezentuje konkretne typy sprav. Ma deklarovane tri metody, ktore musia potom konkretne podtriedy implementovat:
- `CraftTcp()` - sluzi na vytvorenie daneho typu spravy v spravnom formate pre TCP variantu
- `CraftUdp()` - sluzi na vytvorenie daneho typu spravy v spravnom (`byte`) formate pre UDP variantu
- `PrintOutput()` - niektore spravy pri prijati u klienta sa maju zobrazit na vystup u klienta a na to sluzi tato metoda, ktora to vypise v spravnom formate

### `MessageGrammar`
Sluzi na kontrolovanie spravneho formatu sprav pomocou *regexov*.

### `MessageParser`
Trieda zpracuje spravu a vyhodnoti, o aku spravu ide, ake ma parametre a ci je v spravnom formate.

### `ErrorHandler`
Pomocna trieda na oznamenie chyb uzivatelovi a pripadne ukoncenie aplikacie.


## 6. Testovanie
Testovanie projektu som robil z vacsej casti rucne, pouzitim roznych programov a kontrolovanim vystupu.
Pri testovani som pouzil aplikaciu `Wireshark` s pluginom pre IPK24-CHAT protokol, na kontrolovanie prijmanych a odosielnaych sprav.
Na testovanie varianty TCP som pouzil `netcat`, kde som simuloval komunikaciu so serverom.


### 6.1 Unit testy
Triedy, kde mi to davalo zmysel, som testoval aj pomocou jedntokovych testov. Konkretne u tried `MessageParser` 
a `ClientState`. Pri testovani `ClientState` som vyskusal vsetky mozne vstupy v danych stavoch. Pri testovanie triedy
`MessageParser` som skusil par vstupov, ktore by mali prejst a par sprav, ktore mali bud zly pocet parametrov, alebo dane casti spravy
nezodpovedali gramatike sprav pouzivanych v protokole `IPK24-CHAT`. Testy sa daju spustit prikazom `make test`.

### 6.2 Testovaci scenar
Kedze testovanie prebiehalo vacsinu casu rucne, pripravil som si testovaci scenar, kde su vypisane vstupy, ktore ma
pouzivatel zadat a vystupy, ktore sa ocakavaju na strane severu. Testovaci scenar je mozne prezriet v zlozke `tests`
pod nazvom `test_communication_scenarios.txt`. Scenar mi ulahcil testovanie, lebo som nemusel pri kazdom testovani funkcnosti programu
vymyslat vstupy a so zadanim kontrolovat vystupy. Vstupy pre server su v testovacom scenari hlavne pre variantu TCP, ked som pouzival
`netcat` a dane vstupy som vkladal do terminalu, kde bol spusteny.

Tu je ako priklad ukazany prvy testovaci scenar (cisla indikuju poradie posielania a prijmania sprav): 
#### Terminal s `ipk24chat-client`:
```
$ ./ipk24chat-client -t tcp -s 127.0.0.1 -p 4567
/auth user1 123 userNick                                            1.
Success: ok      <- received reply                                  4.
Hello                                                               5.
user2: Hello back <- received mocked message from another user      8.
*C-d*                                                               9.
```
*`netcat` musi byt zapnuty ako prvy*
#### Terminal s `netcatom`
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

### 6.3 Testovanie aplikacie na referncnom serveri
V zaverecnych fazach projektu som pouzil aj discord server na overenie spravneho riesenia. Pri posielani a prijmanych sprav som mal zapnuty 
`Wireshark`, kde som mohol vidiet vsetky detaily o spravach ktore posielam a prijmam. Tu som taktiez vyuzil testovaci scenar popisany [vyssie](#62-testovaci-scenar).

![udp_example](/doc/wireshark_example_udp.jpg)

![tcp_example](/doc/wireshark_example_tcp.jpg)

## 7. Zaver
Projekt bol dost rozsiahly, takze som si potrenoval rozlozenie velkeho problemu na mensie casti, naucil som sa viac programovat v jazyku
C#, pracovat s `netcatom`, `Wiresharkom`. Taktiez som sa naucil viac o programovani pocitacovych komunikacii. Na zaver by som chcel este podakovat
za spristupnenie discord serveru, kde som si mohol vyskusat funkcionalitu svojho programu a zaroven to bolo zaujimave prepojenie skolskeho projektu 
s niecim realnym.

## 8. Bibliografia
[Informacie k TCP](https://datatracker.ietf.org/doc/html/rfc9293#name-key-tcp-concepts)\
[Informacie k UDP](https://datatracker.ietf.org/doc/html/rfc768)\
[Informacie k Socketu](https://www.ibm.com/docs/en/i/7.3?topic=programming-how-sockets-work)\
[Casovac v C#](https://learn.microsoft.com/en-us/dotnet/api/system.timers.timer?view=net-8.0)\
[Asynchronne programvoanie v C#](https://learn.microsoft.com/en-us/dotnet/api/system.timers.timer?view=net-8.0)\
[Vygenerovany CHANGELOG](https://git-cliff.org/docs/)