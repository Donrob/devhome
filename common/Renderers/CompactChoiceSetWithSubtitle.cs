// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AdaptiveCards.ObjectModel.WinUI3;
using AdaptiveCards.Rendering.WinUI3;
using DevHome.Common.DevHomeAdaptiveCards.CardModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Data.Json;

namespace DevHome.Common.Renderers;

public class CompactChoiceSetWithSubtitle : IAdaptiveElementRenderer
{
    private const int UnSelectedIndex = -1;

    public UIElement Render(IAdaptiveCardElement element, AdaptiveRenderContext context, AdaptiveRenderArgs renderArgs)
    {
        var renderer = new AccessibleChoiceSet();

        if (element is AdaptiveChoiceSetInput choiceSet)
        {
            var choices = GetChoicesFromAdditionalProperties(choiceSet);

            if (choices.Count > 0)
            {
                var comboBox = new ComboBox();
                comboBox.ItemsSource = choices;
                comboBox.ItemTemplate = Application.Current.Resources["ChoiceSetWithSubtitleTemplate"] as DataTemplate;
                comboBox.SelectedIndex = int.TryParse(choiceSet.Value, out var selectedIndex) ? selectedIndex : UnSelectedIndex;
                return comboBox;
            }
        }

        return renderer.Render(element, context, renderArgs);
    }

    private List<ChoiceSetWithSubtitleData> GetChoicesFromAdditionalProperties(AdaptiveChoiceSetInput choiceSet)
    {
        if (choiceSet.AdditionalProperties.TryGetValue("devHomeChoicesWithSubtitle", out var choices) && choices.ValueType == JsonValueType.Array)
        {
            return JsonSerializer.Deserialize<List<ChoiceSetWithSubtitleData>>(choices.GetArray().ToString()) ?? new List<ChoiceSetWithSubtitleData>();
        }

        return new List<ChoiceSetWithSubtitleData>();
    }
}
