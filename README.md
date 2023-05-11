# sysprogy
Sistemsko programiranje 2022/2023, Anđelija Mijajlović, Mihajlo Bencun


Struktura datoteka je sledeća: serveri u svom root folderu imaju folder /files, u okviru kog se nalaze podfolderi sa .csv fajlovima. Imena tih foldera odgovaraju imenima Excel fajlova, dok će svi .csv fajlovi u okviru tog foldera postati zasebni radni listovi u tom Excel fajlu.
NPOI biblioteka ne podržava konkurentan pristup jednom XSSFWorkbook objektu zbog toga što taj format interno koristi Shared String Table za smeštanje string podataka iz ćelija i ta struktura nije implementirana na način koji omogućava konkurentan pristup. Zbog toga se XSSFWorkbook objekat mora zaključati pri svakom upisu vrednosti u ćeliju što unosi značajan overhead. Takva implementacija se može naći u okviru GetFilesMT funkcije u web api projektu. 
Konzolna aplikacija koristi više niti uz pomoć Threadpool-a za osluškivanje HTTP zahteva dok sama obrada .csv fajlova ide preko jedne niti iz Threadpool-a.
