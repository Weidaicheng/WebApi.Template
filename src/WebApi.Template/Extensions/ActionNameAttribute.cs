using System;

namespace WebApi.Template.Extensions
{
    public class ActionNameAttribute : Attribute
    {
        public string Name { get; set; }

        public ActionNameAttribute(string name)
        {
            Name = name;
        }
    }
}