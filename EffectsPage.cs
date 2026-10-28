using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Text;
using System.Xml;
using static Microsoft.UI.Reactor.Factories;

namespace ReactorLab;

    internal sealed class EffectsPage : Component
    {
        public override Element Render()
        {

        var (seconds, updateSeconds) = UseReducer(0);
        var (running, setRunning) = UseState(false);
        var (loadState, setLoadState) = UseState("Waiting to load");
        var (refresh, setRefresh) = UseState(0);

        UseEffect(() =>
        {
            if (!running) return () => { };

            // Each running interval is canceled and disposed before replacement or unmount.
            var cancellation = new CancellationTokenSource();
            var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            var token = cancellation.Token;
            _ = Task.Run(async () =>
            {
                try
                {
                    while (await timer.WaitForNextTickAsync(token))
                        updateSeconds(value => value + 1);
                }
                catch (OperationCanceledException) { }
            });

            return () => { cancellation.Cancel(); timer.Dispose(); };
        }, running);

        UseEffect(() =>
        {
            // A changed refresh value cancels any older request before starting this one.
            var cancellation = new CancellationTokenSource();
            var token = cancellation.Token;
            setLoadState("Loading mock telemetry…");

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(900, token);
                    token.ThrowIfCancellationRequested();
                    setLoadState($"Telemetry synchronized at {DateTime.Now:HH:mm:ss}");
                }
                catch (OperationCanceledException) { }
            });

            return () => cancellation.Cancel();
        }, refresh);

        return ScrollView(VStack(16,
                Title("Effects & lifecycle"),
                 Body("Effects run after the UI commits. Dependencies decide when to replace an effect; cleanup runs before replacement and on unmount."),
                 Card(VStack(10,
                 TextBlock("Managed interval").SemiBold(),
                  HStack(8,
                  TextBlock($"Elapsed: {seconds}s").FontSize(20),
                  Button(running ? "Stop" : "Start", () => setRunning(!running))
                        .AutomationName(running ? "Stop timer" : "Start timer"),
                   Button("Reset", () => updateSeconds(_ => 0))
                  ),
                  TextBlock("The effect owns both the PeriodicTimer and cancellation token source.")
                    .Foreground(Theme.SecondaryText)
                    )),
                 Card(VStack(10,
                    TextBlock("Cancelable mock async request").SemiBold(),
                    TextBlock(loadState).AutomationName("Mock request status"),
                    Button("Refresh telemetry", () => setRefresh(refresh + 1))
                 )),
                  TextBlock("Change the dependencies rapidly or switch tabs to exercise cancellation and cleanup.")
                .Foreground(Theme.SecondaryText)

            ).Padding(24));
        }
    }

