using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace NetflixConverter
{
    public class Pelicula
    {
        public string Title { get; set; }
        public string Genre { get; set; }
        public string Premiere { get; set; }
        public int Runtime { get; set; }
        public double ImdbScore { get; set; }
        public string Language { get; set; }
    }

    class Program
    {
        static string csvPath = "NetflixOriginals.csv";
        static string xmlPath = "Peliculas.xml";

        static void Main(string[] args)
        {
            int opcion;
            do
            {
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine(" CONVERTIDOR DE FORMATOS (CSV <-> XML)");
                Console.WriteLine("========================================");
                Console.WriteLine($" Path CSV actual: {csvPath}");
                Console.WriteLine($" Path XML actual: {xmlPath}");
                Console.WriteLine("----------------------------------------");
                Console.WriteLine("--- OPCIONES CSV -> XML ---");
                Console.WriteLine("1. Exportar todo a XML");
                Console.WriteLine("2. Exportar por género a XML");
                Console.WriteLine("3. Exportar por año (igual o mayor) a XML");
                Console.WriteLine("4. Exportar por idioma a XML");
                Console.WriteLine("\n--- OPCIONES XML -> CSV ---");
                Console.WriteLine("5. Exportar todo a CSV");
                Console.WriteLine("6. Exportar por género a CSV");
                Console.WriteLine("7. Exportar por año (igual o mayor) a CSV");
                Console.WriteLine("8. Exportar por idioma a CSV");
                Console.WriteLine("\n--- CONFIGURACIÓN ---");
                Console.WriteLine("9. Configurar ruta del CSV");
                Console.WriteLine("10. Configurar ruta del XML");
                Console.WriteLine("0. Salir");
                Console.WriteLine("========================================");
                Console.Write("Selecciona una opción: ");

                if (!int.TryParse(Console.ReadLine(), out opcion))
                {
                    opcion = -1;
                }

                Console.WriteLine();

                switch (opcion)
                {
                    case 1:
                        ExportarCSVaXML(p => true);
                        break;
                    case 2:
                        FiltrarYExportarCSVaXML(ModoFiltro.Genero);
                        break;
                    case 3:
                        FiltrarYExportarCSVaXML(ModoFiltro.Ano);
                        break;
                    case 4:
                        FiltrarYExportarCSVaXML(ModoFiltro.Idioma);
                        break;
                    case 5:
                        ExportarXMLaCSV(p => true);
                        break;
                    case 6:
                        FiltrarYExportarXMLaCSV(ModoFiltro.Genero);
                        break;
                    case 7:
                        FiltrarYExportarXMLaCSV(ModoFiltro.Ano);
                        break;
                    case 8:
                        FiltrarYExportarXMLaCSV(ModoFiltro.Idioma);
                        break;
                    case 9:
                        Console.Write("Introduce la nueva ruta del CSV: ");
                        csvPath = Console.ReadLine();
                        break;
                    case 10:
                        Console.Write("Introduce la nueva ruta del XML: ");
                        xmlPath = Console.ReadLine();
                        break;
                    case 0:
                        Console.WriteLine("¡Hasta luego!");
                        break;
                    default:
                        Console.WriteLine("Opción no válida.");
                        break;
                }

                if (opcion != 0)
                {
                    Console.WriteLine("\nPresiona cualquier tecla para continuar...");
                    Console.ReadKey();
                }

            } while (opcion != 0);
        }

        enum ModoFiltro { Genero, Ano, Idioma }


        static List<Pelicula> LeerCSV(string ruta)
        {
            List<Pelicula> peliculas = new List<Pelicula>();

            if (!File.Exists(ruta))
            {
                Console.WriteLine($"Error: El archivo CSV no existe en la ruta: {ruta}");
                return peliculas;
            }

            string[] lineas = File.ReadAllLines(ruta);

            for (int i = 1; i < lineas.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lineas[i])) continue;

                string[] campos = ParsearLineaCSV(lineas[i]);

                if (campos.Length >= 6)
                {
                    Pelicula p = new Pelicula
                    {
                        Title = campos[0].Trim(),
                        Genre = campos[1].Trim(),
                        Premiere = campos[2].Trim(),
                        Runtime = int.TryParse(campos[3].Trim(), out int r) ? r : 0,
                        ImdbScore = double.TryParse(campos[4].Trim().Replace('.', ','), out double score) ? score : 0.0,
                        Language = campos[5].Trim()
                    };
                    peliculas.Add(p);
                }
            }

            return peliculas;
        }

        static void GuardarCSV(string ruta, List<Pelicula> peliculas)
        {
            using (StreamWriter sw = new StreamWriter(ruta))
            {
                sw.WriteLine("Title,Genre,Premiere,Runtime,IMDB Score,Language");
                foreach (var p in peliculas)
                {
                    string titulo = p.Title.Contains(",") ? $"\"{p.Title}\"" : p.Title;
                    sw.WriteLine($"{titulo},{p.Genre},{p.Premiere},{p.Runtime},{p.ImdbScore.ToString().Replace(',', '.')},{p.Language}");
                }
            }
            Console.WriteLine($"Exportación finalizada con éxito en: {ruta}");
        }

        static string[] ParsearLineaCSV(string linea)
        {
            List<string> resultado = new List<string>();
            bool enComillas = false;
            string campoActual = "";

            foreach (char c in linea)
            {
                if (c == '"')
                {
                    enComillas = !enComillas;
                }
                else if (c == ',' && !enComillas)
                {
                    resultado.Add(campoActual);
                    campoActual = "";
                }
                else
                {
                    campoActual += c;
                }
            }
            resultado.Add(campoActual);
            return resultado.ToArray();
        }



        static List<Pelicula> LeerXML(string ruta)
        {
            List<Pelicula> peliculas = new List<Pelicula>();

            if (!File.Exists(ruta))
            {
                Console.WriteLine($"Error: El archivo XML no existe en la ruta: {ruta}");
                return peliculas;
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(ruta);

            XmlNodeList nodosPelicula = doc.SelectNodes("//Peliculas/Pelicula");
            if (nodosPelicula != null)
            {
                foreach (XmlNode node in nodosPelicula)
                {
                    Pelicula p = new Pelicula
                    {
                        Title = node["Title"]?.InnerText ?? "",
                        Genre = node["Genre"]?.InnerText ?? "",
                        Premiere = node["Premiere"]?.InnerText ?? "",
                        Runtime = int.TryParse(node["Runtime"]?.InnerText, out int r) ? r : 0,
                        ImdbScore = double.TryParse(node["IMDBScore"]?.InnerText.Replace('.', ','), out double s) ? s : 0.0,
                        Language = node["Language"]?.InnerText ?? ""
                    };
                    peliculas.Add(p);
                }
            }

            return peliculas;
        }

        static void GuardarXML(string ruta, List<Pelicula> peliculas)
        {
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  "
            };

            using (XmlWriter writer = XmlWriter.Create(ruta, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Peliculas"); 

                foreach (var p in peliculas)
                {
                    writer.WriteStartElement("Pelicula");

                    writer.WriteElementString("Title", p.Title);
                    writer.WriteElementString("Genre", p.Genre);
                    writer.WriteElementString("Premiere", p.Premiere);
                    writer.WriteElementString("Runtime", p.Runtime.ToString());
                    writer.WriteElementString("IMDBScore", p.ImdbScore.ToString().Replace(',', '.'));
                    writer.WriteElementString("Language", p.Language);

                    writer.WriteEndElement(); 
                }

                writer.WriteEndElement(); 
                writer.WriteEndDocument();
            }

            Console.WriteLine($"Exportación a XML completada con éxito en: {ruta}");
        }


        static void ExportarCSVaXML(Predicate<Pelicula> filtro)
        {
            List<Pelicula> peliculas = LeerCSV(csvPath);
            List<Pelicula> filtradas = peliculas.FindAll(filtro);
            GuardarXML(xmlPath, filtradas);
        }

        static void ExportarXMLaCSV(Predicate<Pelicula> filtro)
        {
            List<Pelicula> peliculas = LeerXML(xmlPath);
            List<Pelicula> filtradas = peliculas.FindAll(filtro);
            GuardarCSV(csvPath, filtradas);
        }

        static void FiltrarYExportarCSVaXML(ModoFiltro modo)
        {
            List<Pelicula> peliculas = LeerCSV(csvPath);
            if (peliculas.Count == 0) return;

            Predicate<Pelicula> filtro = ObtenerFiltroDeUsuario(peliculas, modo);
            if (filtro != null)
            {
                List<Pelicula> filtradas = peliculas.FindAll(filtro);
                GuardarXML(xmlPath, filtradas);
            }
        }

        static void FiltrarYExportarXMLaCSV(ModoFiltro modo)
        {
            List<Pelicula> peliculas = LeerXML(xmlPath);
            if (peliculas.Count == 0) return;

            Predicate<Pelicula> filtro = ObtenerFiltroDeUsuario(peliculas, modo);
            if (filtro != null)
            {
                List<Pelicula> filtradas = peliculas.FindAll(filtro);
                GuardarCSV(csvPath, filtradas);
            }
        }

        static Predicate<Pelicula> ObtenerFiltroDeUsuario(List<Pelicula> peliculas, ModoFiltro modo)
        {
            switch (modo)
            {
                case ModoFiltro.Genero:
                    var generos = peliculas.Select(p => p.Genre).Distinct().OrderBy(g => g).ToList();
                    Console.WriteLine("--- Lista de Géneros Disponibles ---");
                    for (int i = 0; i < generos.Count; i++)
                    {
                        Console.WriteLine($"{i + 1}. {generos[i]}");
                    }
                    Console.Write("\nSelecciona el número del género deseado: ");
                    if (int.TryParse(Console.ReadLine(), out int idxG) && idxG >= 1 && idxG <= generos.Count)
                    {
                        string sel = generos[idxG - 1];
                        return p => p.Genre.Equals(sel, StringComparison.OrdinalIgnoreCase);
                    }
                    Console.WriteLine("Opción de género inválida.");
                    return null;

                case ModoFiltro.Idioma:
                    var idiomas = peliculas.Select(p => p.Language).Distinct().OrderBy(l => l).ToList();
                    Console.WriteLine("--- Lista de Idiomas Disponibles ---");
                    for (int i = 0; i < idiomas.Count; i++)
                    {
                        Console.WriteLine($"{i + 1}. {idiomas[i]}");
                    }
                    Console.Write("\nSelecciona el número del idioma deseado: ");
                    if (int.TryParse(Console.ReadLine(), out int idxL) && idxL >= 1 && idxL <= idiomas.Count)
                    {
                        string sel = idiomas[idxL - 1];
                        return p => p.Language.Equals(sel, StringComparison.OrdinalIgnoreCase);
                    }
                    Console.WriteLine("Opción de idioma inválida.");
                    return null;

                case ModoFiltro.Ano:
                    Console.Write("Introduce el año a partir del cual deseas filtrar (ej. 2019): ");
                    if (int.TryParse(Console.ReadLine(), out int ano))
                    {
                        return p => ExtraerAno(p.Premiere) >= ano;
                    }
                    Console.WriteLine("Año no válido.");
                    return null;

                default:
                    return null;
            }
        }

        static int ExtraerAno(string fecha)
        {
            if (DateTime.TryParse(fecha, out DateTime dt))
            {
                return dt.Year;
            }
            var match = System.Text.RegularExpressions.Regex.Match(fecha, @"\b\d{4}\b");
            if (match.Success && int.TryParse(match.Value, out int year))
            {
                return year;
            }
            return 0;
        }

    }
}