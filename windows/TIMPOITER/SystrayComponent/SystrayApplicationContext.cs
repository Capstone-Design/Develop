//*********************************************************  
//  
// Copyright (c) Microsoft. All rights reserved.  
// This code is licensed under the MIT License (MIT).  
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF  
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY  
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR  
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.  
//  
//********************************************************* 
using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Microsoft.Win32;
using Windows.Storage;




namespace SystrayComponent
{
    class SystrayApplicationContext : ApplicationContext
    {
        private AppServiceConnection connection = null;
        private NotifyIcon notifyIcon = null;
        private Form1 configWindow = new Form1();
        ApplicationDataContainer localSettings = null;


        public SystrayApplicationContext()
        {
            localSettings = ApplicationData.Current.LocalSettings;
            MenuItem openMenuItem = new MenuItem("설정", new EventHandler(OpenSetting));
            MenuItem settingMenuItem = new MenuItem("자동설정", new EventHandler(autoSetting));
            MenuItem batteryMenuItem = new MenuItem("모듈 배터리 잔량 확인", new EventHandler(batteryConfirm));
            MenuItem exitMenuItem = new MenuItem("종료", new EventHandler(Exit));
            openMenuItem.DefaultItem = true;

            notifyIcon = new NotifyIcon();
            notifyIcon.DoubleClick += new EventHandler(OpenApp);
            notifyIcon.Icon = SystrayComponent.Properties.Resources.baseline_touch_app_black_48_dqL_icon;
            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[]{ openMenuItem, settingMenuItem, batteryMenuItem, exitMenuItem });
            notifyIcon.Visible = true;
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;

        }

        private async void OpenApp(object sender, EventArgs e)
        {
            IEnumerable<AppListEntry> appListEntries = await Package.Current.GetAppListEntriesAsync();
            await appListEntries.First().LaunchAsync();
        }

        private async void OpenSetting(object sender, EventArgs e)
        {
            IEnumerable<AppListEntry> appListEntries = await Package.Current.GetAppListEntriesAsync();
            await appListEntries.First().LaunchAsync();
            ValueSet message = new ValueSet();
            message.Add("openSetting", "");
            await Task.Delay(100);
            await SendToUWP(message);
        }

        private async void autoSetting(object sender, EventArgs e)
        {
            int a = (int)System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width * 10000 + (int)System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            ValueSet message = new ValueSet();
            message.Add("autoSetting", a.ToString());

            await SendToUWP(message);
        }

        private async void batteryConfirm(object sender, EventArgs e)
        {
            ValueSet message = new ValueSet();
            message.Add("battery", Convert.ToInt32(localSettings.Values["battery1"]) * 1000 + Convert.ToInt32(localSettings.Values["battery2"]));
            await SendToUWP(message);
        }

        private async void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            ValueSet message = new ValueSet();
            int a = (int)System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width * 10000 + (int)System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            message.Add("resolutionchaged",a.ToString());
            await SendToUWP(message);
            System.Diagnostics.Debug.WriteLine("resolutionchaged");

        }

        private async void Exit(object sender, EventArgs e)
        {
            ValueSet message = new ValueSet();
            message.Add("exit", "");
            await SendToUWP(message);
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            Application.Exit();
        }

        private async Task SendToUWP(ValueSet message)
        { 
            if (connection == null)
            {
                connection = new AppServiceConnection();
                connection.PackageFamilyName = Package.Current.Id.FamilyName;
                connection.AppServiceName = "SystrayExtensionService";
                connection.ServiceClosed += Connection_ServiceClosed;
                AppServiceConnectionStatus connectionStatus = await connection.OpenAsync();
                if (connectionStatus != AppServiceConnectionStatus.Success)
                {
                    MessageBox.Show("Status: " + connectionStatus.ToString());
                    return;
                }
            }

            await connection.SendMessageAsync(message);
        }

        private void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            connection.ServiceClosed -= Connection_ServiceClosed;
            connection = null;
        }
    }
}
