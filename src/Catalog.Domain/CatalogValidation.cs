namespace Catalog.Domain;

internal static class CatalogValidation
{
    internal static void Id(Guid value, string name)
    {
        if (value == Guid.Empty)
            throw new ArgumentException($"{name} cannot be empty.", name);
    }

    internal static string Required(string? value, int maxLength, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{name} is required.", name);
        if (value.Length > maxLength)
            throw new ArgumentException($"{name} cannot exceed {maxLength} characters.", name);
        return value;
    }

    internal static string? Optional(string? value, int maxLength, string name)
    {
        if (value is not null && string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{name} cannot be empty when supplied.", name);
        if (value?.Length > maxLength)
            throw new ArgumentException($"{name} cannot exceed {maxLength} characters.", name);
        return value;
    }

    internal static string StorageKey(string? value, string name)
    {
        var key = Required(value, 500, name);
        if (key.StartsWith('/') || key.Contains('\\') || key.Contains('%') ||
            key.Contains('?') || key.Contains('#') || key.Contains(':') || key.Any(char.IsControl) ||
            key.Split('/').Any(segment => segment is "" or "." or ".."))
            throw new ArgumentException($"{name} must be a safe relative storage key.", name);
        return key;
    }

    internal static string LocalRootedPath(string? value, int maxLength, string name)
    {
        var path = Required(value, maxLength, name);
        if (path == "/")
            return path;

        if (path != path.Trim() || !path.StartsWith('/') || path.StartsWith("//", StringComparison.Ordinal) ||
            path.Contains('\u005c') || path.Contains('?') || path.Contains('#') || path.Contains('%') ||
            path.Any(char.IsControl) || path.Split('/').Skip(1).Any(segment => segment is "" or "." or ".."))
        {
            throw new ArgumentException(
                $"{name} must be a safe local path rooted with a single '/'.", name);
        }

        return path;
    }

    internal static string Slug(string? value, string name)
    {
        var slug = Required(value, 160, name);
        var previousWasHyphen = false;

        for (var index = 0; index < slug.Length; index++)
        {
            var character = slug[index];
            var isHyphen = character == '-';
            var isLowerAsciiLetter = character is >= 'a' and <= 'z';
            var isDigit = character is >= '0' and <= '9';
            if ((!isLowerAsciiLetter && !isDigit && !isHyphen) ||
                (isHyphen && (index == 0 || index == slug.Length - 1 || previousWasHyphen)))
            {
                throw new ArgumentException(
                    $"{name} must be lowercase ASCII kebab-case (letters and numbers separated by single hyphens).",
                    name);
            }

            previousWasHyphen = isHyphen;
        }

        return slug;
    }

    internal static void NonNegative(long value, string name)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(name, value, $"{name} cannot be negative.");
    }

    internal static void Positive(int value, string name)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(name, value, $"{name} must be positive.");
    }

    internal static void Positive(double value, string name)
    {
        if (!double.IsFinite(value) || value <= 0)
            throw new ArgumentOutOfRangeException(name, value, $"{name} must be finite and positive.");
    }

    internal static (DateTimeOffset PublishedAt, DateTimeOffset UpdatedAt) Timestamps(
        DateTimeOffset publishedAt,
        DateTimeOffset updatedAt)
    {
        if (publishedAt == default)
            throw new ArgumentException("publishedAt is required.", nameof(publishedAt));
        if (updatedAt == default)
            throw new ArgumentException("updatedAt is required.", nameof(updatedAt));
        if (updatedAt < publishedAt)
            throw new ArgumentException("updatedAt cannot precede publishedAt.", nameof(updatedAt));
        return (publishedAt.ToUniversalTime(), updatedAt.ToUniversalTime());
    }

    internal static string[] Tags(IEnumerable<string>? values)
    {
        if (values is null)
            throw new ArgumentNullException(nameof(values), "tags are required.");

        var tags = values.Select(tag => tag?.Trim().ToLowerInvariant()
                ?? throw new ArgumentException("tags cannot contain null values.", nameof(values)))
            .Where(tag => tag.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        if (tags.Length > 100)
            throw new ArgumentException("No more than 100 tags are allowed.", nameof(values));
        if (tags.Any(tag => tag.Length > 100))
            throw new ArgumentException("A tag cannot exceed 100 characters.", nameof(values));
        return tags;
    }
}
