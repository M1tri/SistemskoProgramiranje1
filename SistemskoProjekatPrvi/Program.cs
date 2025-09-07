using SistemskoProjekatPrvi;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SistemskoProjekatPrvi
{
    internal class Program
    {
        private static readonly int port = 5000;
        private static readonly KesLRU m_kes = new(3);
        private static readonly Stopwatch m_stopwatch = new();

        static void Main(string[] args)
        {
            static TimeSpan zaustaviStopwatch()
            {
                m_stopwatch.Stop();
                var vreme = m_stopwatch.Elapsed;
                m_stopwatch.Reset();
                return vreme;
            }

            try
            {
                TcpListener listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                Console.WriteLine($"Otvoren server na portu {port}");

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();
                    StreamReader reader = new StreamReader(stream, Encoding.ASCII);
                    StreamWriter writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

                    string? request = reader.ReadLine();
                    if (string.IsNullOrEmpty(request))
                        continue;

                    Console.WriteLine("------------------------------------------------------------");
                    Console.WriteLine($"Stigao je zahtev {request}.");

                    string[] tokeni = request.Split(' ');
                    if (tokeni.Length <= 2)
                        continue;

                    string method = tokeni[0];
                    string sadrzaj = tokeni[1];

                    if (method == "GET")
                    {
                        int pos1 = sadrzaj.IndexOf('/');

                        String metoda = "";
                        int index = pos1+1;
                        while (index < sadrzaj.Length)
                        {
                            if (sadrzaj[index] == ' ' || sadrzaj[index] == '?')
                                break;
                            metoda += sadrzaj[index];
                            index++;
                        }

                        Console.WriteLine("Metoda je " + metoda);

                        String response = "";
                        if (metoda == "pronadji-fajlove")
                        {
                            try
                            {
                                int start = sadrzaj.IndexOf("kljuc=");

                                if (start == -1)
                                {
                                    Console.WriteLine("Primljen nevalidan zahtev!");

                                    String html = "";
                                    html += "<html> <head>\n";
                                    html += "<title>Projekat 1 - Rezultat</title>\n";
                                    html += "</head>\n";
                                    html += "<body>\n";
                                    html += "<h1> Nevalidan zahtev!</h1>\n";
                                    html += "</body> </html>\n";

                                    response += "HTTP/1.1 200 OK\r\n";
                                    response += "Content-Type: text/html; charset=UTF-8\r\n";
                                    response += $"Content-Length: " + html.Length + "\r\n\r\n";
                                    response += html;

                                    writer.Write(response);
                                    client.Close();
                                    continue;
                                }

                                String kljuc = sadrzaj.Substring(start + "kljuc=".Length);

                                Console.WriteLine("Primljen zahtev za pretragu sa kljucem " + kljuc);

                                String ret;
                                m_stopwatch.Start();

                                if (m_kes.Citaj(kljuc, out String? value))
                                {
                                    var vreme = zaustaviStopwatch();
                                    Console.WriteLine($"Podatak je pronadjen u kesu za {vreme}s");
                                    ret = value;
                                }
                                else
                                {
                                    zaustaviStopwatch();
                                    Console.WriteLine("Nema odgovarajucih podataka u kesu, krece pretraga!");

                                    m_stopwatch.Start();
                                    ret = PretrazivacFajlova.PretraziBezNiti(kljuc);
                                    var vreme_bez_niti = zaustaviStopwatch();

                                    m_stopwatch.Start();
                                    ret = PretrazivacFajlova.PretraziSaNitima(kljuc);
                                    var vreme_sa_nitima = zaustaviStopwatch();
                                    
                                    Console.WriteLine($"Vreme bez niti: {vreme_bez_niti}s");
                                    Console.WriteLine($"Vreme sa nitima: {vreme_sa_nitima}s");

                                    m_kes.Upisi(kljuc, ret);
                                }

                                response = "";
                                response += "HTTP/1.1 200 OK\r\n";
                                response += "Content-Type: text/html; charset=UTF-8\r\n";
                                response += $"Content-Length: " + ret.Length + "\r\n\r\n";
                                response += ret;

                                writer.Write(response);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Greska tokom pretrage fajlova: " + e.Message);
                            }
                        }
                        else if (metoda == "download")
                        {
                            try
                            {
                                int start = sadrzaj.IndexOf("putanja=");

                                if (start == -1)
                                {
                                    Console.WriteLine("Primljen nevalidan zahtev!");

                                    String html = "";
                                    html += "<html> <head>\n";
                                    html += "<title>Projekat 1 - Rezultat</title>\n";
                                    html += "</head>\n";
                                    html += "<body>\n";
                                    html += "<h1> Nevalidan zahtev!</h1>\n";
                                    html += "</body> </html>\n";

                                    response += "HTTP/1.1 200 OK\r\n";
                                    response += "Content-Type: text/html; charset=UTF-8\r\n";
                                    response += $"Content-Length: " + html.Length + "\r\n\r\n";
                                    response += html;

                                    writer.Write(response);
                                    client.Close();
                                    continue;
                                }

                                String putanja = Uri.UnescapeDataString(sadrzaj.Substring(start + "putanja=".Length));

                                if (!System.IO.File.Exists(putanja))
                                {
                                    throw new IOException("Putanja nije pronadjena.");
                                }

                                String imeFajla = Path.GetFileName(putanja);

                                Console.WriteLine("Preuzet fajl: " + imeFajla);

                                var podaci = System.IO.File.ReadAllBytes(putanja);
                                string header = "";
                                header += "HTTP/1.1 200 OK\r\n";
                                header += "Content-Type: application/octet-stream\r\n";
                                header += $"Content-Disposition: attachment; filename=\"{imeFajla}\"\r\n";
                                header += $"Content-Length: {podaci.Length}\r\n\r\n";

                                byte[] headerBytes = Encoding.ASCII.GetBytes(header);
                                stream.Write(headerBytes, 0, headerBytes.Length);
                                stream.Write(podaci, 0, podaci.Length);
                            }
                            catch (IOException e)
                            {
                                Console.WriteLine("Greska tokom citanja fajla: " + e.Message);
                                string notFound = "";
                                notFound += "HTTP/1.1 404 Not Found\r\n";
                                notFound += "Content-Length: 0\r\n\r\n";

                                byte[] notFoundBytes = Encoding.ASCII.GetBytes(notFound);
                                stream.Write(notFoundBytes, 0, notFoundBytes.Length);

                                return;
                            }
                        }
                        else
                        {
                            String html = "";
                            html += "<html> <head>\n";
                            html += "<title>Projekat 1</title>\n";
                            html += "</head>\n";
                            html += "<body>\n";
                            html += "<h1>Sistemsko programiranje - Projekat 1</h1>\n";
                            html += "<form action=\"/pronadji-fajlove\" method=GET>\n";
                            html += "<p> Pretrazite fajlove servera: <input name=\"kljuc\">\n";
                            html += "<input type=submit value=\"Pretrazi\">\n";
                            html += "</form>\n";
                            html += "</body> </html>\n";

                            response += "HTTP/1.1 200 OK\r\n";
                            response += "Content-Type: text/html; charset=UTF-8\r\n";
                            response += $"Content-Length: " + html.Length + "\r\n\r\n";
                            response += html;

                            writer.Write(response);
                        }
                    }
                    
                    client.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Javio se izuzetak: " + e.Message);
            }
        }
    }
}
