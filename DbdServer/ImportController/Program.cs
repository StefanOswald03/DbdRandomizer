using System.Drawing;
using System.Drawing.Text;
using Base.Helper;
using Core.Entities;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore.Storage.Internal;
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
async Task<IList<Perk>?> GetPerksFromJsonAsync()
{
    dynamic? jsonObject = await ReadandConvertJsonAsync();
    if (jsonObject == null)
        return null;

    var perks = new List<Perk>();
    if (jsonObject != null)
    {
        var categoryList = new List<Category>();
        foreach (var perk in jsonObject)
        {
            if (perk == null)
                continue;

            Perk newPerk = new Perk();

            var currentCategories = perk.Value.categories;
            if (currentCategories != null)
            {
                foreach (var cat in currentCategories)
                {
                    var newCategory = cat.ToString();

                    if (!categoryList.Any(x => x.Name == newCategory))
                    {
                        categoryList.Add(new Category()
                        {
                            Name = newCategory,
                            Role = perk.Value.role
                    });
                        Console.WriteLine(newCategory);
                    }

                    newPerk.Categories.Add(categoryList.Single(c => c.Name == newCategory));
                }
            }
            
            newPerk.Name = perk.Value.name;
            newPerk.Description = perk.Value.description;
            newPerk.Role = perk.Value.role;
            perks.Add(newPerk);
        }
    }

    return perks;
}

async Task<dynamic?> ReadandConvertJsonAsync()
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
        var name = tr.Descendants("a").ElementAtOrDefault(1)?.InnerText.Replace("&amp;","&");
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