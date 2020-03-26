using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using IWshRuntimeLibrary;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Net;

namespace AutoUpdateWin
{
    public partial class Main : Form
    {
        Obj obj = new Obj();
        static string nL = Environment.NewLine;

        public Main()
        {
            InitializeComponent();
        }

        public void Main_Load(object sender, EventArgs e)
        {
            obj.LoadData();
            VersionLabel.Text = obj.versionClient;
            Console.ResetText();
            this.Text = obj.nazwa_aplikacji;
            this.Show();

            if (System.IO.File.Exists(obj.appName) == false || System.IO.File.Exists(obj.versionFile) == false)
            {
                Console.AppendText("Nie znaleziono wymaganych plików. Wymagane pobranie niezbędnych plków aplikacji");
                DialogResult resultInstall = MessageBox.Show("Nie znaleziono wymaganych plików. Czy chcesz zainstalować aplikację?",
                     "Nie znaleziono programu", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (resultInstall == DialogResult.No)
                {
                    Console.AppendText(nL + nL);
                    Console.AppendText("Proces niemożliwy do wykonania: (401) permission denied");
                    return;
                }
                else if (resultInstall == DialogResult.Yes)
                {
                    FolderBrowserDialog sfd = new FolderBrowserDialog();
                    //sfd.RootFolder = Environment.SpecialFolder.ProgramFilesX86;
                    sfd.Description = "Wybierz miejsce instalacji programu";
                    if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        obj.adres = sfd.SelectedPath + "\\";
                        Console.AppendText(nL + "Wybrano lokalizację: "+obj.adres);


                        this.Activate();
                        try
                        {
                            if (System.IO.File.Exists(obj.adres + obj.appName) == false) System.IO.File.Create(obj.adres + obj.appName).Dispose();
                            if (System.IO.File.Exists(obj.adres + obj.versionFile) == false) 
                            {
                                System.IO.File.Create(obj.adres + obj.versionFile).Dispose();
                                StreamWriter plik = new StreamWriter(obj.adres + obj.versionFile);
                                plik.WriteLine("0,0");
                                plik.WriteLine("0,0");
                                plik.Close();

                            }
                        }
                        catch (Exception ex)
                        {
                            ex.ToString();
                            string[] exception = ex.ToString().Split('\n');
                            Console.AppendText("wystąpił błąd: " + exception + nL);

                            Console.AppendText(nL);
                            Console.AppendText("Zakończono proces instalacji: (400) nie udało się zainstalować");
                            MessageBox.Show("Nie udało się zainstalować programu", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        Console.AppendText(nL);
                        for (int i = 2; i < obj.numOfTempFiles + 2; i++)
                        {
                            Thread.Sleep(1000);
                            string message = "pobieranie pliku ";
                            message += i - 1; message += " z "; message += obj.numOfTempFiles; message += ": " + obj.tempFiles[i];
                            Console.AppendText(nL);
                            Console.AppendText(message);

                            try
                            {
                                DownloadFile(obj.webPath + obj.tempFiles[i], obj.adres + obj.tempFiles[i]);
                            }
                            catch (Exception ex)
                            {
                                ex.ToString();
                                string[] exception = ex.ToString().Split('\n');
                                Console.AppendText(" - wystąpił błąd pobierania: " + exception + nL);
                                DeleteFiles(obj.tempFiles, i - 2);
                                System.IO.File.Delete(obj.adres + obj.appName);
                                System.IO.File.Delete(obj.adres + obj.versionFile);

                                Console.AppendText(nL + nL);
                                Console.AppendText("Zakończono proces instalacji: (400) nie udało się zainstalować");
                                MessageBox.Show("Instalacja niepowiodła się", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }
                            Console.AppendText(" - ukończone");
                        }
                        Console.AppendText(nL + nL);
                        Console.AppendText("Pobieranie zakończone pomyślnie. Oczekiwanie na restart instalatora...");
                        Thread.Sleep(1000);
                        if (System.IO.File.Exists("restart.bat") == false)
                        {
                            Console.AppendText(nL + "tworzenie pliku wsadowego");
                            System.IO.File.Create("restart.bat").Dispose();

                            StreamWriter plik = new StreamWriter("restart.bat");
                            plik.WriteLine("@echo off");
                            plik.WriteLine("taskkill /f /im AutoUpdateWin.exe");
                            plik.WriteLine("copy /y AutoUpdateWin.exe {0}", "\"" + obj.adres.Remove(obj.adres.Length - 1, 1) + "\"");
                            plik.WriteLine("tiemout 1");
                            plik.WriteLine("del /f /q AutoUpdateWin.exe");
                            plik.WriteLine("cd /d {0}:", obj.adres[0]);
                            plik.WriteLine("cd {0}", "\"" + obj.adres + "\"");
                            plik.WriteLine("start AutoUpdateWin.exe");
                            plik.WriteLine("del %0");
                            plik.WriteLine("exit");
                            plik.Close();
                        }
                        Console.AppendText(nL + " --- ponowne uruchamianie instalatora --- ");
                        Thread.Sleep(2000);
                        System.Diagnostics.Process.Start("restart.bat");
                        return;
                    }
                    else
                    {
                        Console.AppendText(nL + nL + "Zakończono proces instalacji: (404) nie podano ścieżki");
                        return;
                    }
                }
            }
            this.TopMost = true;
            //pobranie wersji i sprawdzanie dostępności
            chceckVerionClient();

            try
            {
                DownloadFile(obj.webPath + obj.webVersion, obj.webVersion);
            }
            catch (Exception ex)
            {
                ex.ToString();
                string[] exception = ex.ToString().Split('\n');

                Console.AppendText("Wystąpił błąd: "+ exception);
                Console.AppendText(nL + nL);
                Console.AppendText("Zakończono proces aktualizacji: (400) nie udało się zaktualizować");
                MessageBox.Show("Nie udało się zaktualizować programu", "", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.TopMost = false;
                return;
            }
            //start aktualizowania
            StreamReader plikR = new StreamReader(obj.webVersion);
            string[] data = new string[obj.numOfFiles];

            for (int i = 0; i < obj.numOfFiles; i++)
            {
                data[i] = plikR.ReadLine();
            }
            plikR.Close();
            System.IO.File.Delete(obj.webVersion);
            if (chceckVerion(data[0]) == false) 
            {
                this.TopMost = false;
                return;
            }

            //kontynuacja aktualizowania
            this.TopMost = false;
            Console.AppendText("Dostępna aktualizacja do wersji "+ data[0]);
            DialogResult resultUpdate = MessageBox.Show("Dostępna aktualizacja do wersji " + data[0] + ". Czy chcesz kontynuować?",
                     "Dostępna aktualizacja", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (resultUpdate == DialogResult.No)
            {
                this.Show();
                Console.AppendText(nL);
                Console.AppendText("Proces niemożliwy do wykonania: (401) permission denied");
                return;
            }
            this.Show();
            ProgressBar.Value = 5;
            this.TopMost = true;
           
            Console.AppendText(nL+nL);
            Console.AppendText("Rozpoczęto proces aktualizacji");
            Thread.Sleep(1000);

            for (int i = 0; i < int.Parse(data[1]); i++)
            {
                Console.AppendText(nL);
                string message;
                message = "pobieranie pliku " + (i + 1) + " z " + data[1] + ": " + data[i + 2];
                Console.AppendText(message);  
                try
                {
                    DownloadFile(obj.webPath + data[i + 2], data[i + 2]);
                    float percent = ((float)(i + 1) / float.Parse(data[1])) * 100;
                    ProgressBar.Value = (int)percent - 5;
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    ex.ToString();
                    string[] exception = ex.ToString().Split('\n');
                    Console.AppendText(" - wystąpił błąd pobierania: "+ exception);
                    Console.AppendText(nL);
                    DeleteFiles(data, i);

                    Console.AppendText(nL+nL);
                    Console.AppendText("Zakończono proces aktualizacji: (400) nie udało się zaktualizować");
                    ProgressBar.ForeColor = Color.Red;
                    this.TopMost = false;

                    MessageBox.Show("Nie udało się zaktualizować", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                Console.AppendText(" - ukończone");
                this.Show();
            }
            if (Replace(data) == false) 
            {
                this.TopMost = false;
                return; 
            }
            string actualVersion = "";
            try
            {
                StreamReader plik = new StreamReader(obj.versionFile);
                plik.ReadLine();
                actualVersion = plik.ReadLine();
                plik.Close();
            }
            catch (Exception ex)
            {
                ex.ToString();
                string[] exception = ex.ToString().Split('\n');
                this.Show();
                Console.AppendText("Wystąpił błąd: " + exception);
                Console.AppendText(nL);
                Console.AppendText("Nie można zweryfikować wersji instalatora" + nL + nL);
                DeleteFiles(data, int.Parse(data[1]));
                Console.AppendText(nL + nL);
                Console.AppendText("Zakończono proces aktualizacji: (400) nie udało się zaktualizować");
                ProgressBar.ForeColor = Color.Red;
                this.TopMost = false;

                MessageBox.Show("Nie udało się zaktualizować", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;

            }
            StreamWriter plikW = new StreamWriter(obj.versionFile);
            plikW.WriteLine(data[0]);
            plikW.WriteLine(actualVersion);
            plikW.Close();
            Console.AppendText(nL + nL);
            Console.AppendText("Pobieranie zakończone pomyślnie");
            Console.AppendText(nL + "Tworzenie skrótu...");
            appShortcutToDesktop(obj.appName);
            Console.AppendText(nL + nL + "Zakończono proces aktualizacji: (200) Pomyślnie zaktualizowano");

            this.TopMost = false;
            ProgressBar.Value = 100;
            MessageBox.Show("Zakończono aktualizację", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        bool chceckVerion(string newVersion)
        {
            string actualVersion = "";
            try
            {
                StreamReader plikR = new StreamReader(obj.versionFile);
                actualVersion = plikR.ReadLine();
                plikR.Close();
            }
            catch (Exception ex)
            {
                ex.ToString();
                string[] exception = ex.ToString().Split('\n');
                this.Show();
                Console.AppendText(nL);
                Console.AppendText("Wystąpił błąd: "+ exception);
                Console.AppendText(nL+nL);
                Console.AppendText("Zakończono proces aktualizacji: (400) nie udało się zaktualizować");
                this.TopMost = false;
                return false;
            }
            if (actualVersion == null) actualVersion = "0";

            try
            {
                if (float.Parse(newVersion) <= float.Parse(actualVersion))
                {
                    this.Show();
                    this.TopMost = false;
                    Console.AppendText("Wersja programu: " + actualVersion + " - aktualizacja niewymagana");
                    MessageBox.Show("Posiadasz najnowszą wersję", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Console.AppendText(nL+nL);
                    Console.AppendText("Zakończono proces aktualizacji: (204) nie wymaga aktualizacji");
                    return false;
                }
                return true;
            }
            catch(Exception ex)
            {
                ex.ToString();
                string[] exception = ex.ToString().Split('\n');
                this.Show();
                Console.AppendText(nL);
                Console.AppendText("Wystąpił błąd: Błędny format danych w pliku "+obj.versionFile);
                Console.AppendText(nL+nL);
                Console.AppendText("Zakończono proces aktualizacji: (400) nie udało się zaktualizować");
                this.TopMost = false;
                return false;

            }

            
        }

        public void DeleteFiles(string[] data, int val)
        {
            for (int i = 2; i < val + 2; i++)
            {

                Console.AppendText(nL);
                Console.AppendText("usuwanie pliku " + data[i] + ": ");
                try
                {
                    System.IO.File.Delete(obj.adres + data[i]);
                }
                catch (Exception)
                {
                    Console.AppendText("nie udało się usunąć pliku");
                }
                Console.AppendText("ukończone");
                this.Show();
            }
            return;
        }
        public bool Replace(string[] data)
        {
            try
            {
                System.IO.File.Replace(data[2], obj.appName, "backupApp.exe");
                System.IO.File.Delete("backupApp.exe");
            }
            catch (Exception ex)
            {
                this.Show();
                ex.ToString();
                string[] exception = ex.ToString().Split('\n');
                Console.AppendText(nL);
                Console.AppendText("Wystapił błąd: "+ exception);

                DeleteFiles(data, int.Parse(data[1]));
                Console.AppendText(nL+nL);
                Console.AppendText("Zakończono proces aktualizacji: (400) nie udało się zaktualizować");
                this.Show();
                this.TopMost = false;
                MessageBox.Show("Nie udało się zaktualizować", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
                
            }
            return true;
        }

        public void DownloadFile(string url, string path)
        {
            WebClient web = new WebClient();
            web.DownloadFile(new Uri(url), path);

            return;
        }

        public void appShortcutToDesktop(string linkName)
        {
            string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            int index = appPath.LastIndexOf('\\');
            string instalerPath = appPath.Remove(index + 1, appPath.Length - index - 1);
            instalerPath += obj.appName;

            string link = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            + Path.DirectorySeparatorChar + obj.appName + ".lnk";
            var shell = new WshShell();
            var shortcut = shell.CreateShortcut(link) as IWshShortcut;
            shortcut.TargetPath = instalerPath;
            shortcut.WorkingDirectory = appPath.Remove(index + 1, appPath.Length - index - 1); 
            //shortcut...
            shortcut.Save();
        }
        void chceckVerionClient()
        {
            try
            {
                DownloadFile(obj.webPath + obj.webVersionClient, obj.webVersionClient);
            }
            catch (Exception ex)
            {
                ex.ToString();
                string[] exception = ex.ToString().Split('\n');

                Console.AppendText("Wystąpił błąd: " + exception);
                Console.AppendText(nL);
                Console.AppendText("Nie można zweryfikować wersji instalatora"+nL+nL);
                Thread.Sleep(2000);
                return;
            }
            string newVersion="";
            try
            {
                StreamReader plikR = new StreamReader(obj.webVersionClient);
                newVersion = plikR.ReadLine();
                plikR.Close();
            }
            catch (Exception ex)
            {
                ex.ToString();
                string[] exception = ex.ToString().Split('\n');
                this.Show();
                Console.AppendText("Wystąpił błąd: " + exception);
                Console.AppendText(nL);
                Console.AppendText("Nie można zweryfikować wersji instalatora"+nL+nL);
                Thread.Sleep(2000);
                return;
            }
            System.IO.File.Delete(obj.webVersionClient);

            string actualVersion = "";
            try
            {
                StreamReader plikR = new StreamReader(obj.versionFile);
                plikR.ReadLine();
                actualVersion = plikR.ReadLine();
                plikR.Close();
            }
            catch (Exception ex)
            {
                ex.ToString();
                string[] exception = ex.ToString().Split('\n');
                this.Show();
                Console.AppendText("Wystąpił błąd: " + exception);
                Console.AppendText(nL);
                Console.AppendText("Nie można zweryfikować wersji instalatora"+nL+nL);
                Thread.Sleep(2000);
                return;
            }

            if (actualVersion == null) actualVersion = "0";
            try
            {
                if (float.Parse(newVersion) > float.Parse(actualVersion))
                {
                    Console.AppendText("Dostępna nowa wersja instalatora "+newVersion);
                    DialogResult result = MessageBox.Show("Dostępna nowa wersja instalatora. Czy chcesz ją zainstalować?",
                        "Dostępna aktualizajna", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            DownloadFile(obj.webPath + obj.ClientName, obj.ClientName);
                        }
                        catch (Exception ex)
                        {
                            ex.ToString();
                            string[] exception = ex.ToString().Split('\n');

                            Console.AppendText(nL+"Wystąpił błąd: " + exception);
                            Console.AppendText(nL);
                            Console.AppendText("Zakończono proces aktualizacji: (400) nie udało się zaktualizować"+nL+nL);
                            Thread.Sleep(2000);
                            return;
                        }

                        string appVersion = "";
                        try
                        {
                            StreamReader plikR = new StreamReader(obj.versionFile);
                            appVersion = plikR.ReadLine();
                            plikR.Close();
                        }
                        catch (Exception ex)
                        {
                            ex.ToString();
                            string[] exception = ex.ToString().Split('\n');
                            this.Show();
                            Console.AppendText(nL);
                            Console.AppendText("Wystąpił błąd: " + exception);
                            Console.AppendText(nL);
                            Console.AppendText("Zakończono proces aktualizacji: (400) nie udało się zaktualizować"+nL+nL);
                            System.IO.File.Delete(obj.ClientName);
                            Thread.Sleep(2000);
                            return;
                        }

                        try
                        {
                            StreamWriter plikW = new StreamWriter(obj.versionFile);
                            plikW.WriteLine(appVersion);
                            plikW.WriteLine(newVersion);
                            plikW.Close();
                        }
                        catch (Exception ex)
                        { 
                            ex.ToString();
                            string[] exception = ex.ToString().Split('\n');
                            this.Show();
                            Console.AppendText(nL);
                            Console.AppendText("Wystąpił błąd: " + exception);
                            Console.AppendText(nL);
                            Console.AppendText("Zakończono proces aktualizacji: (400) nie udało się zaktualizować" + nL + nL);
                            System.IO.File.Delete(obj.ClientName);
                            Thread.Sleep(2000);
                            return;
                        }

                        Console.AppendText(nL + "Pobieranie zakończone. Tworzenie pliku wsadowego...");
                        System.IO.File.Create("restartClient.bat").Dispose();

                        string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

                        int index = appPath.LastIndexOf('\\');
                        string newPath = appPath.Remove(index + 1, appPath.Length - index - 1);
                        newPath += obj.ClientName;

                        StreamWriter plik = new StreamWriter("restartClient.bat");
                        plik.WriteLine("@echo off");
                        plik.WriteLine("taskkill /f /im AutoUpdateWin.exe");
                        plik.WriteLine("cls");
                        plik.WriteLine("timeout 1");
                        plik.WriteLine("copy {0} {1}", "\"" + newPath + "\"", "\"" + appPath + "\"");
                        plik.WriteLine("del {0}", "\"" + newPath + "\"");
                        plik.WriteLine("start AutoUpdateWin.exe");
                        plik.WriteLine("del %0");
                        plik.WriteLine("exit");
                        plik.Close();

                        Console.AppendText(nL + "Kończenie aktualizacji. Oczekiwanie na restart klienta...");
                        Thread.Sleep(2000);
                        System.Diagnostics.Process.Start("restartClient.bat");
                        return;
                    }
                    Console.AppendText(nL + "Zaniechano aktualizacji instalatora. Kontynuacja procesu aktualizacji programu" + nL + nL);
                }
                return;
            }
            catch (Exception ex)
            {
                ex.ToString();
                string[] exception = ex.ToString().Split('\n');
                this.Show();
                Console.AppendText(nL+"Wystąpił błąd: " + exception);
                Console.AppendText(nL);
                Console.AppendText("Nie można zweryfikować wersji instalatora"+nL+nL);
                this.TopMost = false;
                Thread.Sleep(2000);
                return;

            }

            
        }
    }
}
