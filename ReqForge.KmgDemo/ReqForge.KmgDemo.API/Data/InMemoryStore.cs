using ReqForge.KmgDemo.Models;

namespace ReqForge.KmgDemo.Data;

public class InMemoryStore
{
    private readonly object _sync = new();

    public List<Field> Fields { get; } =
    [
        new Field { Id = 1, Name = "Тенгиз", Region = "Атырауская область", IsActive = true },
        new Field { Id = 2, Name = "Кашаган", Region = "Каспийский шельф", IsActive = true },
        new Field { Id = 3, Name = "Карачаганак", Region = "Западно-Казахстанская область", IsActive = true }
    ];

    public Field? GetField(int id) => Fields.FirstOrDefault(f => f.Id == id);

    public Field AddField(string name, string region, bool isActive)
    {
        lock (_sync)
        {
            var id = Fields.Count == 0 ? 1 : Fields.Max(f => f.Id) + 1;
            var field = new Field { Id = id, Name = name, Region = region, IsActive = isActive };
            Fields.Add(field);
            return field;
        }
    }

    public bool TryUpdateField(int id, string name, string region, bool isActive, out Field? field)
    {
        lock (_sync)
        {
            field = Fields.FirstOrDefault(f => f.Id == id);
            if (field == null)
                return false;

            field.Name = name;
            field.Region = region;
            field.IsActive = isActive;
            return true;
        }
    }

    public bool TryDeleteField(int id)
    {
        lock (_sync)
        {
            var field = Fields.FirstOrDefault(f => f.Id == id);
            if (field == null)
                return false;

            Fields.Remove(field);
            return true;
        }
    }
}
