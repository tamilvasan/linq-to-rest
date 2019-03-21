﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Messerli.LinqToRest.Entities;

namespace Messerli.LinqToRest.Test.Stub
{
    public class EntityWithSimpleMembers : IEntity
    {
        public EntityWithSimpleMembers(string name, int number)
        {
            Name = name;
            Number = number;
        }

        public string Name { get; }

        public int Number { get; }

        public string UniqueIdentifier => Name;
    }
}