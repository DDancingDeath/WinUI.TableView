using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Graphics;

namespace TaskManagerSample;

public partial class App : Application
{
    public Window? MainWindow { get; private set; }

    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs e)
    {
        MainWindow = new Window
        {
            Title = "Task Manager"
        };

        // Mica material — matches the real Task Manager's backdrop
        MainWindow.SystemBackdrop = new MicaBackdrop();

        // Start at the same compact size as the real Task Manager
        MainWindow.AppWindow.Resize(new SizeInt32(980, 680));

        var rootFrame = new Frame();
        rootFrame.NavigationFailed += OnNavigationFailed;
        MainWindow.Content = rootFrame;

        rootFrame.Navigate(typeof(MainPage), e.Arguments);
        MainWindow.Activate();
    }

    private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }
}
