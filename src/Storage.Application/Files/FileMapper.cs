using Riok.Mapperly.Abstractions;
using Storage.Domain;

namespace Storage.Application.Files;

[Mapper]
internal static partial class FileMapper
{
    internal static partial FileDto ToDto(this MediaAsset mediaAsset);
}
