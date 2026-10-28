using Microsoft.UI;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using static Microsoft.UI.Reactor.Factories;
using Windows.UI;


namespace ReactorLab;

internal class ReactorLab : Component
{
    private static readonly AppSettings InitialSettings = new(IsDark: false, DisplayName: "Reactor Explorer");
   

    public override Element Render()
    {
        var (settings, setSettings) = UseState(InitialSettings);
        var requestedTheme = settings.IsDark ? ElementTheme.Dark : ElementTheme.Light;
        var backgroundBrush = settings.IsDark ? AppContexts.DarkBackgroundBrush : AppContexts.LightBackgroundBrush;
        var tabs = TabView(
            Tab("Overview", Component<OverviewPage>()),
            Tab("State & components", Component<StatePage>()),
            Tab("Effects", Component<EffectsPage>()),
            Tab("Context", Component<ContextPage>()),
            Tab("Commands", Component<CommandsPage>()),
            Tab("Advanced", Component<AdvancedPage>())
        ).FillContentArea()
        .RequestedTheme(requestedTheme);

        return VStack(0,
          HStack(12,
            VStack(2,
                  TextBlock("Reactor Lab").FontSize(12).FontWeight(FontWeights.SemiBold).Foreground(Theme.Accent),
                  TextBlock(settings.DisplayName).FontSize(22).Bold()),
                  TextBlock("WinUI 3 · declarative C# · preview.16")
                    .Foreground(Theme.SecondaryText),
                  ToggleSwitch(settings.IsDark,
                    isDark => setSettings(settings with { IsDark = isDark }),
                    header: "Dark Mode")
                    ).Padding(20),
            tabs.Provide(AppContexts.Settings, settings),
            HStack(
            TextBlock("© Adel HASSAN 2026 Reactor Lab · Built with WinUI 3 & Reactor.NET")
                .FontSize(12)
                .Foreground(Theme.SecondaryText)
        ).Padding(20)
        .HorizontalAlignment(HorizontalAlignment.Center)
        ).Padding(0)
        .Background(backgroundBrush)
        .RequestedTheme(requestedTheme);
    }
    
}

internal sealed class OverviewPage : Component
{
    public override Element Render()
    {
        var settings = UseContext(AppContexts.Settings);
        var (count, updateCount) = UseReducer(0);
        var increment = UseCallback(() => updateCount(value => value + 1), Array.Empty<object>());

        return ScrollView(VStack(18,
             Title("Framework field guide"),
            Body("A working tour of Reactor's render, state, lifecycle, and reconciliation model."),
            HStack(12,
                StatPanel("Components", "Typed props", "Composition and local ownership"),
                StatPanel("Hooks", "Positional state", "Reducers, effects, memo, and refs"),
                StatPanel("Runtime", "Native WinUI", "Elements reconcile to real controls")
            ),
             Card(VStack(8,
                TextBlock($"Welcome, {settings.DisplayName}").FontSize(18).SemiBold(),
                TextBlock($"Shared context is currently {(settings.IsDark ? "dark" : "light")}."),
                TextBlock("The tab pages below are separate components, each with its own hook slots.")
                    .Foreground(Theme.SecondaryText)
            )),
            Card(VStack(8,
                TextBlock("Callback props and child composition").SemiBold(),
                TextBlock($"Parent reducer value: {count}"),
                Component<IncrementButton, IncrementButtonProps>(new(count, increment))
            ))
            ).Padding(24));
    }
    private static Element StatPanel(string label, string value, string note) =>
       Card(VStack(6,
           TextBlock(label).Foreground(Theme.SecondaryText),
           TextBlock(value).FontSize(18).SemiBold(),
           TextBlock(note).FontSize(12).Foreground(Theme.SecondaryText)
       ));
}

internal sealed record IncrementButtonProps(int Count, Action OnIncrement);
internal sealed class IncrementButton : Component<IncrementButtonProps>
{
    public override Element Render() =>
        Button($"Increment from child ({Props.Count})", Props.OnIncrement)
            .AutomationName("Increment parent count from child component");
}