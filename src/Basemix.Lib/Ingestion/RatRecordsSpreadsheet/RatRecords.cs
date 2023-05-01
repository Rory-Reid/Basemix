namespace Basemix.Lib.Ingestion.RatRecordsSpreadsheet;

public record RatRecords(IReadOnlyList<RatRow> Rats, IReadOnlyList<LitterRow> Litters);