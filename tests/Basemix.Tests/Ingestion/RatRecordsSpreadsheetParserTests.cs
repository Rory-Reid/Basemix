using Basemix.Lib.Ingestion;
using Basemix.Lib.Ingestion.RatRecordsSpreadsheet;
using Basemix.Lib.Litters.Persistence;
using Basemix.Lib.Owners.Persistence;
using Basemix.Lib.Rats.Persistence;
using Basemix.Tests.sdk;
using Shouldly;

namespace Basemix.Tests.Ingestion;

public class RatRecordsSpreadsheetParserTests : SqliteIntegration
{
    private readonly SqliteFixture fixture;

    public RatRecordsSpreadsheetParserTests(SqliteFixture fixture) : base(fixture)
    {
        this.fixture = fixture;
        Parser.Configure();
    }
    
    [Fact]
    public async Task Works()
    {
        using var file = new FileStream("/Users/rory/Downloads/spreadsheet for rory.xlsx", FileMode.Open);
        var parser = new Parser();
        var spreadsheet = parser.ParseFile(file);
        var validator = new Validator();
        var result = validator.Validate(spreadsheet);
        var mapper = new DataMapper();
        var mapped = mapper.Map(spreadsheet, new RatIngestionOptions { UserOwnerName = "Rory Reid" });

        var ingestor = new Ingestor(
            new SqliteLittersRepository(this.fixture.GetConnection),
            new SqliteRatsRepository(this.fixture.GetConnection),
            new SqliteOwnersRepository(this.fixture.GetConnection));
        await ingestor.Ingest(mapped);
        
        spreadsheet.Rats.Count.ShouldBeGreaterThan(2);
    }
}