using Amplifier;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmplifierTest
{
    class Program
    {
        static OpenCLCompiler Compiler = new OpenCLCompiler();
        static dynamic exec;
        static void Main(string[] args)
        {
            WriteLine("\n----List Devices----", ConsoleColor.Yellow);
            foreach (var item in Compiler.Devices)
            {
                WriteLine(item.ToString(), ConsoleColor.Green);
            }
            //Write("GPU Number : ", ConsoleColor.Yellow);
            //var key = Console.ReadKey();
            //WriteLine();
            //Compiler.UseDevice(int.Parse(key.KeyChar.ToString()));
            //exec = Compiler.GetExec();
            Compiler.UseDevice(0);
            Compiler.CompileKernel(typeof(MyFirstKernel));
            WriteLine("\n----List Kernels----", ConsoleColor.Yellow);
            foreach (var item in Compiler.Kernels)
            {
                WriteLine(item, ConsoleColor.Green);
            }
            WriteLine();
            WriteLine();
            foreach (var item in Compiler.Devices)
            {
                Compiler.UseDevice(item.ID);
                Compiler.CompileKernel(typeof(MyFirstKernel));
                exec = Compiler.GetExec();
                WriteLine("Executed on device : " + item.ToString(), ConsoleColor.Red);
                //WriteLine("Execute Add1 :", ConsoleColor.Yellow);
                //ExecuteAdd1();

                //WriteLine("\n\nExecute Matrix :", ConsoleColor.Yellow);
                //ExecuteMatrix();

                WriteLine("Execute Simple For :", ConsoleColor.Yellow);
                ExecuteSimpeFor(1000000);
                WriteLine();
                WriteLine();

                //WriteLine("\n\nExecute HeatMap on CPU basic:", ConsoleColor.Yellow);
                //double[,] heatMap = new double[22, 33];
                //ExecuteHeatMapBasic(heatMap, 33, 22, 3, 2, 0, 0);

                //WriteLine("\n\nExecute HeatMap :", ConsoleColor.Yellow);
                //heatMap = new double[22, 33];
                //ExecuteHeatMap(heatMap, 33,22,3,2,0,0);

                //WriteLine("\n\nExecute Full HeatMap :", ConsoleColor.Yellow);
                //heatMap = new double[22, 33];
                //ExecuteFullHeatMap(heatMap, 33, 22, 3, 2, 0, 0);
            }

            Compiler.Dispose();
            WriteLine("Press any key to close",ConsoleColor.Red, true);
        }

        static void ExecuteHeatMap(double[,] heatMap, int width, int height,
            float widthTerrain, float heightTerrain, float destinationX, float destinationY)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            exec = Compiler.GetExec();
            var resCalculate = new XArray(new double[1]);
            var widthXArray = new XArray(new int[1] { width });
            var heightXArray = new XArray(new int[1] { height });
            var widthTerrainXArray = new XArray(new float[1] { widthTerrain });
            var heightTerrainXArray = new XArray(new float[1] { heightTerrain });
            var destinationXXArray = new XArray(new float[1] { destinationX });
            var destinationYXArray = new XArray(new float[1] { destinationY });
            var xXArray = new XArray(new int[1] { 0 });
            var yXArray = new XArray(new int[1] { 0 });
            WriteLine("Get execution...", ConsoleColor.Blue);
            WriteLine("Execute...", ConsoleColor.Blue);
            //Parallel.For(0, height, y =>
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    //Calcul de la fonction de cout de stratégie
                    xXArray[0] = x;
                    yXArray[0] = y;
                    exec.HeatMapCalculator(resCalculate,
                        widthXArray, heightXArray,
                        widthTerrainXArray, heightTerrainXArray,
                        destinationXXArray, destinationYXArray,
                        xXArray, yXArray);
                    heatMap[y, x] = resCalculate[0];
                }
            }
            WriteLine("Get result...", ConsoleColor.Blue);
            sw.Stop();
            //string resultXString = resultX.GetValue(0).ToString();
            //string resultYString = resultY.GetValue(0).ToString();
            //WriteLine("Result X : " + resultXString, ConsoleColor.Green);
            //WriteLine("Result Y : " + resultYString, ConsoleColor.Green);
            WriteLine("Process Time : " + sw.ElapsedMilliseconds + "ms", ConsoleColor.Gray);
            sw.Reset();
        }

        static void ExecuteFullHeatMap(double[,] heatMap, int width, int height,
            float widthTerrain, float heightTerrain, float destinationX, float destinationY)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            exec = Compiler.GetExec();
            var heatMapD1 = new XArray(new double[height]);
            var heatMapD2 = new XArray(new double[width]);
            var widthXArray = new XArray(new int[1] { width });
            var heightXArray = new XArray(new int[1] { height });
            var widthTerrainXArray = new XArray(new float[1] { widthTerrain });
            var heightTerrainXArray = new XArray(new float[1] { heightTerrain });
            var destinationXXArray = new XArray(new float[1] { destinationX });
            var destinationYXArray = new XArray(new float[1] { destinationY });
            var xXArray = new XArray(new int[1] { 0 });
            var yXArray = new XArray(new int[1] { 0 });
            WriteLine("Get execution...", ConsoleColor.Blue);
            WriteLine("Execute...", ConsoleColor.Blue);
            exec.FullHeatMapCalculator(heatMapD1, heatMapD2,
                        widthXArray, heightXArray,
                        widthTerrainXArray, heightTerrainXArray,
                        destinationXXArray, destinationYXArray);
            WriteLine("Get result...", ConsoleColor.Blue);
            sw.Stop();
            //string resultXString = resultX.GetValue(0).ToString();
            //string resultYString = resultY.GetValue(0).ToString();
            //WriteLine("Result X : " + resultXString, ConsoleColor.Green);
            //WriteLine("Result Y : " + resultYString, ConsoleColor.Green);
            WriteLine("Process Time : " + sw.ElapsedMilliseconds + "ms", ConsoleColor.Gray);
            sw.Reset();
        }

        static void ExecuteHeatMapBasic(double[,] heatMap, int width, int height,
            float widthTerrain, float heightTerrain, float destinationX, float destinationY)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            float destXInHeatmap = (float)((float)destinationX / widthTerrain + 0.5) * (width - 1);  //-1 car on a augmenté la taille de 1 pour avoir une figure symétrique
            float destYInHeatmap = (float)((float)destinationY / heightTerrain + 0.5) * (height - 1);  //-1 car on a augmenté la taille de 1 pour avoir une figure symétrique

            float normalizer = height;

            Parallel.For(0, height, y =>
            {
                for (int x = 0; x < width; x++)
                {
                    //Calcul de la fonction de cout de stratégie
                    heatMap[y, x] = Math.Max(0, 1 - Math.Sqrt((destXInHeatmap - x) * (destXInHeatmap - x) + (destYInHeatmap - y) * (destYInHeatmap - y)) / normalizer);
                }
            });
            sw.Stop();
            WriteLine("Process Time : " + sw.ElapsedMilliseconds + "ms", ConsoleColor.Gray);
            sw.Reset();
        }

        static void ExecuteMatrix()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var testX = new XArray(new double[1]);
            var testY = new XArray(new double[1]);
            WriteLine("Get execution...", ConsoleColor.Blue);
            //var exec = Compiler.GetExec();
            WriteLine("Execute...", ConsoleColor.Blue);
            exec.Matrix(testX, testY);
            WriteLine("Get result...", ConsoleColor.Blue);
            var resultX = testX.ToArray();
            var resultY = testY.ToArray();
            sw.Stop();
            string resultXString = resultX.GetValue(0).ToString();
            string resultYString = resultY.GetValue(0).ToString();
            WriteLine("Result X : " + resultXString, ConsoleColor.Green);
            WriteLine("Result Y : " + resultYString, ConsoleColor.Green);
            WriteLine("Process Time : " + sw.ElapsedMilliseconds + "ms", ConsoleColor.Gray);
            sw.Reset();
        }

        static void ExecuteSimpeFor(int iterNb)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var value = new XArray(new double[1], Direction.Output);
            var iterationNb = new XArray(new int[1] { iterNb }, Direction.Input);
            WriteLine("Get execution...", ConsoleColor.Blue);
            //var exec = Compiler.GetExec();
            WriteLine("Execute...", ConsoleColor.Blue);
            exec.SimpleFor(value, iterationNb);
            WriteLine("Get result...", ConsoleColor.Blue);
            var result = value.ToArray();
            sw.Stop();
            string resultXString = result.GetValue(0).ToString();
            WriteLine("Result : " + resultXString, ConsoleColor.Green);
            WriteLine("Process Time : " + sw.ElapsedMilliseconds + "ms", ConsoleColor.Gray);
            sw.Reset();
        }

        static void ExecuteAdd1()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var test = new XArray(new int[] { 1 });
            string originalValue = test[0].ToString();
            WriteLine("Get execution...", ConsoleColor.Blue);
            //var exec = Compiler.GetExec();
            WriteLine("Execute...", ConsoleColor.Blue);
            exec.Add1(test);
            WriteLine("Get result...", ConsoleColor.Blue);
            var result = test.ToArray();
            sw.Stop();
            string resultString = result.GetValue(0).ToString();
            WriteLine("Original value : " + originalValue, ConsoleColor.Green);
            WriteLine("Result : " + resultString, ConsoleColor.Green);
            WriteLine("Process Time : " + sw.ElapsedMilliseconds + "ms", ConsoleColor.Gray);
            sw.Reset();
        }

        static void WriteLine(string text = "", ConsoleColor color = ConsoleColor.White, bool wait = false)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);

            if (wait)
            {
                Console.ReadKey();
            }
        }
        static void Write(string text = "", ConsoleColor color = ConsoleColor.White, bool wait = false)
        {
            Console.ForegroundColor = color;
            Console.Write(text);

            if (wait)
            {
                Console.ReadKey();
            }
        }
    }
}
