using Riok.Mapperly.Abstractions;
using Storage.Application.Files;
using Storage.Application.Files.Commands.ImportFilesFromUrls;

namespace Storage.Presentation.Files;

[Mapper]
internal static partial class FileMapper
{
    internal static partial FileResponse ToResponse(this FileDto dto);

    internal static partial ImportFilesFromUrlsCommand ToCommand(this ImportFilesRequest request);
}
