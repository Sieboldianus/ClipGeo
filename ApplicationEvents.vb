Imports System.Globalization
Imports System.Threading

Namespace My

    ' The following events are available for MyApplication:
    ' 
    ' Startup: Raised when the application starts, before the startup form is created.
    ' Shutdown: Raised after all application forms are closed.  This event is not raised if the application terminates abnormally.
    ' UnhandledException: Raised if the application encounters an unhandled exception.
    ' StartupNextInstance: Raised when launching a single-instance application and the application is already active. 
    ' NetworkAvailabilityChanged: Raised when the network connection is connected or disconnected.
    Partial Friend Class MyApplication

        Private Sub MyApplication_Startup(sender As Object, e As ApplicationServices.StartupEventArgs) Handles Me.Startup
            Thread.CurrentThread.CurrentCulture = New CultureInfo("en-US") 'Regionale Einstellungen en-US für englisches Nummernformat bei Export .txt
            Thread.CurrentThread.CurrentUICulture = New CultureInfo("en-US")
        End Sub
    End Class




End Namespace

