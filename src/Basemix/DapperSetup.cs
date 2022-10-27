namespace Basemix;

public static class DapperSetup
{
    public static void Configure()
    {
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
    }
}