﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RakeLib
{
    public interface IOutputCompute
    {
        Task<string> ComputeAsync();
    }
}