using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace Basemix.Tests.sdk;

public static class RazorEngine
{
    public static Task InvokeOnInitializedAsync<T>(T page) where T : ComponentBase =>
        (Task)page
            .GetType()
            .GetMethod("OnInitializedAsync" , BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(page, null)!;

    public static Task InvokeOnParametersSetAsync<T>(T page) where T : ComponentBase =>
        (Task) page
            .GetType()
            .GetMethod("OnParametersSetAsync", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(page, null)!;
}