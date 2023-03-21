namespace Basemix.Lib;

public static class DapperSetup
{
    public static void Configure()
    {
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
    }
}