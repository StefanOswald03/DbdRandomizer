using Base.Helper;
using Core.Entities;
using HtmlAgilityPack;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Persistence;

const string PERK_FILE_NAME = "perks.json";
const string ENGLISH_PERKS_URL = "https://deadbydaylight.fandom.com/wiki/Perks";
const string GERMAN_PERKS_URL = "https://deadbydaylight-de.gamepedia.com/Perks";
Dictionary<string, int> roles = new Dictionary<string, int>()
{
    { "survivor", 1 },
    { "killer", 2 }
};

Console.WriteLine("Import der Movies und Categories in die Datenbank");
await using var unitOfWork = new UnitOfWork();
Console.WriteLine("Delete Database");
await unitOfWork.DeleteDatabaseAsync();
Console.WriteLine("Recreate Database");
await unitOfWork.MigrateDatabaseAsync();

var (englishPerkTranslations, perks) = await ParsePerks(ENGLISH_PERKS_URL, "killer");
var (survivorPerkTranslations, survivorPerks) = await ParsePerks(ENGLISH_PERKS_URL, "survivor");
englishPerkTranslations.AddRange(survivorPerkTranslations);
perks.AddRange(survivorPerks);
await AddCategoriesToPerksAsync(englishPerkTranslations);

if (englishPerkTranslations != null)
{
    await unitOfWork.PerkTranslation.AddRangeAsync(englishPerkTranslations);
    await unitOfWork.SaveChangesAsync();
    Console.WriteLine($"{englishPerkTranslations.Count} Perks are imported to the db!");
}
else
{
    Console.WriteLine("No perks were read in.");
}

// https://github.com/MrTipson/otz-builds/blob/master/build/getPerks.js
async Task AddCategoriesToPerksAsync(List<PerkTranslation> perks)
{
    dynamic? jsonObject = await ReadAndConvertJsonAsync();
    if (jsonObject == null)
        return;
    var categoryList = new List<Category>();
    if (jsonObject != null)
    {
        foreach (var perk in jsonObject)
        {
            if (perk == null)
                continue;

            var currentCategories = perk.Value.categories;
            if (currentCategories != null)
            {
                foreach (var cat in currentCategories)
                {
                    var newCategory = new Category()
                    {
                        Name = cat.ToString(),
                        Role = perk.Value.role
                    };
                    var perkName = perk.Value.name.ToString().Replace("'", "’").Replace("&nbsp;", " ").Replace("Hex: Blood Favor", "Hex: Blood Favour");

                    if (!categoryList.Any(x => x.Name == newCategory.Name))
                    {
                        categoryList.Add(newCategory);
                        //await Console.Out.WriteLineAsync(perk.Value.name.ToString());
                        perks.Single(p => String.Equals(p.Name, perkName, StringComparison.OrdinalIgnoreCase))?.Perk!.Categories.Add(newCategory);
                        //Console.WriteLine(newCategory);
                    }
                    else
                    {
                        //await Console.Out.WriteLineAsync(perk.Value.name.ToString());
                        //try
                        //{
                            perks.Single(p => String.Equals(p.Name, perkName, StringComparison.OrdinalIgnoreCase))?.Perk!.Categories
                            .Add(categoryList.Single(c => c.Name == newCategory.Name));
                        //}
                        //catch(Exception ex)
                        //{
                        //    perks.ForEach(p => DisplayStringDifference(p.Name, perkName));
                        //    throw ex;
                        //}
                    }
                }
            }
        }
    }
}

static void DisplayStringDifference(string string1, string string2)
{
    int minLength = Math.Min(string1.Length, string2.Length);
    int maxLength = Math.Max(string1.Length, string2.Length);

    for (int i = 0; i < minLength; i++)
    {
        if (string1[i] != string2[i])
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(string1[i]);
            Console.ResetColor();
        }
        else
        {
            Console.Write(string1[i]);
        }
    }

    if (maxLength > minLength)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write(string1.Length > string2.Length ? string1.Substring(minLength) : string2.Substring(minLength));
        Console.ResetColor();
    }

    Console.WriteLine();
}

async Task<dynamic?> ReadAndConvertJsonAsync()
{
    var path = MyFile.GetFullNameInApplicationTree(PERK_FILE_NAME);
    if (path.IsNullOrEmpty())
        return null;

    var jsonSerializer = new JsonSerializer();
    var json = await File.ReadAllTextAsync(path!);
    return JsonConvert.DeserializeObject(json);
}

async Task<(List<PerkTranslation>, List<Perk>)> ParsePerks(string url, string role)
{
    var httpClient = new HttpClient();
    var html = await httpClient.GetStringAsync(url);
    var htmlDocument = new HtmlDocument();
    htmlDocument.LoadHtml(html);

    // Remove mini icons next to links
    var nodesToRemove = new List<HtmlNode>();
    foreach (var x in htmlDocument.DocumentNode.Descendants().Where(n => n.HasClass("formattedPerkDesc")))
    {
        foreach (var y in x.Descendants("span").Where(n => n.GetAttributeValue("style", "").Contains("padding")))
        {
            nodesToRemove.Add(y);
        }
    }
    foreach (var node in nodesToRemove)
    {
        node.Remove();
    }

    var lal = roles[role];

    // Grab all rows in table
    var tbodyNode = htmlDocument.DocumentNode.SelectSingleNode($"//table[{roles[role]}]/tbody");
    var perks = tbodyNode.Descendants("tr").Skip(1).Select(tr =>
    {
        var imageUrl = tr.Descendants("a").ElementAtOrDefault(0)?.GetAttributeValue("href", "").Replace(@"/revision/latest.+", "");
        return new Perk { ImageUrl = imageUrl, Role = role };
    }).ToList();

    var perkTranslations = tbodyNode.Descendants("tr").Skip(1).Select(tr =>
    {
        var imageUrl = tr.Descendants("a").ElementAtOrDefault(0)?.GetAttributeValue("href", "").Replace(@"/revision/latest.+", "");
        var name = tr.Descendants("a").ElementAtOrDefault(1)?.InnerText.Replace("&amp;", "&").Replace("&nbsp;", " ").Replace("Barbecue & Chilli", "Barbecue & Chili").Replace("'","’").Replace("é", "e").Replace("à","a");
        var formattedPerkDescNode = tr.Descendants("div").FirstOrDefault(div => div.HasClass("formattedPerkDesc"));
        var description = formattedPerkDescNode?.InnerText;
        return new PerkTranslation 
        { 
            Language = "en",
            Name = name, 
            Description = description,
            Perk = perks.Single(p => p.ImageUrl == imageUrl)
        };
    }).ToList();

    return (perkTranslations, perks);
}

async Task AddPerkTranslation(string url, string languageCode, int tableIndexMultiplier) 
{

}