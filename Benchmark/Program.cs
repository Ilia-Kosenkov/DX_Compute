﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkDotNet.Running.BenchmarkRunner.Run<Interop_1>();
        }
    }
}
