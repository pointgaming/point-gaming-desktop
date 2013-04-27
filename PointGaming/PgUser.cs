﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PointGaming.POCO;
using RestSharp;
using SocketIOClient;
using SocketIOClient.Messages;

namespace PointGaming
{
    public interface IBetOperand : INotifyPropertyChanged
    {
        string Id { get; }
        string ShortDescription { get; }
        string PocoType { get; }
    }

    public class PgTeam : IBetOperand
    {
        public string PocoType { get { return "Team"; } }

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
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name)
                    return;
                _name = value;
                NotifyChanged("Name");
                NotifyChanged("ShortDescription");
            }
        }

        public string ShortDescription { get { return _name; } }
    }

    public class PgUser : IBetOperand
    {
        public string PocoType { get { return "User"; } }
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

        private string _username;
        public string Username
        {
            get { return _username; }
            set
            {
                if (value == _username)
                    return;
                _username = value;
                NotifyChanged("Username");
                NotifyChanged("ShortDescription");
            }
        }

        public string ShortDescription { get { return _username; } }

        private string _status;
        public string Status
        {
            get { return _status; }
            set
            {
                if (value == _status)
                    return;
                _status = value;
                NotifyChanged("Status");
            }
        }

        public UserBase ToUserBase()
        {
            return new UserBase { _id = Id, username = Username, };
        }

        public override bool Equals(object obj)
        {
            var other = obj as PgUser;
            if (other == null)
                return false;
            return _id == other._id;
        }
        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
        public override string ToString()
        {
            return Username;
        }
    }

}