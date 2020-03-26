using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdateWin
{
    public class Obj
    {
        public string versionClient;
        public string nazwa_aplikacji;
        public string appName;
        public string versionFile;
        public string webPath;
        public string webVersion;
        public string ClientName;
        public string webVersionClient;
        public int numOfFiles;
        public int numOfTempFiles;
        public string[] tempFiles;

        //ścieżka (wybierana przez użytkownika podczas instalacji)
        public string adres = "";

        public void LoadData()
        {
            //globals
            versionClient = "wersja klienta: 0.0";
            nazwa_aplikacji = "TFT Analyser Auto Update";
            appName = "TFT Analyser.exe";
            versionFile = "version.dll";
            webPath = "https://raw.githubusercontent.com/OsKUBAny/TFT-Analyser-autoupdate/master/";
            webVersion = "ver.dll";
            ClientName = "newClient.exe";
            webVersionClient = "verClient.dll";
            numOfFiles = 7;  //liczba elementów w pliku
            numOfTempFiles = 5;  //liczba plików do pobrania przy instalacji
            tempFiles = new string[7] { "","","data.dll", "history.dll", "AutoUpdateWin.exe.config", "AutoUpdateWin.exe.manifest", "AutoUpdateWin.pdb" };
            //nazwy plików do instalacji ORAZ <liczba plików + 2>


        //FORMAT PLIKU versionFile:             Przykład:

        //0 wersja                              1,0                  - wymagany przecinek
        //1 ilość plików do pobrania            3         
        //2 ściezka 1 - aplikacja               NazwaAplikacji.exe   - musi być inna od appName
        //3 ścieżka 2                           plik.dll
        //4 ścieżka 3                           innyPlik.txt
        //5 ścieżka 4                           kolejnyPlik.txt
        //6 ścieżka 5                           -
        //...
        }

    }
}
