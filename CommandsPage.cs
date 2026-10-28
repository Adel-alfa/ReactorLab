using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Windows.System;

using static Microsoft.UI.Reactor.Factories;

namespace ReactorLab;

internal class CommandsPage :Component
{
    public override Element Render()
    {

        var (status, setStatus) = UseState("Idle");
        var (simulateError, setSimulateError) = UseState(false);
        var loadCommand = UseCommand(new Command
        {
            Label = "Load sample data",
            Description = "Fetch mock data asynchronously",
            Accelerator = Accelerator(VirtualKey.L, VirtualKeyModifiers.Control),
            ExecuteAsync = async () =>
            {
                setStatus("Loading");
                await Task.Delay(1200);
                setStatus(simulateError
                    ? "Error: mock service unavailable"
                    : $"Loaded at {DateTime.Now:HH:mm:ss}");
            }
        });
        var resetCommand = new Command
        {
            Label = "Reset",
            CanExecute = !loadCommand.IsExecuting,
            Execute = () => setStatus("Idle")
        };
        return ScrollView(VStack(16,
            Title("Commands & shortcuts"),
             Body("Commands bundle the action, enabled state, and accelerator. UseCommand tracks asynchronous work and suppresses re-entry."),
             HStack(8,
               Button(loadCommand),
               Button(resetCommand),
               When(loadCommand.IsExecuting, () => ProgressRing().Width(24).Height(24))
             ),
              CheckBox(simulateError, value => setSimulateError(value), label: "Simulate request error"),
              Card(VStack(8,
                TextBlock($"Status: {status}").SemiBold()
                    .Foreground(status.StartsWith("Error", StringComparison.Ordinal)
                        ? Theme.SystemCritical
                        : Theme.SecondaryText),
                TextBlock($"Command executing: {loadCommand.IsExecuting}"),
                TextBlock("Keyboard accelerator: Ctrl+L").Foreground(Theme.SecondaryText)
            )),
              TextBlock("Try clicking repeatedly while loading; UseCommand keeps the action single-flight.")
                .Foreground(Theme.SecondaryText)
            ).Padding(24)
            );
    }
}
