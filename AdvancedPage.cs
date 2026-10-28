using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;
using PropertyChangedEventHandler = System.ComponentModel.PropertyChangedEventHandler;
using INotifyPropertyChanged = System.ComponentModel.INotifyPropertyChanged;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using static Microsoft.UI.Reactor.Factories;

namespace ReactorLab;

internal class AdvancedPage : Component
{
    private static readonly ObservableSettings Settings = new();
    public override Element Render()
    {
        var (renderCount, setRenderCount) = UseState(0);
        var renderRef = UseRef(0);

        renderRef.Current++;

        var observedSettings = UseObservableTree(Settings);

        return ScrollView(VStack(16,
            Title("Advanced patterns"),
             Body("Use refs for mutable non-visual bookkeeping, memoize expensive subtrees, bridge observable models, and isolate risky rendering."),
             Card(VStack(8,
                TextBlock("UseRef and element refs").SemiBold(),
                TextBlock($"Render-triggering state: {renderCount}; mutable ref count: {renderRef.Current}"),
                TextBox("", _ => { }, placeholderText: "Focus target")
                    .AutomationName("Focus target")
                    .Width(260),
                TextBlock("UseRef persists without causing a render.")
             )),
             Card(VStack(8,
                TextBlock("Observable bridge · UseObservableTree").SemiBold(),
                TextBox(observedSettings.UserName, value => Settings.UserName = value,
                    header: "Observable user")
                    .AutomationName("Observable user name")
                    .Width(260),
                TextBlock($"Live projection: {observedSettings.UserName}")
            )),
                 Card(VStack(8,
                TextBlock("Memo subtree").SemiBold(),
                Button("Update parent render", () => setRenderCount(renderCount + 1)),
                Memo(_ => Border(VStack(4,
                    TextBlock("Memoized subtree").SemiBold(),
                    TextBlock("Parent updates do not rebuild this subtree.")
                        .Foreground(Theme.SecondaryText)
                ).Padding(10)).Background(Theme.ControlFillSecondary).CornerRadius(4), "stable")
            )),
                  Card(VStack(8,
                TextBlock("ErrorBoundary with retry key").SemiBold(),
                Component<RecoverableBoundaryDemo>()
            ))
            ).Padding(24));
        throw new NotImplementedException();
    }
}

internal sealed class RecoverableBoundaryDemo : Component
{
    public override Element Render()
    {
        var (attempt, setAttempt) = UseState(0);

        return ErrorBoundary(
            Component<FlakySubtree>().WithKey($"flaky-{attempt}"),
            exception => VStack(8,
                TextBlock("Subtree failed; the rest of the dashboard is still available.")
                    .Foreground(Theme.SystemCritical),
                TextBlock(exception.Message).FontSize(12),
                Button("Retry subtree", () => setAttempt(attempt + 1))
            ));
    }
}

internal sealed class FlakySubtree : Component
{
    public override Element Render()
    {
        var (shouldFail, setShouldFail) = UseState(false);
        if (shouldFail) throw new InvalidOperationException("Intentional demo failure");
        return Button("Trigger a contained render error", () => setShouldFail(true));
    }
}

internal sealed class ObservableSettings : INotifyPropertyChanged
{
    private string _userName = "Ada";

    public string UserName
    {
        get => _userName;
        set
        {
            if (_userName == value) return;
            _userName = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserName)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}