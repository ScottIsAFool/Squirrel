﻿using Telerik.Windows.Controls;

namespace Squirrel.Services
{
    public class ReviewReminderService : RadRateApplicationReminder
    {
        private static ReviewReminderService _current;

        public static ReviewReminderService Current
        {
            get
            {
                return _current ?? (_current = new ReviewReminderService());
            }
        }

        public ReviewReminderService()
        {
            RecurrencePerUsageCount = 8;
            SkipFurtherRemindersOnYesPressed = true;
            AllowUsersToSkipFurtherReminders = true;
        }
    }
}
