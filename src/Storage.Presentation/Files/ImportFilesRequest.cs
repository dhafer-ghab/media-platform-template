namespace Storage.Presentation.Files;

public sealed record ImportFilesRequest(IReadOnlyList<string> Urls);
