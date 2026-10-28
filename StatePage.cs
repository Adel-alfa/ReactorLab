using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Text;
using static Microsoft.UI.Reactor.Factories;
using static System.Net.WebRequestMethods;

namespace ReactorLab;

    internal sealed record TaskItem(int Id, string Title, bool IsDone);
    internal sealed record TaskState(TaskItem[] Items, int NextId);
    internal abstract record TaskAction;
    internal sealed record AddTask(string Title) : TaskAction;
    internal sealed record SetTaskDone(int Id, bool IsDone) : TaskAction;
    internal sealed record RemoveTask(int Id) : TaskAction;
    internal sealed record ClearCompleted : TaskAction;
    internal class StatePage: Component
    {
        private static readonly TaskState InitialTasks = new(
       new TaskItem[]
       {
            new TaskItem(1, "Explore component props", true),
            new TaskItem(2, "Try a reducer action", false),
            new TaskItem(3, "Inspect a keyed list", false)
       }, 4);
    public override Element Render()
    {
        var (filter, setFilter) = UseState("");
        var (showDone, setShowDone) = UseState(true);
        var (state, dispatch) = UseReducer(
            (TaskState current, TaskAction action) => Reduce(current, action), InitialTasks);

        var visibleTasks = UseMemo(() => state.Items
            .Where(task => task.Title.Contains(filter, StringComparison.OrdinalIgnoreCase))
            .Where(task => showDone || !task.IsDone)
            .ToArray(), state.Items, filter, showDone);

        var completedCount = state.Items.Count(task => task.IsDone);

        return ScrollView(VStack(16,
            Title("State & Components"),
            Body("UseState is for simple local values; UseReducer keeps related transitions in one pure update function."),
            Card(VStack(10,
            TextBlock("UseState · counter and controlled input").SemiBold(),
            HStack(8,
                 TextBox(filter, setFilter, placeholderText: "Filter tasks")
                    .AutomationName("Filter tasks").Width(220)
            ),
            CheckBox(showDone, value => setShowDone(value), label: "Include completed tasks")
            )),
            Card(VStack(10,
                HStack(8,
                    TextBlock("UseReducer · task state machine").SemiBold(),
                    TextBlock($"{completedCount}/{state.Items.Length} complete")
                        .Foreground(Theme.SecondaryText)
                ),
                 TaskEntry(dispatch),
                  VStack(4, visibleTasks.Select(task =>
                   HStack(8,
                   CheckBox(task.IsDone, value => dispatch(new SetTaskDone(task.Id, value)), label: task.Title),
                   Button("Remove", () => dispatch(new RemoveTask(task.Id)))
                            .AutomationName($"Remove {task.Title}")
                   ).WithKey(task.Id.ToString())
                         ).ToArray()
                  ),
                   Button("Clear completed", () => dispatch(new ClearCompleted()))
                    .IsEnabled(completedCount > 0)
            )),
             Body("UseMemo caches the filtered projection until the task list, query, or completion toggle changes.")
                .Foreground(Theme.SecondaryText)
            ).Padding(24)); 
        }



    private static Element TaskEntry(Action<TaskAction> dispatch) => Memo(ctx =>
    {
        var (title, setTitle) = ctx.UseState("");
        return HStack(8,
            TextBox(title, setTitle, placeholderText: "Add a task")
                .AutomationName("New task title").Width(280),
            Button("Add", () =>
            {
                if (!string.IsNullOrWhiteSpace(title))
                {
                    dispatch(new AddTask(title.Trim()));
                    setTitle("");
                }
            }).IsEnabled(!string.IsNullOrWhiteSpace(title))
        );
    });


    // Reducers are pure transitions: return a new snapshot for each action.
    private static TaskState Reduce(TaskState state, TaskAction action) => action switch
    {
        AddTask add => state with
        {
            Items = [.. state.Items, new TaskItem(state.NextId, add.Title, false)],
            NextId = state.NextId + 1
        },
        SetTaskDone update => state with
        {
            Items = state.Items.Select(task => task.Id == update.Id
                ? task with { IsDone = update.IsDone }
                : task).ToArray()
        },
        RemoveTask remove => state with { Items = state.Items.Where(task => task.Id != remove.Id).ToArray() },
        ClearCompleted => state with { Items = state.Items.Where(task => !task.IsDone).ToArray() },
        _ => state
    };
    }

