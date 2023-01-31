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
//await unitOfWork.DeleteDatabaseAsync();
//await unitOfWork.MigrateDatabaseAsync();
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


async Task<IList<Perk>?> GetPerksFromJsonAsync()
{
    var path = MyFile.GetFullNameInApplicationTree(PerkFileName);
    if (path.IsNullOrEmpty())
        return null;

    var js = new JsonSerializer();
    var json = await File.ReadAllTextAsync(path!);
    dynamic? jsonObject = JsonConvert.DeserializeObject(json);
    var perks = new List<Perk>();

    if (jsonObject != null)
    {
        
        var categoryList = new List<PerkCategory>();
        foreach (var perk in jsonObject)
        {
            if (perk == null)
                continue;

            Perk p = new Perk();

            var categories = perk.Value.categories;
            if (categories != null)
            {
                foreach (var cat in categories)
                {
                    var newCategory = cat.ToString();

                    if (!categoryList.Any(x => x.Name == newCategory))
                    {
                        categoryList.Add(new PerkCategory()
                        {
                            Name = newCategory,
                            Role = perk.Value.role
                    });
                        Console.WriteLine(newCategory);
                    }

                    p.Categories.Add(categoryList.Single(c => c.Name == newCategory));
                }
            }
            
            p.Name = perk.Value.name;
            p.Description = perk.Value.description;
            p.Role = perk.Value.role;
            var tmpUrl = (string)perk.Value.image;
            p.ImageUrl = BaseImageURL + tmpUrl[2..];
            perks.Add(p);
        }
    }

    return perks;
}