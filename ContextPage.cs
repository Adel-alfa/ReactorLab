using Microsoft.UI;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using static Microsoft.UI.Reactor.Factories;
using Windows.UI;

namespace ReactorLab;

internal class ContextPage : Component
{
       private static readonly AppSettings InitialSettings = new(false, "Reactor Explorer");
    public override Element Render()
    {
        var (settings, setSettings) = UseState(InitialSettings);
        var stableSettings = UseMemo(() => settings, settings.IsDark, settings.DisplayName);

        return ScrollView(VStack(16,
            Title("Context API"),
            Body("A context provides cross-cutting values to distant descendants without passing them through every intermediate component."),
            HStack(8,
                TextBox(settings.DisplayName,
                    name => setSettings(settings with { DisplayName = name }),
                    header: "Display name")
                    .AutomationName("Display name")
                    .Width(260),
                ToggleSwitch(settings.IsDark,
                    isDark => setSettings(settings with { IsDark = isDark }),
                    header: "Dark mode")
                ),
            VStack(12,
                Component<ContextConsumer>(),
                 VStack(8,
                    TextBlock("Nested provider override").SemiBold(),
                    Component<ContextConsumer>()
                    ).Provide(AppContexts.Settings, new AppSettings(!settings.IsDark, "Nested preview"))
            ).Provide(AppContexts.Settings, stableSettings),
             TextBlock("The parent owns the settings state; both consumers read it through UseContext.")
                .Foreground(Theme.SecondaryText)
            )
        ).Padding(24);
    }

}
internal sealed class ContextConsumer : Component
{
  
    public override Element Render()
    {

        var settings = UseContext(AppContexts.Settings);      
        var requestedTheme = settings.IsDark ? ElementTheme.Dark : ElementTheme.Light;
        var backgroundBrush = settings.IsDark ? AppContexts.DarkBackgroundBrush : AppContexts.LightBackgroundBrush;

        return Card(VStack(6,
            TextBlock(settings.DisplayName).SemiBold(),
            TextBlock($"Context theme: {(settings.IsDark ? "dark" : "light")}")
                .Foreground(Theme.SecondaryText)
        )).Background(backgroundBrush).RequestedTheme(requestedTheme);
    }
}