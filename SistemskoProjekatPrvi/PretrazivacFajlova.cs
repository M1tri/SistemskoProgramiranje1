using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemskoProjekatPrvi
{
    public class PretrazivacFajlova
    {
        static readonly object workerLocker = new();
        static int brojAktivnihNiti = 0;
        static readonly StringBuilder ret = new("");
        static int brojFajlova = 0;
        public static String PretraziSaNitima(String kljuc)
        {
            brojAktivnihNiti = 0;
            brojFajlova = 0;
            ret.Clear();
            String content = "";

            try
            {
                String path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\.."));

                String[] fajlovi = Directory.GetFiles(path);

                foreach (String f in fajlovi)
                {
                    String imeFajla = Path.GetFileName(f);
                    if (imeFajla.ToLower().Contains(kljuc))
                    {
                        brojFajlova++;
                        ret.Append($"<p><a href=\"/download?putanja={Uri.EscapeDataString(f)}\"> {imeFajla} </a></p>");
                    }
                }

                String[] direktorijumi = Directory.GetDirectories(path);

                brojAktivnihNiti += direktorijumi.Length;

                foreach (String dir in direktorijumi)
                {
                    ThreadPool.QueueUserWorkItem(PretraziDirSaNitima, new Tuple<String, String>(dir, kljuc));
                }

                lock (workerLocker)
                {
                    while (brojAktivnihNiti > 0)
                    {
                        Monitor.Wait(workerLocker);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Javio se izuzetak: " + e.Message);
            }

            Console.WriteLine("Sa nitima: Zavrsena pretraga!");

            if (String.IsNullOrEmpty(ret.ToString()))
            {
                content += "<html> <head>\n";
                content += "<title>Projekat 1 - Rezultat</title>\n";
                content += "</head>\n";
                content += "<body>\n";
                content += "<h1>Sistemsko programiranje - Projekat 1</h1>\n";
                content += $"<h3>Rec koju ste pretrazili je: <span style='color:blue;'>{kljuc}</span></h3>\n";
                content += "<h3>Nije pronadjen nijedan fajl na osnovu kljuca.</h3>\n";
                content += "</body> </html>\n";

                return content;
            }

            content += "<html> <head>\n";
            content += "<title>Projekat 1 - Rezultat</title>\n";
            content += "</head>\n";
            content += "<body>\n";
            content += "<h1>Sistemsko programiranje - Projekat 1</h1>\n";
            content += $"<h3>Rec koju ste pretrazili je: <span style='color:blue;'>{kljuc}</span></h3>\n";
            content += $"<h3>Rezultati pretrage: </h3>\n";
            content += $"<h4>Broj pronadjenih fajlova: {brojFajlova} </h4>\n";
            content += "<h4>Kliknite na fajl da biste ga preuzeli.</h4>\n";
            content += $"<h4>{ret}</h4>\n";
            content += "</body> </html>\n";

            return content;
        }

        static void PretraziDirSaNitima(object? state)
        {
            try
            {
                var (path, kljuc) = ((Tuple<String, String>)state!);

                String files = "";
                int brFiles = 0;

                String[] fajlovi = Directory.GetFiles(path);

                foreach (String f in fajlovi)
                {
                    String imeFajla = Path.GetFileName(f);
                    if (imeFajla.ToLower().Contains(kljuc))
                    { 
                        brFiles++;
                        files += $"<p><a href=\"/download?putanja={Uri.EscapeDataString(f)}\"> {imeFajla} </a></p>";
                    }
                }

                String[] direktorijumi = Directory.GetDirectories(path);

                if (direktorijumi.Length > 0)
                {
                    lock (workerLocker)
                    {
                        brojAktivnihNiti += direktorijumi.Length;
                    }
                }

                foreach (String dir in direktorijumi)
                {
                    ThreadPool.QueueUserWorkItem(PretraziDirSaNitima, new Tuple<String, String>(dir, kljuc));
                }

                lock (workerLocker)
                {
                    ret.Append(files);
                    brojFajlova += brFiles;
                    brojAktivnihNiti--;

                    if (brojAktivnihNiti == 0)
                        Monitor.Pulse(workerLocker);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Javio se izuzetak tokom izvrsenja niti: " + e.Message);

                lock (workerLocker)
                {
                    brojAktivnihNiti--;

                    if (brojAktivnihNiti == 0)
                        Monitor.Pulse(workerLocker);
                }
            }
        }

        public static String PretraziBezNiti(String kljuc)
        {
            ret.Clear();
            brojFajlova = 0;
            String content = "";

            try
            {
                String path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\.."));

                String[] fajlovi = Directory.GetFiles(path);

                foreach (String f in fajlovi)
                {
                    String imeFajla = Path.GetFileName(f);
                    if (imeFajla.ToLower().Contains(kljuc))
                    {
                        brojFajlova++;
                        ret.Append($"<p><a href=\"/download?putanja={Uri.EscapeDataString(f)}\"> {imeFajla} </a></p>");
                    }
                }

                String[] direktorijumi = Directory.GetDirectories(path);

                foreach (String dir in direktorijumi)
                {
                    ret.Append(PretraziDirBezNiti(dir, kljuc));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("Bez niti: Zavrsena pretraga!");

            if (String.IsNullOrEmpty(ret.ToString()))
            {
                content += "<html> <head>\n";
                content += "<title>Projekat 1 - Rezultat</title>\n";
                content += "</head>\n";
                content += "<body>\n";
                content += "<h1>Sistemsko programiranje - Projekat 1</h1>\n";
                content += $"<h3>Rec koju ste pretrazili je: <span style='color:blue;'>{kljuc}</span></h3>\n";
                content += "<h3>Nije pronadjen nijedan fajl na osnovu kljuca.</h3>\n";
                content += "</body> </html>\n";

                return content;
            }

            content += "<html> <head>\n";
            content += "<title>Projekat 1 - Rezultat</title>\n";
            content += "</head>\n";
            content += "<body>\n";
            content += "<h1>Sistemsko programiranje - Projekat 1</h1>\n";
            content += $"<h3>Rec koju ste pretrazili je: <span style='color:blue;'>{kljuc}</span></h3>\n";
            content += $"<h3>Rezultati pretrage: </h3>\n";
            content += $"<h4>Broj pronadjenih fajlova: {brojFajlova} </h4>\n";
            content += "<h4>Kliknite na fajl da biste ga preuzeli.</h4>\n";
            content += $"<h4>{ret}</h4>\n";
            content += "</body> </html>\n";

            return content;
        }

        static String PretraziDirBezNiti(String path, String kljuc)
        {
            String files = "";

            try
            {
                String[] fajlovi = Directory.GetFiles(path);

                foreach (String f in fajlovi)
                {
                    String imeFajla = Path.GetFileName(f);
                    if (imeFajla.ToLower().Contains(kljuc))
                    {
                        brojFajlova++;
                        files += $"<p><a href=\"/download?putanja={Uri.EscapeDataString(f)}\"> {imeFajla} </a></p>";
                    }
                }

                String[] direktorijumi = Directory.GetDirectories(path);

                foreach (String dir in direktorijumi)
                {
                    files += PretraziDirBezNiti(dir, kljuc);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Javio se izuzetak tokom pretrage u {path}: {e.Message}");
            }

            return files;
        }
    }
}
