using System;
using System.Collections.Generic;
using System.Text;

namespace RakeLib
{
    public class Quotas
    {
        public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(2);

        public int Memory { get; set; } = 1024 * 1024;
    }
}