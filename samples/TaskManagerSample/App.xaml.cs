using Microsoft.UI.Xaml.Navigation;

namespace TaskManagerSample;

public partial class App : Application
{
    private Window? _window;

    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs e)
    {
        _window = new Window
        {
            Title = "Task Manager"
        };

        var rootFrame = new Frame();
        rootFrame.NavigationFailed += OnNavigationFailed;
        _window.Content = rootFrame;

        rootFrame.Navigate(typeof(MainPage), e.Arguments);
        _window.Activate();
    }

    private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }
}
