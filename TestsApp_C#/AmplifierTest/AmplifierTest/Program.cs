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
        static void Main(string[] args)
        {
            Console.WriteLine("\nList Devices----");
            foreach (var item in Compiler.Devices)
            {
                Console.WriteLine(item);
            }
            Write("GPU Number : ", ConsoleColor.Yellow);
            var key = Console.ReadKey();
            WriteLine();
            Compiler.UseDevice(int.Parse(key.KeyChar.ToString()));
            Compiler.CompileKernel(typeof(MyFirstKernel));
            WriteLine("\nList Kernels", ConsoleColor.Yellow);
            foreach (var item in Compiler.Kernels)
            {
                WriteLine(item, ConsoleColor.Green);
            }
            WriteLine("Execute Add1 :");
            ExecuteAdd1();
            WriteLine("\n\nExecute Matrix :");
            ExecuteMatrix();
            WriteLine("Press any key to close",ConsoleColor.Red, true);
        }

        static void ExecuteMatrix()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var testX = new XArray(new double[1]);
            var testY = new XArray(new double[1]);
            WriteLine("Get execution...", ConsoleColor.Blue);
            var exec = Compiler.GetExec();
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

        static void ExecuteAdd1()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var test = new XArray(new int[] { 1 });
            string originalValue = test[0].ToString();
            WriteLine("Get execution...", ConsoleColor.Blue);
            var exec = Compiler.GetExec();
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
