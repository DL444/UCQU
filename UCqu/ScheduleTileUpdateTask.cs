using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;
using CquScoreLib;

namespace UCqu
{
    static class ScheduleTileUpdateTask
    {
        public static async Task UpdateTile()
        {
            TileUpdater updater = TileUpdateManager.CreateTileUpdaterForApplication();

            string id = "", pwdHash = "", host = "";
            if(LoadCredentials(out id, out pwdHash, out host) == false)
            {
                updater.Clear();
                return;
            }

            Watcher watcher = new Watcher(host, id, pwdHash);
            bool isCorrect = false;
            try
            {
                await watcher.LoginAsync();
                isCorrect = await watcher.ValidateLoginAsync();
            }
            catch(System.Net.WebException)
            {
                return;
            }
            TileContent content = null;
            if (isCorrect)
            {
                await watcher.PerformSchedule(ConstantResources.CurrentTerm);
                List<ScheduleEntry> entries = watcher.Schedule.GetDaySchedule((DateTime.Today - ConstantResources.StartDate).Days);
                TileBindingContentAdaptive tileContent = new TileBindingContentAdaptive();

                if (entries.Count > 0)
                {
                    foreach (ScheduleEntry e in entries)
                    {
                        AdaptiveGroup group = new AdaptiveGroup()
                        {
                            Children =
                            {
                                new AdaptiveSubgroup()
                                {
                                    Children =
                                    {
                                        new AdaptiveText()
                                        {
                                            Text = e.Name,
                                            HintStyle = AdaptiveTextStyle.Base,
                                        },
                                        new AdaptiveText()
                                        {
                                            Text = e.SessionSpan + "  " + e.Room,
                                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                                        },
                                        new AdaptiveText()
                                    }
                                }
                            }
                        };
                        tileContent.Children.Add(group);
                    }
                }
                else
                {
                    List<HuxiImgEntry> huxiImgEntries = await HuxiImg.GetEntries();
                    Random randomizer = new Random((int)DateTime.Now.Ticks);
                    int index = randomizer.Next(0, huxiImgEntries.Count);
                    tileContent.BackgroundImage = new TileBackgroundImage()
                    {
                        Source = huxiImgEntries[index].Uri,
                        HintOverlay = 40
                    };
                    tileContent.Children.Add
                    (
                        new AdaptiveText()
                        {
                            Text = huxiImgEntries[index].Title,
                            HintStyle = AdaptiveTextStyle.Base
                        }
                    );
                    tileContent.Children.Add
                    (
                        new AdaptiveText()
                        {
                            Text = huxiImgEntries[index].Author,
                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                        }
                    );
                }


                content = new TileContent()
                {
                    Visual = new TileVisual()
                    {
                        Branding = TileBranding.Name,
                        TileLarge = new TileBinding()
                        {
                            Content = tileContent
                        },
                        TileMedium = new TileBinding()
                        {
                            Content = tileContent
                        },
                        TileWide = new TileBinding()
                        {
                            Content = tileContent
                        },
                    }
                };
            }
            else
            {
                updater.Clear();
                return;
            }

            var notification = new TileNotification(content.GetXml());
            notification.ExpirationTime = DateTimeOffset.UtcNow.AddMinutes(60);
            updater.Clear();
            TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);
        }

        static bool LoadCredentials(out string id, out string pwdHash, out string host)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            string uid = localSettings.Values["id"] as string;
            if (uid == null) { id = ""; pwdHash = ""; host = ""; return false; }
            else
            {
                id = uid; pwdHash = localSettings.Values["pwdHash"] as string; host = localSettings.Values["host"] as string;
                return true;
            }
        }

    }
}
