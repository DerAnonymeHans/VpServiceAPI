namespace VpServiceAPI.Entities.Plan
{
    public sealed class WhatPlan
    {
        public int Number { get; private set; }

        public bool First => Number == 0;
        public bool NotFirst => Number != 0;
        public WhatPlan(int number)
        {
            Number = number;
        }

        public void NextPlan()
        {
            Number++;
        }
    }
}
