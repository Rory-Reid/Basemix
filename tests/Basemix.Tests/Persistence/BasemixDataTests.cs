using Basemix.Lib.Persistence;
using Bogus;
using Shouldly;

namespace Basemix.Tests.Persistence;

public class BasemixDataTests
{
    private readonly Faker faker = new();

    /// <summary>
    /// On _some_ systems <see cref="Environment.SpecialFolder.Personal"/> was returning the 'wrong' thing and a
    /// breaking change was introduced in dotnet 8 to fix this. See:
    ///   - https://github.com/dotnet/runtime/issues/102110
    ///   - https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/8.0/getfolderpath-unix
    ///
    /// What this means is that on some systems (android phones for example) the "user profile" directory was being used
    ///
    /// Need to preserve the legacy path for migration reasons now.
    /// </summary>
    [Fact]
    public void Get_legacy_db_file_path_returns_user_profile_db_path()
    {
        BasemixData.GetLegacyDbFilePath().ShouldBe(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "basemix/db.sqlite"));
    }
    
    [Fact]
    public void Get_base_directory_returns_personal_basemix_path()
    {
        BasemixData.GetBaseDirectory().ShouldBe(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Personal),
            "basemix"));
    }
    
    [Fact]
    public void Get_db_file_path_returns_personal_directory_path()
    {
        BasemixData.GetDbFilePath().ShouldBe(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Personal),
            "basemix/db.sqlite"));
    }

    [Fact]
    public void Try_move_db__does_nothing_if_legacy_db_not_exists()
    {
        var legacyDb = this.faker.Random.AlphaNumeric(10);
        var newDb = this.faker.Random.AlphaNumeric(10);
        
        BasemixData.TryMoveDb(legacyDb, newDb).ShouldBeFalse();
        
        File.Exists(legacyDb).ShouldBeFalse();
        File.Exists(newDb).ShouldBeFalse();
    }

    [Fact]
    public void Try_move_db_moves_legacy_db_to_new()
    {
        var legacyDb = this.faker.Random.AlphaNumeric(10);
        var newDb = this.faker.Random.AlphaNumeric(10);

        using (var writer = File.CreateText(legacyDb))
        {
            writer.WriteLine(nameof(this.Try_move_db_moves_legacy_db_to_new));
        }
        
        BasemixData.TryMoveDb(legacyDb, newDb).ShouldBeTrue();
        
        File.Exists(legacyDb).ShouldBeFalse();
        File.Exists(newDb).ShouldBeTrue();
    }
    
    [Fact]
    public void Try_move_db_does_not_overwrite_existing_db()
    {
        var legacyDb = this.faker.Random.AlphaNumeric(10);
        var newDb = this.faker.Random.AlphaNumeric(10);

        using (var writer = File.CreateText(legacyDb))
        {
            writer.WriteLine(nameof(this.Try_move_db_does_not_overwrite_existing_db));
        }
        
        using (var writer = File.CreateText(newDb))
        {
            writer.WriteLine(nameof(this.Try_move_db_does_not_overwrite_existing_db));
        }
        
        BasemixData.TryMoveDb(legacyDb, newDb).ShouldBeFalse();
        
        File.Exists(legacyDb).ShouldBeTrue();
        File.Exists(newDb).ShouldBeTrue();
    }
    
    [Fact]
    public void Try_move_db_does_not_error_if_no_legacy_db_exists()
    {
        var legacyDb = this.faker.Random.AlphaNumeric(10);
        var newDb = this.faker.Random.AlphaNumeric(10);

        
        using (var writer = File.CreateText(newDb))
        {
            writer.WriteLine(nameof(this.Try_move_db_does_not_error_if_no_legacy_db_exists));
        }
        
        BasemixData.TryMoveDb(legacyDb, newDb).ShouldBeFalse();
        
        File.Exists(legacyDb).ShouldBeFalse();
        File.Exists(newDb).ShouldBeTrue();
    }
    
    [Fact(Skip = "This test is not portable")]
    public void Get_legacy_db_file_path_returns_my_personal_legacy_path()
    {
        BasemixData.GetLegacyDbFilePath().ShouldBe("/Users/rory/basemix/db.sqlite");
    }

    [Fact(Skip = "This test is not portable")]
    public void Get_base_directory_returns_my_personal_base_directory()
    {
        BasemixData.GetBaseDirectory().ShouldBe("/Users/rory/Documents/basemix");
    }
    
    [Fact(Skip = "This test is not portable")]
    public void Get_db_file_path_returns_my_personal_db_file_path()
    {
        BasemixData.GetDbFilePath().ShouldBe("/Users/rory/Documents/basemix/db.sqlite");
    }
}