﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;
using Model = DL444.UcquLibrary.Models;

namespace UCqu
{
    static class ScheduleNotificationUpdateTasks
    {
        public static async Task UpdateTile(Model.Schedule schedule)
        {
            TileUpdater updater = TileUpdateManager.CreateTileUpdaterForApplication();
            if (schedule == null)
            {
                updater.Clear();
                return;
            }
            //Windows.Foundation.Diagnostics.LoggingChannel lc = new Windows.Foundation.Diagnostics.LoggingChannel("UCQU_BackgroundTaskPayload", null, new Guid("4bd2826e-54a1-4ba9-bf63-92b73ea1ac4a"));
            //lc.LogMessage("Entered Payload Method.");
            //lc.LogMessage("Tile Updated Created.");
            string id = "", pwdHash = "";
            if(Login.LoadCredentials(out id, out pwdHash) == false)
            {
                //lc.LogMessage("Credential Load Failed. Clearing tile and exiting...");
                updater.Clear();
                return;
            }

            TileContent content = null;
            //lc.LogMessage("Obtaining Schedule.");
            //lc.LogMessage("Getting Schedule of Today.");
            List<Model.ScheduleEntry> entries = schedule.GetDaySchedule((DateTime.Today /*CommonResources.TestDate*/ - RuntimeData.StartDate).Days).ToList();

            entries.RemoveAll(x =>
            {
                (_, var endTime) = SessionTimeConverter.ConvertShort(x.SessionSpan, CampusSelector.IsCampusD(x.Room));
                return endTime.GetDateTime() < DateTime.Now;
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
                foreach (Model.ScheduleEntry e in entries)
                {
                    (var start, var end) = SessionTimeConverter.ConvertShort(e.SessionSpan, CampusSelector.IsCampusD(e.Room));

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

                if (!IsToastScheduled)
                {
                    try
                    {
                        ScheduleToast(huxiImgEntries[index]);
                    }
                    catch (Exception ex) { }
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

        public static void ScheduleToast(List<Model.ScheduleEntry> entries)
        {
            bool courseSwitch = true;
            bool dailySwitch = true;
            if (RuntimeData.LoadSetting("courseToastSwitch", out string _cSwitch) == false) { RuntimeData.SaveSetting("courseToastSwitch", "on"); }
            else { if (_cSwitch == "off") { courseSwitch = false; } }
            if (RuntimeData.LoadSetting("dailyToastSwitch", out string _dSwitch) == false) { RuntimeData.SaveSetting("dailyToastSwitch", "on"); }
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
                (var startTimeSch, _) = SessionTimeConverter.ConvertShort(entries[i].SessionSpan, CampusSelector.IsCampusD(entries[i].Room));
                DateTime startTime = startTimeSch.GetDateTime();
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
            if(RuntimeData.LoadSetting("imgToastSwitch", out string _switch) == false) { RuntimeData.SaveSetting("imgToastSwitch", "on"); }
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

        static bool IsToastScheduled
        {
            get
            {
                bool valid = RuntimeData.LoadSetting("toastLastSchedule", out string dateStr);
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
                    RuntimeData.SaveSetting("toastLastSchedule", $"{today.Year}.{today.Month}.{today.Day}");
                }
                else
                {
                    RuntimeData.SaveSetting("toastLastSchedule", $"1.1.1");
                }
            }
        }

    }
}
