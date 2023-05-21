using Basemix.Lib.Rats;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Basemix.Components;

public partial class RatSearchModal
{
    [Parameter] public bool ShowSearch { get; set; }
    [Parameter] public string SearchTerm { get; set; } = null!;
    [Parameter] public EventCallback<MouseEventArgs> OnSearch { get; set; }
    [Parameter] public Func<RatSearchResult, Task> OnResultSet { get; set; } = null!;
    [Parameter] public List<RatSearchResult> SearchResults { get; set; } = null!;
}