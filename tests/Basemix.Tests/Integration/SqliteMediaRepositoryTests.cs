using Basemix.Lib.Media;
using Basemix.Lib.Media.Persistence;
using Basemix.Tests.sdk;
using Bogus;
using Dapper;
using Shouldly;

namespace Basemix.Tests.Integration;

public class SqliteMediaRepositoryTests(SqliteFixture fixture) : SqliteIntegration(fixture)
{
    private readonly Faker faker = new();
    private readonly SqliteFixture fixture = fixture;

    private DateTimeOffset Now { get; set; } = DateTimeOffset.UtcNow;

    private SqliteMediaRepository CreateRepository() =>
        new(this.fixture.GetMediaConnection, () => this.Now);

    private static MemoryStream CreateTestJpegStream(int width = 100, int height = 100, SkiaSharp.SKColor? color = null)
    {
        using var bitmap = new SkiaSharp.SKBitmap(width, height);
        bitmap.Erase(color ?? SkiaSharp.SKColors.Red);
        using var image = SkiaSharp.SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Jpeg, 85);
        return new MemoryStream(data.ToArray());
    }

    [Fact]
    public async Task Can_save_and_load_photo()
    {
        var repository = this.CreateRepository();
        var id = MediaIds.RatProfilePhoto(this.faker.Random.Long(1, 100000));
        using var stream = CreateTestJpegStream();
        var filename = this.faker.System.FileName("jpg");

        await repository.SavePhoto(id, stream, filename, 50, false);

        var photo = await repository.GetPhoto(id);
        photo.ShouldNotBeNull().ShouldSatisfyAllConditions(
            () => photo.Id.ShouldBe(id),
            () => photo.ImageFormat.ShouldBe("jpeg"),
            () => photo.Data.ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Can_save_and_load_photo_with_compression()
    {
        var repository = this.CreateRepository();
        var id = MediaIds.RatProfilePhoto(this.faker.Random.Long(1, 100000));
        using var stream = CreateTestJpegStream();
        var filename = this.faker.System.FileName("jpg");

        await repository.SavePhoto(id, stream, filename, 50, true);

        var photo = await repository.GetPhoto(id);
        photo.ShouldNotBeNull().ShouldSatisfyAllConditions(
            () => photo.Id.ShouldBe(id),
            () => photo.ImageFormat.ShouldBe("jpeg"),
            () => photo.Data.ShouldNotBeEmpty());
    }

    [Fact]
    public async Task Can_delete_photo()
    {
        var repository = this.CreateRepository();
        var id = MediaIds.RatProfilePhoto(this.faker.Random.Long(1, 100000));
        using var stream = CreateTestJpegStream();
        var filename = this.faker.System.FileName("jpg");

        await repository.SavePhoto(id, stream, filename, 50, false);
        await repository.DeletePhoto(id);

        var photo = await repository.GetPhoto(id);
        photo.ShouldBeNull();
    }

    [Fact]
    public async Task Can_replace_photo()
    {
        var repository = this.CreateRepository();
        var id = MediaIds.RatProfilePhoto(this.faker.Random.Long(1, 100000));

        using var stream1 = CreateTestJpegStream(100, 100, SkiaSharp.SKColors.Red);
        var filename1 = this.faker.System.FileName("jpg");
        await repository.SavePhoto(id, stream1, filename1, 50, false);
        var original = await repository.GetPhoto(id);

        using var stream2 = CreateTestJpegStream(100, 100, SkiaSharp.SKColors.Blue);
        var filename2 = this.faker.System.FileName("jpg");
        await repository.SavePhoto(id, stream2, filename2, 50, false);
        var replaced = await repository.GetPhoto(id);

        original.ShouldNotBeNull();
        replaced.ShouldNotBeNull();
        replaced.Data.ShouldNotBe(original.Data);
    }

    [Fact]
    public async Task Returns_null_for_missing_photo()
    {
        var repository = this.CreateRepository();
        var id = MediaIds.RatProfilePhoto(this.faker.Random.Long(1, 100000));

        var photo = await repository.GetPhoto(id);
        photo.ShouldBeNull();
    }

    [Fact]
    public async Task Save_photo_persists_original_filename()
    {
        var repository = this.CreateRepository();
        var id = MediaIds.RatProfilePhoto(this.faker.Random.Long(1, 100000));
        using var stream = CreateTestJpegStream();
        var filename = this.faker.System.FileName("jpg");

        await repository.SavePhoto(id, stream, filename, 50, false);

        using var db = this.fixture.GetMediaConnection();
        var row = await db.QuerySingleAsync<MediaRow>(
            "SELECT * FROM media WHERE id = @Id", new { Id = id });

        row.original_filename.ShouldBe(filename);
    }

    [Fact]
    public async Task Save_photo_persists_created_at_from_injected_time()
    {
        this.Now = new DateTimeOffset(2024, 6, 15, 12, 30, 0, TimeSpan.Zero);
        var expectedUnixSeconds = this.Now.ToUnixTimeSeconds();

        var repository = this.CreateRepository();
        var id = MediaIds.RatProfilePhoto(this.faker.Random.Long(1, 100000));
        using var stream = CreateTestJpegStream();
        var filename = this.faker.System.FileName("jpg");

        await repository.SavePhoto(id, stream, filename, 50, false);

        using var db = this.fixture.GetMediaConnection();
        var row = await db.QuerySingleAsync<MediaRow>(
            "SELECT * FROM media WHERE id = @Id", new { Id = id });

        row.created_at.ShouldBe(expectedUnixSeconds);
    }

    [Fact]
    public async Task Replace_photo_updates_filename_and_created_at()
    {
        this.Now = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var repository = this.CreateRepository();
        var id = MediaIds.RatProfilePhoto(this.faker.Random.Long(1, 100000));

        using var stream1 = CreateTestJpegStream();
        var filename1 = "original-photo.jpg";
        await repository.SavePhoto(id, stream1, filename1, 50, false);

        this.Now = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var expectedUnixSeconds = this.Now.ToUnixTimeSeconds();

        using var stream2 = CreateTestJpegStream(100, 100, SkiaSharp.SKColors.Blue);
        var filename2 = "replacement-photo.jpg";
        await repository.SavePhoto(id, stream2, filename2, 50, false);

        using var db = this.fixture.GetMediaConnection();
        var row = await db.QuerySingleAsync<MediaRow>(
            "SELECT * FROM media WHERE id = @Id", new { Id = id });

        row.ShouldSatisfyAllConditions(
            () => row.original_filename.ShouldBe(filename2),
            () => row.created_at.ShouldBe(expectedUnixSeconds));
    }

    [Fact]
    public async Task Delete_photo_removes_row_from_database()
    {
        var repository = this.CreateRepository();
        var id = MediaIds.RatProfilePhoto(this.faker.Random.Long(1, 100000));
        using var stream = CreateTestJpegStream();
        var filename = this.faker.System.FileName("jpg");

        await repository.SavePhoto(id, stream, filename, 50, false);
        await repository.DeletePhoto(id);

        using var db = this.fixture.GetMediaConnection();
        var row = await db.QuerySingleOrDefaultAsync<MediaRow>(
            "SELECT * FROM media WHERE id = @Id", new { Id = id });

        row.ShouldBeNull();
    }

    private record MediaRow(
        string id,
        string image_format,
        string compression,
        long original_width,
        long original_height,
        long stored_width,
        long stored_height,
        long size_bytes,
        byte[] data,
        string original_filename,
        long created_at);
}
