using System.Text.Json;
using System.Text.RegularExpressions;

namespace Host.WebApi.ArtworkViews;

internal sealed partial class ArtworkSlugCatalog
{
    private readonly HashSet<string> _slugs;

    public ArtworkSlugCatalog()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "ArtworkViews", "artwork-slugs.json");
        var slugs = JsonSerializer.Deserialize<string[]>(File.ReadAllText(path)) ?? [];
        if (slugs.Length == 0 ||
            slugs.Any(slug => slug.Length is < 1 or > 120 || !SlugPattern().IsMatch(slug)))
        {
            throw new InvalidOperationException("Artwork slug manifest is empty or contains invalid slugs.");
        }

        _slugs = new HashSet<string>(slugs, StringComparer.Ordinal);
        if (_slugs.Count != slugs.Length)
        {
            throw new InvalidOperationException("Artwork slug manifest contains duplicate slugs.");
        }
    }

    public IReadOnlyCollection<string> Slugs => _slugs;

    public bool Contains(string slug) => _slugs.Contains(slug);

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex SlugPattern();
}
