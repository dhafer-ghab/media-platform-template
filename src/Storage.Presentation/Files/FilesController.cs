using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedKernal.Extensions;
using Storage.Application.Files.Commands.DeleteFile;
using Storage.Application.Files.Commands.ImportFilesFromUrls;
using Storage.Application.Files.Commands.UploadFile;
using Storage.Application.Files.Queries.GetAllFiles;
using Storage.Application.Files.Queries.GetFileById;
using Storage.Application.Files.Queries.GetFileDownloadUrl;
using Storage.Presentation.Authorization;

namespace Storage.Presentation.Files;

[ApiController]
[Authorize]
[Route("api/files")]
public sealed class FilesController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = StoragePolicies.RequireAdmin)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        var command = new UploadFileCommand(
            file.OpenReadStream(),
            file.FileName,
            file.ContentType,
            file.Length);

        var result = await sender.Send(command, ct);

        return result.Match(
            id => Created($"/api/files/{id}", new { id })
        );
    }

    [HttpPost("import")]
    [Authorize(Policy = StoragePolicies.RequireAdmin)]
    public async Task<IActionResult> Import([FromBody] ImportFilesRequest request, CancellationToken ct)
    {
        var result = await sender.Send(request.ToCommand(), ct);

        return result.Match(
            queued => Accepted(new { queued })
        );
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = StoragePolicies.RequireAdmin)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetFileByIdQuery(id), ct);

        return result.Match(
            file => Ok(file.ToResponse())
        );
    }

    [HttpGet("{id:guid}/download-url")]
    [Authorize(Policy = StoragePolicies.RequireAdmin)]
    public async Task<IActionResult> GetDownloadUrl(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetFileDownloadUrlQuery(id), ct);

        return result.Match(
            url => Ok(new { url })
        );
    }

    [HttpGet]
    [Authorize(Policy = StoragePolicies.RequireAdmin)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await sender.Send(new GetAllFilesQuery(), ct);

        return result.Match(
            files => Ok(files.Select(f => f.ToResponse()))
        );
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = StoragePolicies.RequireAdmin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteFileCommand(id), ct);

        return result.Match(
            _ => (IActionResult)NoContent()
        );
    }
}
