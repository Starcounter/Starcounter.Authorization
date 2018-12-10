using Starcounter;

namespace Application
{
    [Database]
    public class UserGroupMember
    {
        public User WhatIs { get; set; }
        public UserGroup ToWhat { get; set; }
    }
}