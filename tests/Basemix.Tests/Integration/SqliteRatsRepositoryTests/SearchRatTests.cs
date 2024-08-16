using Basemix.Lib.Litters.Persistence;
using Basemix.Lib.Rats;
using Basemix.Lib.Rats.Persistence;
using Basemix.Tests.sdk;
using Bogus;
using Shouldly;

namespace Basemix.Tests.Integration.SqliteRatsRepositoryTests;

public class SearchRatTests(SqliteFixture fixture) : SqliteIntegration(fixture)
{
    private readonly Faker faker = new();
    private readonly SqliteFixture fixture = fixture;
    private readonly SqliteRatsRepository repository = new(fixture.GetConnection);
    private readonly SqliteLittersRepository littersRepository = new(fixture.GetConnection);

    [Fact]
    public async Task Search_rat_returns_rats_matching_term()
    {
        var expectedRat1 = await Rat.Create(this.repository);
        var expectedRat2 = await Rat.Create(this.repository);
        var otherRat = await Rat.Create(this.repository);

        expectedRat1.Name = this.faker.Random.Hash();
        expectedRat2.Name = expectedRat1.Name;
        otherRat.Name = this.faker.Random.Hash();

        await this.repository.UpdateRat(expectedRat1);
        await this.repository.UpdateRat(expectedRat2);
        await this.repository.UpdateRat(otherRat);

        var results = await this.repository.SearchRat(expectedRat1.Name);
        results.ShouldSatisfyAllConditions(
            () => results.Count.ShouldBe(2),
            () => results.ShouldContain(r => r.Id == expectedRat1.Id),
            () => results.ShouldContain(r => r.Id == expectedRat2.Id));
    }

    [Fact]
    public async Task Search_returns_rat_matching_start_of_search_term()
    {
        var rat = await Rat.Create(this.repository);
        rat.Name = this.faker.Random.Hash(40);
        await this.repository.UpdateRat(rat);

        var results = await this.repository.SearchRat(rat.Name[..10]);
        results.ShouldHaveSingleItem().Id.ShouldBe(rat.Id);
    }

    [Fact]
    public async Task Search_returns_rat_matching_end_of_search_term()
    {
        var rat = await Rat.Create(this.repository);
        rat.Name = this.faker.Random.Hash(40);
        await this.repository.UpdateRat(rat);

        var results = await this.repository.SearchRat(rat.Name[29..]);
        results.ShouldHaveSingleItem().Id.ShouldBe(rat.Id);
    }

    [Fact]
    public async Task Search_returns_rat_matching_part_of_search_term()
    {
        var rat = await Rat.Create(this.repository);
        rat.Name = this.faker.Random.Hash(40);
        await this.repository.UpdateRat(rat);

        var results = await this.repository.SearchRat(rat.Name.Substring(10, 10));
        results.ShouldHaveSingleItem().Id.ShouldBe(rat.Id);
    }

    [Fact]
    public async Task Search_is_case_insensitive()
    {
        var rat = await Rat.Create(this.repository);
        rat.Name = this.faker.Random.Hash().ToLower();
        await this.repository.UpdateRat(rat);

        var results = await this.repository.SearchRat(rat.Name.ToUpper());
        results.ShouldHaveSingleItem().Id.ShouldBe(rat.Id);
    }
    
    [Fact]
    public async Task Search_deceased_only_returns_dead_rats()
    {
        var expectedRat = await Rat.Create(this.repository);
        var otherRat = await Rat.Create(this.repository);

        expectedRat.Dead = true;
        otherRat.Dead = false;

        await this.repository.UpdateRat(expectedRat);
        await this.repository.UpdateRat(otherRat);

        var results = await this.repository.SearchRat(deceased: true);
        
        results.ShouldSatisfyAllConditions(
            () => results.ShouldContain(rat => rat.Id == expectedRat.Id),
            () => results.ShouldNotContain(rat => rat.Id == otherRat.Id));

    }

    [Fact]
    public async Task Search_deceased_false_only_returns_rats_not_marked_as_dead()
    {
        var expectedRat = await Rat.Create(this.repository);
        var otherRat = await Rat.Create(this.repository);

        expectedRat.Dead = false;
        otherRat.Dead = true;

        await this.repository.UpdateRat(expectedRat);
        await this.repository.UpdateRat(otherRat);

        var results = await this.repository.SearchRat(deceased: false);
        
        results.ShouldSatisfyAllConditions(
            () => results.ShouldContain(rat => rat.Id == expectedRat.Id),
            () => results.ShouldNotContain(rat => rat.Id == otherRat.Id));
    }

    [Fact]
    public async Task Search_deceased_null_returns_rats_whether_dead_or_alive()
    {
        var expectedRat = await Rat.Create(this.repository);
        var otherRat = await Rat.Create(this.repository);

        expectedRat.Dead = false;
        otherRat.Dead = true;

        await this.repository.UpdateRat(expectedRat);
        await this.repository.UpdateRat(otherRat);

        var results = await this.repository.SearchRat(deceased: null);
        
        results.ShouldSatisfyAllConditions(
            () => results.ShouldContain(rat => rat.Id == expectedRat.Id),
            () => results.ShouldContain(rat => rat.Id == otherRat.Id));
    }
    
    [Fact]
    public async Task Search_owned_only_returns_owned_rats()
    {
        var expectedRat = await Rat.Create(this.repository);
        var otherRat = await Rat.Create(this.repository);

        expectedRat.Owned = true;
        otherRat.Owned = false;

        await this.repository.UpdateRat(expectedRat);
        await this.repository.UpdateRat(otherRat);

        var results = await this.repository.SearchRat(owned: true);
        
        results.ShouldSatisfyAllConditions(
            () => results.ShouldContain(rat => rat.Id == expectedRat.Id),
            () => results.ShouldNotContain(rat => rat.Id == otherRat.Id));

    }
    
    [Fact]
    public async Task Search_owned_false_only_returns_unowned_rats()
    {
        var expectedRat = await Rat.Create(this.repository);
        var otherRat = await Rat.Create(this.repository);

        expectedRat.Owned = false;
        otherRat.Owned = true;

        await this.repository.UpdateRat(expectedRat);
        await this.repository.UpdateRat(otherRat);

        var results = await this.repository.SearchRat(owned: false);
        
        results.ShouldSatisfyAllConditions(
            () => results.ShouldContain(rat => rat.Id == expectedRat.Id),
            () => results.ShouldNotContain(rat => rat.Id == otherRat.Id));
    }
    
    [Fact]
    public async Task Search_owned_null_returns_owned_and_unowned_rats()
    {
        var expectedRat = await Rat.Create(this.repository);
        var otherRat = await Rat.Create(this.repository);

        expectedRat.Owned = false;
        otherRat.Owned = true;

        await this.repository.UpdateRat(expectedRat);
        await this.repository.UpdateRat(otherRat);

        var results = await this.repository.SearchRat(owned: null);
        
        results.ShouldSatisfyAllConditions(
            () => results.ShouldContain(rat => rat.Id == expectedRat.Id),
            () => results.ShouldContain(rat => rat.Id == otherRat.Id));
    }

    [Theory]
    [InlineData(Sex.Buck)]
    [InlineData(Sex.Doe)]
    public async Task Search_sex_returns_matching_sex(Sex sex)
    {
        var expectedRat = await Rat.Create(this.repository);
        var otherRat = await Rat.Create(this.repository);

        expectedRat.Sex = sex;
        otherRat.Sex = this.faker.PickRandom(this.faker.PickNonDefault(except: sex), (Sex?)null);

        await this.repository.UpdateRat(expectedRat);
        await this.repository.UpdateRat(otherRat);

        var results = await this.repository.SearchRat(sex: sex);
        
        results.ShouldSatisfyAllConditions(
            () => results.ShouldContain(rat => rat.Id == expectedRat.Id),
            () => results.ShouldNotContain(rat => rat.Id == otherRat.Id));
    }

    [Fact]
    public async Task Search_sex_null_returns_matching_sex()
    {
        var buck = await Rat.Create(this.repository);
        var doe = await Rat.Create(this.repository);
        var unset = await Rat.Create(this.repository);

        buck.Sex = Sex.Buck;
        doe.Sex = Sex.Doe;

        await this.repository.UpdateRat(buck);
        await this.repository.UpdateRat(doe);

        var results = await this.repository.SearchRat(sex: null);
        
        results.ShouldSatisfyAllConditions(
            () => results.ShouldContain(rat => rat.Id == buck.Id),
            () => results.ShouldContain(rat => rat.Id == doe.Id),
            () => results.ShouldContain(rat => rat.Id == unset.Id));
    }

    [Fact]
    public async Task Search_is_assigned_to_litter_true_returns_only_rats_with_litter_ids()
    {
        var child = await Rat.Create(this.repository);
        var orphan = await Rat.Create(this.repository);
        var litter = await this.littersRepository.GetLitter(await this.littersRepository.CreateLitter());

        await litter!.AddOffspring(this.littersRepository, child);
        
        var results = await this.repository.SearchRat(isAssignedToLitter: true);
        
        results.ShouldSatisfyAllConditions(
            () => results.ShouldContain(rat => rat.Id == child.Id),
            () => results.ShouldNotContain(rat => rat.Id == orphan.Id));
    }

    [Fact]
    public async Task Search_is_assigned_to_litter_false_returns_only_rats_without_litter_ids()
    {
        var child = await Rat.Create(this.repository);
        var orphan = await Rat.Create(this.repository);
        var litter = await this.littersRepository.GetLitter(await this.littersRepository.CreateLitter());

        await litter!.AddOffspring(this.littersRepository, child);
        
        var results = await this.repository.SearchRat(isAssignedToLitter: false);
        
        results.ShouldSatisfyAllConditions(
            () => results.ShouldNotContain(rat => rat.Id == child.Id),
            () => results.ShouldContain(rat => rat.Id == orphan.Id));
    }
    
    [Fact]
    public async Task Search_is_assigned_to_litter_null_returns_rats_with_and_without_parent_litters()
    {
        var child = await Rat.Create(this.repository);
        var orphan = await Rat.Create(this.repository);
        var litter = await this.littersRepository.GetLitter(await this.littersRepository.CreateLitter());

        await litter!.AddOffspring(this.littersRepository, child);
        
        var results = await this.repository.SearchRat(isAssignedToLitter: null);
        
        results.ShouldSatisfyAllConditions(
            () => results.ShouldContain(rat => rat.Id == child.Id),
            () => results.ShouldContain(rat => rat.Id == orphan.Id));
    }
}
