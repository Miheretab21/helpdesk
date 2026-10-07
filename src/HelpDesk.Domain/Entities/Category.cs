using HelpDesk.Domain.Common;

namespace HelpDesk.Domain.Entities;

public class Category : Entity
{
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private Category() { }

    public Category(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category name is required.");
        Name = name.Trim();
    }

    public void Deactivate() => IsActive = false;
}