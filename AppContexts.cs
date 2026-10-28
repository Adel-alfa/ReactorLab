using Microsoft.UI;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace ReactorLab;

internal sealed record AppSettings(bool IsDark, string DisplayName);
internal static class AppContexts
{
    public static readonly Context<AppSettings> Settings =
        new(new AppSettings(IsDark: false, DisplayName: "Reactor Explorer"));
    public static readonly SolidColorBrush DarkBackgroundBrush = new(Color.FromArgb(255, 32, 32, 32));
    public static readonly SolidColorBrush LightBackgroundBrush = new(Colors.White);
}