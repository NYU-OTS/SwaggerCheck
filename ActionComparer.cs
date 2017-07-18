using System;
using System.Collections.Generic;
using System.Text;

namespace APICheck
{
    class ActionComparer : IEqualityComparer<Action>
    {
        public bool Equals(Action x, Action y)
        {
            return x.Route == y.Route && x.Httpmethods.SetEquals(y.Httpmethods);
        }

        public int GetHashCode(Action obj)
        {
            return 0;
        }
    }
}
