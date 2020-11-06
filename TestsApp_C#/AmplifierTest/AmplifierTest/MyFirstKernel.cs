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

        [OpenCLKernel]
        void HeatMapCalculator([Global] double[] resCalculate,
            [Global] int[] width, [Global] int[] height,
            [Global] float[] widthTerrain, [Global] float[] heightTerrain,
            [Global] float[] destinationX, [Global] float[] destinationY,
            [Global] int[] x, [Global] int[] y)
        {
            int indexGpu = get_global_id(0);

            float destXInHeatmap = (float)((float)destinationX[indexGpu] / widthTerrain[indexGpu] + 0.5) * (width[indexGpu] - 1);  //-1 car on a augmenté la taille de 1 pour avoir une figure symétrique
            float destYInHeatmap = (float)((float)destinationY[indexGpu] / heightTerrain[indexGpu] + 0.5) * (height[indexGpu] - 1);  //-1 car on a augmenté la taille de 1 pour avoir une figure symétrique

            float normalizer = height[indexGpu];
            float calculation = 1 - rsqrt((destXInHeatmap - x[indexGpu]) * (destXInHeatmap - x[indexGpu]) + (destYInHeatmap - y[indexGpu]) * (destYInHeatmap - y[indexGpu])) / normalizer;
            if (calculation > 0)
                resCalculate[indexGpu] = calculation;
            else
                resCalculate[indexGpu] = 0;
        }
    }
}
