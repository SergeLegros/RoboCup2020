using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amplifier;
using Amplifier.OpenCL;

namespace AmplifierTest
{
    class MyFirstKernel : OpenCLFunctions
    {
        [OpenCLKernel]
        void Add1([Global] int[] a)
        {
            int indexGpu = get_global_id(0);
            a[indexGpu] = a[indexGpu] + 1;
        }

        [OpenCLKernel]
        void Matrix([Global] double[] x, [Global] double[] y)
        {
            int indexGpu = get_global_id(0);
            x[indexGpu]++;
            y[indexGpu] += 2;
        }
    }
}
