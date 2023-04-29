using Base.Helper;
using Core.Entities;
using HtmlAgilityPack;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Persistence;

const string PERK_FILE_NAME = "perks.json";
const string SURVIVOR_URL = "https://deadbydaylight.fandom.com/wiki/Survivor_Perks";
const string KILLER_URL = "https://deadbydaylight.fandom.com/wiki/Killer_Perks";

Console.WriteLine("Import der Movies und Categories in die Datenbank");
await using var unitOfWork = new UnitOfWork();
Console.WriteLine("Clear Tables Perk and Category");
await unitOfWork.Perk.ClearTable();
await unitOfWork.Category.ClearTable();

var perks = await ParsePerks(KILLER_URL,"killer");
perks.AddRange(await ParsePerks(SURVIVOR_URL, "survivor"));
await AddCategoriesToPerksAsync(perks);

if (perks != null)
{
    await unitOfWork.Perk.AddRangeAsync(perks);
    await unitOfWork.SaveChangesAsync();
    Console.WriteLine($"{perks.Count} Perks are imported to the db!");
}
else
{
    Console.WriteLine("No perks were read in.");
}

// https://github.com/MrTipson/otz-builds/blob/master/build/getPerks.js
async Task AddCategoriesToPerksAsync(List<Perk> perks)
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
                        perks.Single(p => String.Equals(p.Name, perkName, StringComparison.OrdinalIgnoreCase))?.Categories.Add(newCategory);
                        Console.WriteLine(newCategory);
                    }
                    else
                    {
                        //await Console.Out.WriteLineAsync(perk.Value.name.ToString());
                        //try
                        //{
                            perks.Single(p => String.Equals(p.Name, perkName, StringComparison.OrdinalIgnoreCase))?.Categories
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
    var noCategory = perks.Where(p => p.Categories.Count == 0).ToList();
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

async Task<List<Perk>> ParsePerks(string url, string role)
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

    // Grab all rows in table
    var tbodyNode = htmlDocument.DocumentNode.SelectSingleNode("//tbody");
    var perks = tbodyNode.Descendants("tr").Skip(1).Select(tr =>
    {
        var imageUrl = tr.Descendants("a").ElementAtOrDefault(0)?.GetAttributeValue("href", "").Replace(@"/revision/latest.+", "");
        var name = tr.Descendants("a").ElementAtOrDefault(1)?.InnerText.Replace("&amp;", "&").Replace("&nbsp;", " ").Replace("Barbecue & Chilli", "Barbecue & Chili").Replace("'","’").Replace("é", "e").Replace("à","a");
        var formattedPerkDescNode = tr.Descendants("div").FirstOrDefault(div => div.HasClass("formattedPerkDesc"));
        var description = formattedPerkDescNode?.InnerText;
        return new Perk { ImageUrl = imageUrl, Name = name, Description = description, Role = role };
    }).ToList();

    //var tbodyNode = htmlDocument.DocumentNode.SelectSingleNode("//tbody");
    //var trNodes = tbodyNode.Elements("tr").Skip(1);
    //var perks = new List<Perk>();
    //var count = 0;
    //foreach (var x in trNodes)
    //{
    //    var imageUrl = x.Descendants("a").ElementAtOrDefault(0)?.GetAttributeValue("href", "").Replace(@"/revision/latest.+", "");
    //    var name = x.Descendants("a").ElementAtOrDefault(1)?.GetAttributeValue("title", "");
    //    var tmp = x.SelectNodes("//div[@class='formattedPerkDesc']").ElementAtOrDefault(count);
    //    count++;
    //    var description = x.SelectNodes("//div[@class='formattedPerkDesc']").ElementAtOrDefault(count)?.InnerText;
    //    var perk = new Perk { ImageUrl = imageUrl, Name = name, Description = description, Role = role };
    //    perks.Add(perk);
    //}


    //var perks = htmlDocument.DocumentNode.SelectSingleNode("//tbody").Elements("tr").Skip(1).Select(x =>
    //{
    //    // Remap each row into object
    //    return new Perk
    //    {
    //        ImageUrl = x.Descendants("a").ElementAtOrDefault(0)?.GetAttributeValue("href", "").Replace(@"/revision/latest.+", "")!,
    //        Name = x.Descendants("a").ElementAtOrDefault(1)?.GetAttributeValue("title", "")!,
    //        Description = Uri.EscapeUriString(x.Descendants(".formattedPerkDesc").FirstOrDefault()?.InnerHtml?.Replace("/wiki/", "https://deadbydaylight.fandom.com/wiki/")),
    //        Role = role
    //    };
    //}).ToList();

    return perks;
}