using AppKit;
using Foundation;

using Foundation;

namespace app.OSX
{
    [Register("AppDelegate")]
    public class AppDelegate : global::Xamarin.Forms.Platform.MacOS.FormsApplicationDelegate //NSApplicationDelegate
    {
        public override NSWindow MainWindow => _window;

        NSWindow _window;
        public AppDelegate()
        {
            var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;

            var rect = new CoreGraphics.CGRect(200, 1000, 1024, 768);
            _window = new NSWindow(rect, style, NSBackingStore.Buffered, false);
            _window.Title = "app.OSX";
            _window.TitleVisibility = NSWindowTitleVisibility.Hidden;
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());
            base.DidFinishLaunching(notification);
            return;
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }
    }
}
