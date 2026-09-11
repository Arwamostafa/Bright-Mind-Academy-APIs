using Domain.Common;

namespace Application.Services.Implementation;


internal static class FileSignatureValidator
{
    private static readonly Dictionary<string, (int Offset, byte[] Signature)[]> Signatures =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = [(0, [0xFF, 0xD8, 0xFF])],
            [".jpeg"] = [(0, [0xFF, 0xD8, 0xFF])],
            [".png"] = [(0, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A])],
            [".pdf"] = [(0, [0x25, 0x50, 0x44, 0x46])], // %PDF
            [".mp4"] = [(4, [0x66, 0x74, 0x79, 0x70])], // "ftyp" box
            [".mov"] = [(4, [0x66, 0x74, 0x79, 0x70])],
            [".avi"] = [(0, [0x52, 0x49, 0x46, 0x46])], // RIFF
            [".mkv"] = [(0, [0x1A, 0x45, 0xDF, 0xA3])],
            [".docx"] = [(0, [0x50, 0x4B, 0x03, 0x04])], // zip-based (PK..)
        };

    public static async Task<bool> MatchesAsync(IFileUpload file, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(file.FileName);
        if (!Signatures.TryGetValue(extension, out var candidates))
            return true; // no known signature for this extension - nothing to check against

        var maxBytesNeeded = candidates.Max(c => c.Offset + c.Signature.Length);
        var buffer = new byte[maxBytesNeeded];

        await using var stream = file.OpenReadStream();
        var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);

        return candidates.Any(c =>
            bytesRead >= c.Offset + c.Signature.Length &&
            buffer.AsSpan(c.Offset, c.Signature.Length).SequenceEqual(c.Signature));
    }
}
