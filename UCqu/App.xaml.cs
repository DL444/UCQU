﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Diagnostics;
using Windows.Foundation.Metadata;


namespace UCqu
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            EnvironmentApiCheck();
        }

        private static void EnvironmentApiCheck()
        {
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 6))
            {
                CommonResources.ApiContract = 6;
                // RS4 - RefreshContainer
            }
            else if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5))
            {
                CommonResources.ApiContract = 5;
                // RS3 - AcrylicBrush
            }
            else if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 4))
            {
                CommonResources.ApiContract = 4;
                // RS2 - Conditional XAML
            }
            else
            {
                CommonResources.ApiContract = 3;
                // RS1 - Windows Phone
            }
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = InitializeWindow(e);
            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter

                    //rootFrame.Navigate(typeof(MainPage), e.Arguments);
                    rootFrame.Navigate(typeof(Login), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        protected override async void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            //LoggingChannel lc = new LoggingChannel("UCQU_BackgroundTask", null, new Guid("4bd2826e-54a1-4ba9-bf63-92b73ea1ac4a"));
            //lc.LogMessage("Background Task Activated.");
            base.OnBackgroundActivated(args);
            //lc.LogMessage("Base Handler Called.");
            IBackgroundTaskInstance taskInstance = args.TaskInstance;
            //lc.LogMessage("Task Instance Set.");
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            //lc.LogMessage("Task Deferral Obtained.");
            //lc.LogMessage("Executing Payload Method.");
            switch(args.TaskInstance.Task.Name)
            {
                case "Hourly Tile Update Task":
                case "Login Tile Update Task":
                    try
                    {
                        await ScheduleNotificationUpdateTasks.UpdateTile();
                    }
                    catch (Exception e)
                    {
                        //lc.LogMessage($"Unhandled exception thrown when executing task payload.\n\n{e.Message}\n\n{e.StackTrace}");
                    }
                    break;
            }
            //lc.LogMessage("Completing Deferral.");
            deferral.Complete();
            //lc.LogMessage("Task Completed.");
        }

        protected override void OnActivated(IActivatedEventArgs e)
        {
            base.OnActivated(e);
            if (e is ToastNotificationActivatedEventArgs)
            {
                Frame rootFrame = InitializeWindow(e);
                if(rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(Login), "");
                }
                Window.Current.Activate();
            }
        }

        Frame InitializeWindow(IActivatedEventArgs e)
        {
            //if (CommonResources.ApiContract > 4)
            //{
            //    AcrylicBrush navViewTopPaneBackgroundBrush = new AcrylicBrush
            //    {
            //        BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
            //        TintColor = (Color)Resources["SystemAccentColor"],
            //        TintOpacity = 0.7,
            //        FallbackColor = (Color)this.Resources["SystemAccentColor"]
            //    };
            //    this.Resources["NavigationViewTopPaneBackground"] = navViewTopPaneBackgroundBrush;
            //}


            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

            var viewTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.ButtonBackgroundColor = Colors.Transparent;
            viewTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            viewTitleBar.ButtonForegroundColor = (Color)Resources["SystemBaseHighColor"];


            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }
            return rootFrame;
        }
    }
}
