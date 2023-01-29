using System.Drawing.Text;
using Base.Helper;
using Core.Entities;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Persistence;

const string PerkFileName = "perks.json";

Console.WriteLine("Import der Movies und Categories in die Datenbank");
//await using var unitOfWork = new UnitOfWork();
Console.WriteLine("Clear Tables Perk and Category");
//await unitOfWork.Perk.ClearTable();
//await unitOfWork.Category.ClearTable();
await GetPerksFromJsonAsync();

async Task GetPerksFromJsonAsync()
{
    var path = MyFile.GetFullNameInApplicationTree(PerkFileName);
    if (path.IsNullOrEmpty())
        return;

    var js = new JsonSerializer();

    var json = await File.ReadAllTextAsync(path!);
    dynamic jsonObject = JsonConvert.DeserializeObject(json);
    List<Perk> perks = new List<Perk>();
    foreach (var perk in jsonObject)
    {
        Perk p = new Perk();
        p.Name = perk.Value.name;
        p.Description = perk.Value.description;
        p.Role = perk.Value.role;
        perks.Add(p);
    }
}