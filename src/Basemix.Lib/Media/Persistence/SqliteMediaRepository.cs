using System.IO.Compression;
using Dapper;

namespace Basemix.Lib.Media.Persistence;

public class SqliteMediaRepository(GetMediaDatabase getDatabase, NowDateTimeOffset now) : IMediaRepository
{
    private readonly GetMediaDatabase getDatabase = getDatabase;
    private readonly NowDateTimeOffset now = now;

    public async Task SavePhoto(string id, Stream imageStream, string originalFilename, int maxResolution, bool compress)
    {
        // Buffer into a MemoryStream to guarantee seekability —
        // some platform streams (e.g. MacCatalyst's SecurityScopedStream) don't support seeking.
        using var buffered = new MemoryStream();
        await imageStream.CopyToAsync(buffered);
        buffered.Position = 0;

        var processed = ImageProcessor.ResizeImage(buffered, maxResolution);

        var data = compress ? Compress(processed.Data) : processed.Data;
        var compression = compress ? "deflate" : "none";

        using var db = this.getDatabase();

        // INSERT OR REPLACE so that re-uploading to the same id (e.g. replacing a rat's
        // existing photo) is a single atomic statement — no separate delete-then-insert.
        await db.ExecuteAsync(
            """
            INSERT OR REPLACE INTO media (id, image_format, compression, original_width, original_height,
                                          stored_width, stored_height, size_bytes, data, original_filename, created_at)
            VALUES (@Id, @ImageFormat, @Compression, @OriginalWidth, @OriginalHeight,
                    @StoredWidth, @StoredHeight, @SizeBytes, @Data, @OriginalFilename, @CreatedAt)
            """,
            new
            {
                Id = id,
                ImageFormat = processed.Format,
                Compression = compression,
                processed.OriginalWidth,
                processed.OriginalHeight,
                processed.StoredWidth,
                processed.StoredHeight,
                SizeBytes = data.Length,
                Data = data,
                OriginalFilename = originalFilename,
                CreatedAt = this.now().ToUnixTimeSeconds()
            });
    }

    public async Task<RatPhoto?> GetPhoto(string id)
    {
        using var db = this.getDatabase();

        var row = await db.QuerySingleOrDefaultAsync<PersistedMedia>(
            """
            SELECT id, image_format, compression, stored_width, stored_height, data
            FROM media
            WHERE id = @Id
            """,
            new { Id = id });

        if (row == null)
        {
            return null;
        }

        var data = row.Compression == "deflate" ? Decompress(row.Data) : row.Data;

        return new RatPhoto
        {
            Id = row.Id,
            ImageFormat = row.ImageFormat,
            StoredWidth = row.StoredWidth,
            StoredHeight = row.StoredHeight,
            Data = data
        };
    }

    public async Task DeletePhoto(string id)
    {
        using var db = this.getDatabase();

        await db.ExecuteAsync(
            "DELETE FROM media WHERE id = @Id",
            new { Id = id });
    }

    private static byte[] Compress(byte[] data)
    {
        using var output = new MemoryStream();
        using (var deflate = new DeflateStream(output, CompressionLevel.Optimal))
        {
            deflate.Write(data, 0, data.Length);
        }

        return output.ToArray();
    }

    private static byte[] Decompress(byte[] data)
    {
        using var input = new MemoryStream(data);
        using var deflate = new DeflateStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        deflate.CopyTo(output);
        return output.ToArray();
    }

    private class PersistedMedia
    {
        public string Id { get; init; } = null!;
        public string ImageFormat { get; init; } = null!;
        public string? Compression { get; init; }
        public int StoredWidth { get; init; }
        public int StoredHeight { get; init; }
        public byte[] Data { get; init; } = null!;
    }
}
