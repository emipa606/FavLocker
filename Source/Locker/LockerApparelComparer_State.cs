using System.Collections.Generic;

namespace Locker
{
    public class LockerApparelComparer_State : IComparer<LockerApparel>
    {
        public int Compare(LockerApparel x, LockerApparel y)
        {
            var num = x is { OtherPawnWearing: true } ? 1 : 0;
            var num2 = y is { OtherPawnWearing: true } ? 1 : 0;
            return num - num2;
        }
    }
}