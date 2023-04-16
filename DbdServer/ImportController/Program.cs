using System.Drawing;
using System.Drawing.Text;
using Base.Helper;
using Core.Entities;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Persistence;

const string PerkFileName = "perks.json";
const string BaseImageURL = "https://dbd.tricky.lol";

Console.WriteLine("Import der Movies und Categories in die Datenbank");
await using var unitOfWork = new UnitOfWork();
Console.WriteLine("Clear Tables Perk and Category");
await unitOfWork.Perk.ClearTable();
await unitOfWork.Category.ClearTable();

var perks = await GetPerksFromJsonAsync();

if (perks != null)
{
    await unitOfWork.Perk.AddRangeAsync(perks);
    var count = await unitOfWork.SaveChangesAsync();
    Console.WriteLine($"{count} Perks are imported to the db!");
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
            newPerk.ImageUrl = BaseImageURL + ((string)perk.Value.image)[2..];
            perks.Add(newPerk);
        }
    }

    return perks;
}

async Task<dynamic?> ReadandConvertJsonAsync()
{
    var path = MyFile.GetFullNameInApplicationTree(PerkFileName);
    if (path.IsNullOrEmpty())
        return null;

    var jsonSerializer = new JsonSerializer();
    var json = await File.ReadAllTextAsync(path!);
    return JsonConvert.DeserializeObject(json);
}