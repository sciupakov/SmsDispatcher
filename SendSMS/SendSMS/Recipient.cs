using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace SendSMS
{
    public class Recipient
    {
        public bool IsSent { get; set; } = false;
        public string Number { get; set; }
        public string Text { get; set; }
        public Guid UniqueID { get; set; }

        public Recipient(Guid id, string number, string text)
        {
            UniqueID = id;
            Number = number;
            Text = text;
        }
    }
}