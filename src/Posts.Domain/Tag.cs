using SharedKernal.Entities;
using SharedKernal.Results;

namespace Posts.Domain;

public sealed class Tag : Entity
{
    private Tag(Guid id, string name, string normalizedName) : base(id)
    {
        Name = name;
        NormalizedName = normalizedName;
    }

    private Tag()
    {
        Name = null!;
        NormalizedName = null!;
    }

    public string Name { get; private set; }
    public string NormalizedName { get; private set; }
    public ICollection<Post> Posts { get; private set; } = new List<Post>();

    public static Result<Tag> Create(string? name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().TrimStart('#').Length is 0 or > 50)
            return Error.Validation("Tag.NameInvalid", "Tag name is required and cannot exceed 50 characters.");

        var cleaned = name.Trim().TrimStart('#');
        return new Tag(Guid.NewGuid(), cleaned, Normalize(cleaned));
    }

    public static string Normalize(string value) => value.Trim().TrimStart('#').ToLowerInvariant();
}
