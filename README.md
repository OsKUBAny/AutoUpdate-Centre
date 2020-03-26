# AutoUpdate-Centre ©Mvcias

Program służący do automatycznego sprawdzania dostępnych aktualizacji dla wybranej aplikacji

Wszystkie ustawienia można zmienić w klasie Obj.cs \n
Uwaga: program obsługuje jedynie poączenia http/https

Na serwerze znajdować się muszą oprócz plików konfiguracyjnych:
>plik z wersją klienta w formacie float(,)
>aplikacja oraz klient
>dodatkowe pliki wymagane do instalacji programu
>plik z informacjami dotyczącymi aktualizacji w formacie:
//FORMAT PLIKU versionFile:             Przykład:

        //0 wersja                              1,0                  - wymagany przecinek
        //1 ilość plików do pobrania            3         
        //2 ściezka 1 - aplikacja               NazwaAplikacji.exe   - musi być inna od appName
        //3 ścieżka 2                           plik.dll
        //4 ścieżka 3                           innyPlik.txt
        //5 ścieżka 4                           kolejnyPlik.txt
        //6 ścieżka 5                           -
        //...

Przy instalacji program stworzy pusty plik o nazwie docelowej aplikacji oraz plik z wersją aplikacji oraz klienta
a także pobierze wybrane pliki wymagane do poprawnego działania klienta jak i aplikacji.

Program po poprawnym zaktualizowaniu aplikacji tworzy do niej skrót na pulpicie użytkownika.

Wymagany system Windows oraz .NetFramework w wersji 4.7.2
