using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Diagnostics;

namespace PointGaming.Desktop.Chat
{
    public class ChatTabCommon
    {
        public static bool FilterMessage(string messageIn, out string send, out string remain)
        {
            send = messageIn.Trim();

            remain = "";
            if (send.Length > 1024)
            {
                send = send.Substring(0, 1024);
                send = send.Trim();
                remain = send.Substring(1024);
            }

            return send != "";
        }

        public static void Format(string message, InlineCollection collection)
        {
            UrlMatch urlMatch;
            int cur = 0;

            while (UrlMatcher.TryGetMatch(message, cur, out urlMatch))
            {
                string before = message.Substring(cur, urlMatch.Offset - cur);
                if (before.Length > 0)
                    collection.Add(new Run(before));

                var matchRun = new Run(urlMatch.Text);
                try
                {
                    Hyperlink link = new Hyperlink(matchRun);
                    link.NavigateUri = new Uri(urlMatch.Text);
                    link.RequestNavigate += hlink_RequestNavigate;
                    collection.Add(link);
                }
                catch
                {
                    collection.Add(matchRun);
                }

                cur = urlMatch.Offset + urlMatch.Text.Length;
            }

            string ending = message.Substring(cur);
            if (ending.Length > 0)
                collection.Add(new Run(ending));
        }

        private static void hlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
