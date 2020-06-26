using System;
using System.Collections;
using System.Collections.Generic;

namespace WebApi.Template.Models
{
    public class ReflectionCache
    {
        public IEnumerable<Type> AllControllers { get; set; }

        public IEnumerable<string> AllApiVersions { get; set; }
    }
}