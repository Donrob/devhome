// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DevHome.Common.Extensions;
using DevHome.Dashboard.Services;
using Microsoft.UI.Xaml;
using Microsoft.Windows.Widgets.Hosts;

namespace DevHome.Dashboard.ComSafeWidgetObjects;

/// <summary>
/// Since WidgetDefinitions are OOP COM objects, we need to wrap them in a safe way to handle COM exceptions
/// that arise when the underlying OOP object vanishes. All WidgetDefinitions should be wrapped in a
/// ComSafeWidgetDefinition and calls to the WidgetDefinition should be done through the ComSafeWidgetDefinition.
/// This class will handle the COM exceptions and get a new OOP WidgetDefinition if needed.
/// All APIs on the IWidgetDefinition and IWidget2Definition interfaces are reflected here.
/// </summary>
public class ComSafeWidgetDefinition
{
    public WidgetDefinition OopWidgetDefinition { get; private set; }

    public bool AllowMultiple { get; private set; }

    public string Description { get; private set; }

    public string DisplayTitle { get; private set; }

    public string Id { get; private set; }

    public WidgetProviderDefinition ProviderDefinition { get; private set; }

    public string AdditionalInfoUri { get; private set; }

    public bool IsCustomizable { get; private set; }

    private const int RpcServerUnavailable = unchecked((int)0x800706BA);
    private const int RpcCallFailed = unchecked((int)0x800706BE);

    public string ProgressiveWebAppHostPackageFamilyName => throw new System.NotImplementedException();

    public WidgetType Type { get; private set; }

    public ComSafeWidgetDefinition(WidgetDefinition widgetDefinition)
    {
        OopWidgetDefinition = widgetDefinition;
        AllowMultiple = widgetDefinition.AllowMultiple;
        Description = widgetDefinition.Description;
        DisplayTitle = widgetDefinition.DisplayTitle;
        Id = widgetDefinition.Id;
        ProviderDefinition = widgetDefinition.ProviderDefinition;
        AdditionalInfoUri = widgetDefinition.AdditionalInfoUri;
        IsCustomizable = widgetDefinition.IsCustomizable;
        Type = widgetDefinition.Type;
    }

    public async Task<WidgetThemeResources> GetThemeResourceAsync(WidgetTheme theme)
    {
        return await Task.Run(async () =>
        {
            try
            {
                return OopWidgetDefinition.GetThemeResource(theme);
            }
            catch (COMException e) when (e.HResult == RpcServerUnavailable || e.HResult == RpcCallFailed)
            {
                await GetNewOopWidgetDefinitionAsync();
                return OopWidgetDefinition.GetThemeResource(theme);
            }
        });
    }

    public async Task<WidgetCapability[]> GetWidgetCapabilitiesAsync()
    {
        return await Task.Run(async () =>
        {
            try
            {
                return OopWidgetDefinition.GetWidgetCapabilities();
            }
            catch (COMException e) when (e.HResult == RpcServerUnavailable || e.HResult == RpcCallFailed)
            {
                await GetNewOopWidgetDefinitionAsync();
                return OopWidgetDefinition.GetWidgetCapabilities();
            }
        });
    }

    private async Task GetNewOopWidgetDefinitionAsync()
    {
        var hostingService = Application.Current.GetService<IWidgetHostingService>();
        var catalog = await hostingService.GetWidgetCatalogAsync();
        OopWidgetDefinition = catalog.GetWidgetDefinition(Id);

        AllowMultiple = OopWidgetDefinition.AllowMultiple;
        Description = OopWidgetDefinition.Description;
        DisplayTitle = OopWidgetDefinition.DisplayTitle;
        Id = OopWidgetDefinition.Id;
        ProviderDefinition = OopWidgetDefinition.ProviderDefinition;
        AdditionalInfoUri = OopWidgetDefinition.AdditionalInfoUri;
        IsCustomizable = OopWidgetDefinition.IsCustomizable;
        Type = OopWidgetDefinition.Type;
    }
}
