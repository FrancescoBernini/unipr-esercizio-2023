using Esercizio;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;

async Task<string> GetBornAsync(string url, CancellationToken cancellationToken = default)
{
    using HttpClient client = new();

    HttpResponseMessage response = await client.SendAsync(new(HttpMethod.Get, url), cancellationToken);

    await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
    using StreamReader reader = new(stream, leaveOpen: true);
    string content = await reader.ReadToEndAsync(cancellationToken);

    return content;
}

string JsonpToJson(string noMoreJsonp)
{
    // rimuovo callback
    noMoreJsonp = noMoreJsonp.Remove(0, noMoreJsonp.IndexOf('(') + 1);
    // rimuovo years:2021
    noMoreJsonp = noMoreJsonp.Remove(noMoreJsonp.IndexOf('"'), noMoreJsonp.IndexOf(','));
    // rimuovo tonda e ; della callback
    noMoreJsonp = noMoreJsonp.Remove(noMoreJsonp.LastIndexOf(')'));// Deserializza la stringa JSON in un oggetto JSON
    
    JsonDocument document = JsonDocument.Parse(noMoreJsonp);

    noMoreJsonp = JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions
    {
        WriteIndented = true
    });
    

    return noMoreJsonp;
}

async Task SaveFileAsync(string path, string text, CancellationToken cancellationToken = default)
{
    await using FileStream file = File.Open(path, FileMode.CreateNew, FileAccess.Write);

    await using (StreamWriter writer = new(file, leaveOpen: true))
    {
        await writer.WriteLineAsync(text);

        await writer.FlushAsync();
    }

    // Forza un flush del buffer di scrittura.
    await file.FlushAsync(cancellationToken);
}

async Task<string> ReadFileAsync(string path, CancellationToken cancellationToken = default)
{
    await using FileStream file = File.OpenRead(path);

    using StreamReader reader = new(file, leaveOpen: true);

    return await reader.ReadToEndAsync(cancellationToken);
}

string pathISTATFile = "./nascite2021.js";
string pathJsonFile = "./nascite2021.json";
string borns = "";
IEnumerable<Baby>? babies = null;
Dictionary<string, IEnumerable<Baby>>? boysAndGirls = null;

if (!File.Exists(pathISTATFile))
{
    borns = await GetBornAsync("https://www.istat.it/ws/nati/index2021.php?type=list&limit=137&year=2021", CancellationToken.None);

    await SaveFileAsync(pathISTATFile, borns, CancellationToken.None);
}
else
{
    borns = await ReadFileAsync(pathISTATFile, CancellationToken.None);
}

borns = JsonpToJson(borns);

if (!File.Exists(pathJsonFile))
{
    await SaveFileAsync(pathJsonFile, borns, CancellationToken.None);
}
else
{
    borns = await ReadFileAsync(pathJsonFile, CancellationToken.None);
}

boysAndGirls = JsonSerializer.Deserialize<Dictionary<string, IEnumerable<Baby>>>(borns);

babies = boysAndGirls["0"];
babies = babies.Union(boysAndGirls["1"]);

IEnumerable<string> res =
    from b in babies
    orderby b.name
    select $"{b.name + (b.name.Length < 8 ? "\t\t" : "\t")}{b.gender}\t{b.count}";

foreach (var item in res)
{
    Console.WriteLine(item);
}
/*
JsonConverter<string> a;
a.Write(borns,borns, new JsonSerializerOptions() { WriteIndented = true });

borns = Utf8JsonReader.
Console.WriteLine(borns);

clas

    //TODO
    //salvare su file il risultato su file

*/

