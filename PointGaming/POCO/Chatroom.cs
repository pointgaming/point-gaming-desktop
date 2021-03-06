﻿using System.Collections.Generic;

namespace PointGaming.POCO
{
	public class Chatroom
	{
		public string _id { get; set; }
	}
    public class ChatroomWithPassword : Chatroom
    {
        public string password { get; set; }
    }
    public class ChatroomMessageOut
    {
        public string _id { get; set; }
        public string message { get; set; }
    }
    public class ChatroomInviteOut
    {
        public string _id { get; set; }
        public UserBase toUser { get; set; }
    }
    public class ChatroomUserList
    {
        public List<string> chatrooms { get; set; }
    }
    public class ChatroomMemberList
    {
        public string _id { get; set; }
        public List<UserBase> members { get; set; }
    }
    public class ChatroomMemberChange
    {
        public string _id { get; set; }
        public UserBase user { get; set; }
        public string status { get; set; }
    }
    public class ChatroomMessageNew
    {
        public string _id { get; set; }
        public UserBase fromUser { get; set; }
        public string message { get; set; }
    }
    public class ChatroomInviteNew
    {
        public string _id { get; set; }
        public UserBase fromUser { get; set; }
    }
}
