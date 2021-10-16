﻿using System;
using System.Diagnostics;
using csFastFloat;
using System.Globalization;

namespace BenchmarkHandCoded
{
  internal class Program
  {
    public delegate double ParsingFunc(string[] values);
    public delegate double ParsingFuncUTF8(byte[][] values);

    static internal Tuple<double, double> time_it_ns<T>(string[] lines, ParsingFunc sut, long repeat)
    {
      double average = 0;
      double min_value = double.MaxValue;

      //  warmup
      for (int i = 0; i != 250; i++)
      {

        sut(lines);
      }


      Stopwatch sw = new Stopwatch();

      for (int i = 0; i != repeat; i++)
      {
        // ...; 

        sw.Restart();
        sut(lines);

        sw.Stop();


        // NOTE : Elapsed.TotalMilliseconds (double) returns the total number of whole and fractional milliseconds elapsed since inception
        //        Elapsed.Milliseconds (int) returns the number of whole milliseconds in the current second => this is absolutely not what we need !

        var dif = sw.Elapsed.TotalMilliseconds * 1000000;

        average += dif;


        min_value = min_value < dif ? min_value : dif;
      }

      average /= repeat;
      return new Tuple<double, double>(min_value, average);
    }
    static internal Tuple<double, double> time_it_ns_ut8<T>(byte[][] lines, ParsingFuncUTF8 sut, long repeat)
    {
      double average = 0;
      double min_value = double.MaxValue;

      //  warmup
      for (int i = 0; i != 250; i++)
      {

        sut(lines);
      }


      Stopwatch sw = new Stopwatch();

      for (int i = 0; i != repeat; i++)
      {
        // ...; 

        sw.Restart();
        sut(lines);

        sw.Stop();

        // NOTE : Elapsed.TotalMilliseconds (double) returns the total number of whole and fractional milliseconds elapsed since inception
        //        Elapsed.Milliseconds (int) returns the number of whole milliseconds in the current second => this is absolutely not what we need !


        var dif = sw.Elapsed.TotalMilliseconds * 1000000;

        average += dif;


        min_value = min_value < dif ? min_value : dif;
      }

      average /= repeat;
      return new Tuple<double, double>(min_value, average);
    }

    internal static string[] GetLinesFromFile(string fileName) =>
        System.IO.File.ReadAllLines(fileName);

    private static double find_max_fast_float(string[] lines)
    {

      double max = double.MinValue;

      foreach (string l in lines)
      {
        double x = FastDoubleParser.ParseDouble(l);
        max = max > x ? max : x;
      }

      return max;
    }

    private static double find_max_fast_float_try(string[] lines)
    {

      double max = double.MinValue;

      foreach (string l in lines)
      {

        if (FastDoubleParser.TryParseDouble(l, out double x))
        {
          max = max > x ? max : x;
        }
        else
        {
          Console.WriteLine("bug");

        }
      }

      return max;
    }

    private static double find_max_fast_float_utf8(byte[][] lines)
    {

      double max = double.MinValue;
      foreach (var l in lines)
      {
        double x = FastDoubleParser.ParseDouble(l);
        max = max > x ? max : x;
      }

      return max;
    }

    private static double find_max_fast_float_try_utf8(byte[][] lines)
    {

      double max = double.MinValue;

      foreach (var l in lines)
      {
        if (FastDoubleParser.TryParseDouble(l, out double x))
        {
          max = max > x ? max : x;
        }
        else
        {
          Console.WriteLine("bug");

        }
      }

      return max;
    }



    private static double find_max_double_parse(string[] lines)
    {
      double max = double.MinValue;
      foreach (string l in lines)
      {
        double x = double.Parse(l, CultureInfo.InvariantCulture);
        max = max > x ? max : x;
      }

      return max;
    }

    static private void pretty_print(double volume, uint number_of_floats, string name, Tuple<double, double> result)
    {
      double volumeMB = volume / (1024.0 * 1024.0);
      Console.Write("{0,-40}: {1,8:f2} MB/s (+/- {2:f1} %) ", name, volumeMB * 1000000000 / result.Item1, (result.Item2 - result.Item1) * 100.0 / result.Item2);
      Console.Write("{0,8:f2} Mfloat/s  ", number_of_floats * 1000 / result.Item1);
      Console.Write(" {0,8:f2} ns/f \n", (double)result.Item1 / number_of_floats);
    }

    private static void Main(string[] args)
    {
      string fileName = @"data/canada.txt";
      var lines = GetLinesFromFile(fileName);
      var _linesUtf8 = Array.ConvertAll(lines, System.Text.Encoding.UTF8.GetBytes);
      int volume = 0;
      foreach (string l in lines)
      {
        volume += l.Length;
      }


      Console.WriteLine("Canada.txt");
      Console.WriteLine("--------------------------");
      double volumeMB = volume / (1024.0 * 1024.0);
      Console.WriteLine($"Volume : {volumeMB}");

      process_test(lines, _linesUtf8, (double)volume);


      Console.WriteLine("");
      Console.WriteLine("");

      Console.WriteLine("Mesh.txt");
      Console.WriteLine("--------------------------");

      lines = GetLinesFromFile(@"data/mesh.txt");
      _linesUtf8 = Array.ConvertAll(lines, System.Text.Encoding.UTF8.GetBytes);
      volume = 0;
      foreach (string l in lines)
      {
        volume += l.Length;
      }
      volumeMB = volume / (1024.0 * 1024.0);
      Console.WriteLine($"Volume : {volumeMB}");

      process_test(lines, _linesUtf8, (double)volume);


      Console.WriteLine("");
      Console.WriteLine("");

      Console.WriteLine("Sythetic.txt");
      Console.WriteLine("--------------------------");

      lines = GetLinesFromFile(@"data/synthetic.txt");
      _linesUtf8 = Array.ConvertAll(lines, System.Text.Encoding.UTF8.GetBytes);

      volume = 0;
      foreach (string l in lines)
      {
        volume += l.Length;
      }
      volumeMB = volume / (1024.0 * 1024.0);
      Console.WriteLine($"Volume : {volumeMB}");

      process_test(lines, _linesUtf8, (double)volume);


    }

    private static void process_test(string[] lines, byte[][] linesUTF8, double volume)
    {

      pretty_print(volume, (uint)lines.Length, "Double.Parse", time_it_ns<double>(lines, find_max_double_parse, 100));
      pretty_print(volume, (uint)lines.Length, "FastParser.ParseDouble", time_it_ns<double>(lines, find_max_fast_float, 100));
      pretty_print(volume, (uint)lines.Length, "FastParser.TryParseDouble", time_it_ns<double>(lines, find_max_fast_float_try, 100));

      pretty_print(volume, (uint)lines.Length, "FastParser.ParseDouble UT8", time_it_ns_ut8<double>(linesUTF8, find_max_fast_float_utf8, 100));
      pretty_print(volume, (uint)lines.Length, "FastParser.TryParseDouble UT8", time_it_ns_ut8<double>(linesUTF8, find_max_fast_float_try_utf8, 100));


    }

  }
}