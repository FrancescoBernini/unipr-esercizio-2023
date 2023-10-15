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
    noMoreJsonp = noMoreJsonp.Remove(0, noMoreJsonp.IndexOf('(') + 1);
    noMoreJsonp = noMoreJsonp.Remove(noMoreJsonp.Length - ");".Length);
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

string pathISTATFile = "./Mynascite2021.js";
string pathJsonFile = "./Mynascite2021.json";
string borns = "";
IEnumerable<Baby>? babies = null;
Tuple<IEnumerable<Baby>, IEnumerable<Baby>>? boysAndGirls = null;
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

boysAndGirls = JsonSerializer.Deserialize<Tuple<IEnumerable<Baby>, IEnumerable<Baby>>>(borns);
//babies = babies.Append<IEnumerable<Baby>>(boysAndGirls.Item1).Append<IEnumerable<Baby>>(boysAndGirls.Item2);
Console.WriteLine(
    from b in babies
    orderby b.name
    select $"{b.name}\t{b.gender}\t{b.count}"
);

/*
JsonConverter<string> a;
a.Write(borns,borns, new JsonSerializerOptions() { WriteIndented = true });

borns = Utf8JsonReader.
Console.WriteLine(borns);

clas

    //TODO
    //salvare su file il risultato su file

*/

