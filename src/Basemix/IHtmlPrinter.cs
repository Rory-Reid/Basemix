namespace Basemix;

public interface IHtmlPrinter
{
    Task PrintHtmlAsync(string html, string jobName);
}
