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
    static class ScheduleNotificationUpdateTasks
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
            //Watcher watcher = new Watcher(host, id, pwdHash);
            Watcher watcher = new Watcher("202.202.1.41", id, pwdHash);
            // TODO: Fix anti-scraper
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
                List<ScheduleEntry> entries = watcher.Schedule.GetDaySchedule((DateTime.Today /*CommonResources.TestDate*/ - CommonResources.StartDate).Days);

                entries.RemoveAll((ScheduleEntry x) =>
                {
                    (_, DateTime endTime) = SessionTimeConverter.Convert(x.SessionSpan);
                    DateTime now = DateTime.Now;
                    return endTime < now;
                });

                entries.Sort((x, y) => x.StartSlot - y.StartSlot);

                //lc.LogMessage("Constructing Tile Content.");
                TileBindingContentAdaptive midTileContent = new TileBindingContentAdaptive();
                TileBindingContentAdaptive wideTileContent = new TileBindingContentAdaptive();
                TileBindingContentAdaptive largeTileContent = new TileBindingContentAdaptive();

                if (entries.Count > 0)
                {
                    if (!IsToastScheduled)
                    {
                        try
                        {
                            ScheduleToast(entries);
                        }
                        catch (Exception ex) { }
                        IsToastScheduled = true;
                    }

                    //lc.LogMessage("Course Detected. Creating daily schedule tile.");
                    foreach (ScheduleEntry e in entries)
                    {
                        (var start, var end) = SessionTimeConverter.ConvertShort(e.SessionSpan);

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
                                            Text = $"{start}-{end}  {e.Room}",
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
                                            Text = $"{start}-{end}  {e.Room}",
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
                                            Text = $"{start}-{end}  {e.Room}",
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

                    if(!IsToastScheduled)
                    {
                        try
                        {
                            ScheduleToast(huxiImgEntries[index]);
                        }
                        catch(Exception ex) { }
                        IsToastScheduled = true;
                    }
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

        public static void ScheduleToast(List<ScheduleEntry> entries)
        {
            bool courseSwitch = true;
            bool dailySwitch = true;
            if (CommonResources.LoadSetting("courseToastSwitch", out string _cSwitch) == false) { CommonResources.SaveSetting("courseToastSwitch", "on"); }
            else { if (_cSwitch == "off") { courseSwitch = false; } }
            if (CommonResources.LoadSetting("dailyToastSwitch", out string _dSwitch) == false) { CommonResources.SaveSetting("dailyToastSwitch", "on"); }
            else { if (_dSwitch == "off") { dailySwitch = false; } }


            if (entries.Count == 0) { return; }

            ToastContent dailyGlance = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText() { Text = "今日课程", HintStyle = AdaptiveTextStyle.Subtitle } 
                        }
                    }
                }
            };

            for (int i = 0; i < entries.Count; i++)
            {
                (DateTime startTime, _) = SessionTimeConverter.Convert(entries[i].SessionSpan);
                ToastContent courseNotification = new ToastContent()
                {
                    Visual = new ToastVisual()
                    {
                        BindingGeneric = new ToastBindingGeneric()
                        {
                            Children =
                            {
                                new AdaptiveText() { Text = entries[i].Name },
                                new AdaptiveText() { Text = $"{new SessionTimeConverter().Convert(entries[i].SessionSpan, typeof(string), null, null)}  {entries[i].Room}" },
                            }
                        }
                    },
                    Scenario = ToastScenario.Reminder
                };

                dailyGlance.Visual.BindingGeneric.Children.Add
                (
                    new AdaptiveGroup()
                    {
                        Children =
                        {
                            new AdaptiveSubgroup()
                            {
                                Children =
                                {
                                    new AdaptiveText() { Text = entries[i].Name, HintStyle = AdaptiveTextStyle.Base },
                                    new AdaptiveText() { Text = $"{new SessionTimeConverter().Convert(entries[i].SessionSpan, typeof(string), null, null)}  {entries[i].Room}", HintStyle = AdaptiveTextStyle.BodySubtle },
                                }
                            }
                        }
                    }
                );
                try
                {
                    if(courseSwitch)
                    {
                        ScheduledToastNotification toast = new ScheduledToastNotification(courseNotification.GetXml(), new DateTimeOffset(startTime.AddMinutes(-15), new TimeSpan(8, 0, 0)));
                        toast.ExpirationTime = new DateTimeOffset(startTime.AddMinutes(15), TimeZoneInfo.Local.GetUtcOffset(DateTime.Now));
                        ToastNotificationManager.CreateToastNotifier().AddToSchedule(toast);
                    }
                }
                catch (ArgumentException) { }
            }

            if(dailySwitch)
            {
                if (DateTime.Now.Hour > 7 || (DateTime.Now.Hour == 7 && DateTime.Now.Minute > 25))
                {
                    ToastNotification dailyGlanceToast = new ToastNotification(dailyGlance.GetXml());
                    dailyGlanceToast.ExpirationTime = DateTime.Now.AddDays(1);
                    ToastNotificationManager.CreateToastNotifier().Show(dailyGlanceToast);
                }
                else
                {
                    ScheduledToastNotification dailyGlanceToast = new ScheduledToastNotification(dailyGlance.GetXml(),
                        new DateTimeOffset(new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 7, 30, 0), TimeZoneInfo.Local.GetUtcOffset(DateTime.Now)));
                    dailyGlanceToast.ExpirationTime = DateTime.Now.AddDays(1);
                    ToastNotificationManager.CreateToastNotifier().AddToSchedule(dailyGlanceToast);
                }
            }
        }
        public static void ScheduleToast(HuxiImgEntry entry)
        {
            if(CommonResources.LoadSetting("imgToastSwitch", out string _switch) == false) { CommonResources.SaveSetting("imgToastSwitch", "on"); }
            else { if(_switch == "off") { return; } }

            ToastContent imageToast = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        //HeroImage = new ToastGenericHeroImage()
                        //{
                        //    Source = entry.Uri,
                        //    AlternateText = "图说虎溪"
                        //},

                        Children =
                        {
                            new AdaptiveImage()
                            {
                                Source = entry.Uri,
                                AlternateText = "图说虎溪",
                                HintAlign = AdaptiveImageAlign.Stretch,
                                HintRemoveMargin = true
                            },
                            new AdaptiveText() { Text = entry.Title },
                            new AdaptiveText() { Text = entry.Content, HintWrap = true, HintMaxLines = 3 },
                            new AdaptiveText() { Text = $"摄影  {entry.Author}" }
                        },
                        Attribution = new ToastGenericAttributionText() { Text = "图说虎溪" }
                    }
                }
            };

            var toast = new ToastNotification(imageToast.GetXml());
            toast.SuppressPopup = true;
            toast.ExpirationTime = DateTime.Now.AddDays(1);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
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

        static bool IsToastScheduled
        {
            get
            {
                bool valid = CommonResources.LoadSetting("toastLastSchedule", out string dateStr);
                if(dateStr == "") { return false; }
                string[] segments = dateStr.Split('.');
                DateTime lastScheduled = new DateTime(int.Parse(segments[0]), int.Parse(segments[1]), int.Parse(segments[2]));
                if(lastScheduled == DateTime.Today)
                {
                    return true;
                }
                return false;
            }
            set
            {
                if(value == true)
                {
                    DateTime today = DateTime.Today;
                    CommonResources.SaveSetting("toastLastSchedule", $"{today.Year}.{today.Month}.{today.Day}");
                }
                else
                {
                    CommonResources.SaveSetting("toastLastSchedule", $"1.1.1");
                }
            }
        }

    }
}
