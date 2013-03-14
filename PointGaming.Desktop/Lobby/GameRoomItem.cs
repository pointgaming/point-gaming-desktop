using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Controls;
using System.Diagnostics;
using System.ComponentModel;

namespace PointGaming.Desktop.Lobby
{
    public class GameRoomItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyChanged(string propertyName)
        {
            var changedCallback = PropertyChanged;
            if (changedCallback == null)
                return;
            var args = new PropertyChangedEventArgs(propertyName);
            changedCallback(this, args);
        }

        private string _id;
        public string Id
        {
            get { return _id; }
            set
            {
                if (value == _id)
                    return;
                _id = value;
                NotifyChanged("Id");
            }
        }

        private int _position;
        public int Position
        {
            get { return _position; }
            set
            {
                if (value == _position)
                    return;
                _position = value;
                NotifyChanged("Position");
                NotifyChanged("DisplayName");
            }
        }
        private bool _isLocked;
        public bool IsLocked
        {
            get { return _isLocked; }
            set
            {
                if (value == _isLocked)
                    return;
                _isLocked = value;
                NotifyChanged("IsLocked");
                NotifyChanged("DisplayName");
            }
        }
        public string DisplayName { get { return "Game " + _position + (_isLocked ? " (Locked)" : ""); } }

        private int _memberCount;
        public int MemberCount
        {
            get { return _memberCount; }
            set
            {
                if (value == _memberCount)
                    return;
                _memberCount = value;
                NotifyChanged("MemberCount");
                NotifyChanged("MemberStatus");
            }
        }
        private int _maxMemberCount;
        public int MaxMemberCount
        {
            get { return _maxMemberCount; }
            set
            {
                if (value == _maxMemberCount)
                    return;
                _maxMemberCount = value;
                NotifyChanged("MaxMemberCount");
                NotifyChanged("MemberStatus");
            }
        }
        public string MemberStatus { get { return _memberCount + "/" + MaxMemberCount; } }

        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                if (value == _description)
                    return;
                _description = value;
                NotifyChanged("Description");
                NotifyChanged("DescriptionDocument");
            }
        }

        public FlowDocument DescriptionDocument
        {
            get
            {
                var p = new Paragraph();
                Chat.ChatTabCommon.Format(_description, p.Inlines);

                var doc = new FlowDocument();
                doc.SetPointGamingDefaults();
                doc.Blocks.Add(p);
                return doc;
            }
        }
    }
}
