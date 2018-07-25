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
            //Windows.Foundation.Diagnostics.LoggingChannel lc = new Windows.Foundation.Diagnostics.LoggingChannel("UCQU_BackgroundTaskPayload", null, new Guid("4bd2826e-54a1-4ba9-bf63-92b73ea1ac4a"));
            //lc.LogMessage("Entered Payload Method.");
            TileUpdater updater = TileUpdateManager.CreateTileUpdaterForApplication();
            //lc.LogMessage("Tile Updated Created.");
            string id = "", pwdHash = "", host = "";
            if(LoadCredentials(out id, out pwdHash, out host) == false)
            {
                //lc.LogMessage("Credential Load Failed. Clearing tile and exiting...");
                updater.Clear();
                return;
            }
            //lc.LogMessage("Constructing Watcher...");
            Watcher watcher = new Watcher(host, id, pwdHash);
            bool isCorrect = false;
            try
            {
                //lc.LogMessage("Attempting Login.");
                await watcher.LoginAsync();
                isCorrect = await watcher.ValidateLoginAsync();
            }
            catch(System.Net.WebException ex)
            {
                //lc.LogMessage($"Network Exception thrown attempting to login.\n\n{ex.Message}\n\n{ex.StackTrace}");
                return;
            }
            TileContent content = null;
            if (isCorrect)
            {
                //lc.LogMessage("Obtaining Schedule.");
                await watcher.PerformSchedule(CommonResources.CurrentTerm);
                //lc.LogMessage("Getting Schedule of Today.");
                List<ScheduleEntry> entries = watcher.Schedule.GetDaySchedule((DateTime.Today/*ConstantResources.TestDate*/ - CommonResources.StartDate).Days);

                //lc.LogMessage("Constructing Tile Content.");
                TileBindingContentAdaptive midTileContent = new TileBindingContentAdaptive();
                TileBindingContentAdaptive wideTileContent = new TileBindingContentAdaptive();
                TileBindingContentAdaptive largeTileContent = new TileBindingContentAdaptive();

                if (entries.Count > 0)
                {
                    //lc.LogMessage("Course Detected. Creating daily schedule tile.");
                    foreach (ScheduleEntry e in entries)
                    {
                        AdaptiveGroup midGroup = new AdaptiveGroup()
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
                                            HintStyle = AdaptiveTextStyle.Caption,
                                            HintWrap = true
                                        },
                                        new AdaptiveText()
                                        {
                                            Text = e.SessionSpan + "  " + e.Room,
                                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                                        },
                                    }
                                }
                            }
                        };
                        AdaptiveGroup wideGroup = new AdaptiveGroup()
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
                                            HintStyle = AdaptiveTextStyle.Caption,
                                        },
                                        new AdaptiveText()
                                        {
                                            Text = e.SessionSpan + "  " + e.Room,
                                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                                        },
                                    }
                                }
                            }
                        };
                        AdaptiveGroup largeGroup = new AdaptiveGroup()
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
                                            HintWrap = true
                                        },
                                        new AdaptiveText()
                                        {
                                            Text = e.SessionSpan + "  " + e.Room,
                                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                                        },
                                        new AdaptiveText()
                                        {
                                            HintStyle = AdaptiveTextStyle.Caption
                                        }
                                    }
                                }
                            }
                        };
                        midTileContent.Children.Add(midGroup);
                        wideTileContent.Children.Add(wideGroup);
                        largeTileContent.Children.Add(largeGroup);
                    }
                    //lc.LogMessage("Assigning Tile Content.");
                    content = new TileContent()
                    {
                        Visual = new TileVisual()
                        {
                            Branding = TileBranding.Name,
                            TileLarge = new TileBinding()
                            {
                                Content = largeTileContent
                            },
                            TileMedium = new TileBinding()
                            {
                                Content = midTileContent
                            },
                            TileWide = new TileBinding()
                            {
                                Content = wideTileContent
                            },
                        }
                    };
                }
                else
                {
                    //lc.LogMessage("No Course Detected. Creating image tile.");
                    //lc.LogMessage("Getting Image Entries Metadata.");
                    List<HuxiImgEntry> huxiImgEntries = await HuxiImg.GetEntries();
                    //lc.LogMessage("Creating Randomizer.");
                    Random randomizer = new Random((int)DateTime.Now.Ticks);
                    int index = randomizer.Next(0, huxiImgEntries.Count);
                    //lc.LogMessage("Randomly Selected Index: " + index);
                    //lc.LogMessage("Constructing Image Tile.");
                    largeTileContent.BackgroundImage = new TileBackgroundImage()
                    {
                        Source = huxiImgEntries[index].Uri,
                        HintOverlay = 40
                    };
                    largeTileContent.Children.Add
                    (
                        new AdaptiveText()
                        {
                            Text = huxiImgEntries[index].Title,
                            HintStyle = AdaptiveTextStyle.Base
                        }
                    );
                    largeTileContent.Children.Add
                    (
                        new AdaptiveText()
                        {
                            Text = huxiImgEntries[index].Author,
                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                        }
                    );
                    //lc.LogMessage("Assigning Tile Content.");
                    content = new TileContent()
                    {
                        Visual = new TileVisual()
                        {
                            Branding = TileBranding.Name,
                            TileLarge = new TileBinding()
                            {
                                Content = largeTileContent
                            },
                            TileMedium = new TileBinding()
                            {
                                Content = largeTileContent
                            },
                            TileWide = new TileBinding()
                            {
                                Content = largeTileContent
                            },
                        }
                    };
                }
            }
            else
            {
                //lc.LogMessage("Credential Invalid. Clearing tile and exiting...");
                updater.Clear();
                return;
            }
            //lc.LogMessage("Creating Tile Notification.");
            var notification = new TileNotification(content.GetXml());
            //lc.LogMessage("Setting Expiry Time.");
            notification.ExpirationTime = DateTimeOffset.UtcNow.AddMinutes(60);
            //lc.LogMessage("Clearing Existing Tiles");
            updater.Clear();
            //lc.LogMessage("Updating Tiles");
            TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);
            //lc.LogMessage("Payload Method Complete.");
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
