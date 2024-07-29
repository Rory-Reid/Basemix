using Basemix.Lib.Settings.Persistence;
using Basemix.Tests.sdk;
using Bogus;
using Shouldly;

namespace Basemix.Tests.Integration;

public class SqliteOptionsRepositoryTests : SqliteIntegration
{
    private readonly SqliteOptionsRepository repository;

    public SqliteOptionsRepositoryTests(SqliteFixture fixture) : base(fixture)
    {
        this.repository = new SqliteOptionsRepository(fixture.GetConnection);
    }
    
    [Fact]
    public async Task Get_death_reasons_returns_all_default_reasons_in_alphabetical_order()
    {
        var expected = new List<DeathReason>
        {
            new(11, "Abscess"),
            new(12, "Abscess - Facial"),
            new(1, "Accident"),
            new(14, "Birthing Difficulties"),
            new(19, "Heart"),
            new(20, "Heart - Heart Failure"),
            new(24, "Infection"),
            new(25, "Infection - Systemic"),
            new(29, "Malocclusion"),
            new(21, "Neurological"),
            new(22, "Neurological - Pituitary/Brain tumour"),
            new(23, "Neurological - Stroke"),
            new(27, "Operation - Complications"),
            new(28, "Operation - Post-op complications"),
            new(13, "Pregnancy"),
            new(17, "Renal"),
            new(18, "Renal - Kidney Failure"),
            new(7, "Respiratory"),
            new(9, "Respiratory - Acute"),
            new(8, "Respiratory - Chronic"),
            new(10, "Respiratory - Consolidated Lungs"),
            new(26, "Sudden Death"),
            new(2, "Tumor/Mass"),
            new(3, "Tumor/Mass - Abdominal"),
            new(6, "Tumor/Mass - Bladder"),
            new(4, "Tumor/Mass - Mammary"),
            new(5, "Tumor/Mass - Zymbal's Gland"),
            new(15, "Urinary Tract"),
            new(16, "Urinary Tract - Infection"),
            new(30, "Virus"),
            new(32, "Virus - SDAV"),
            new(31, "Virus - Sendai"),
        };

        var reasons = await this.repository.GetDeathReasons();
        
        reasons.ShouldBeEquivalentTo(expected);
    }
}