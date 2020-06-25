﻿using System.Collections.Generic;

namespace Norns.Destiny.Notations
{
    public class ParameterNotation : MembersNotation
    {
        public string Type { get; set; }
        public string Name { get; set; }

        public override IEnumerable<INotation> GetMembers()
        {
            yield return Type.ToNotation();
            yield return ConstNotations.Blank;
            yield return Name.ToNotation();
        }
    }
}