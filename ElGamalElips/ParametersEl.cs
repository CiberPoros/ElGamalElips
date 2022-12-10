using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using BigMath;
using System.IO;
using static ObjectExtensionsNameSpace.ObjectExtensions;
using static System.Console;

namespace ElGamalElips
{
    public class ParametersEl
    {
        BigInteger _val;
        bool _isPoint;
        BigInteger _x;
        BigInteger _y;
        string _name;

        public BigInteger Val { get => _val; set => _val = value; }
        public bool IsPoint { get => _isPoint; set => _isPoint = value; }
        public BigInteger X { get => _x; set => _x = value; }
        public BigInteger Y { get => _y; set => _y = value; }
        public string Name { get => _name; set => _name = value; }

        public ParametersEl(BigInteger val, string name)
        {
            _val = val;
            _isPoint = false;
            _name = name;
        }

        public ParametersEl(BigInteger x, BigInteger y, string name)
        {
            _x = x;
            _y = y;
            _isPoint = true;
            _name = name;
        }

        public ParametersEl(F_int x, F_int y, string name)
        {
            if (x.IsNull())
            {
                _x = -1;
                _y = -1;
            }
            else
            {
                _x = x._val;
                _y = y._val;
            }

            _isPoint = true;
            _name = name;
        }

        public static bool ReadInFile(string fileName, __arglist)
        {
            string[] temp = new string[0]; 

            try { temp = File.ReadAllLines(fileName, Encoding.Default); }
            catch { throw new Exception("Файл \"" + fileName + "\" не существует или не доступен для чтения!"); }

            ArgIterator iterator = new ArgIterator(__arglist);

            string s_out = "";
            while (iterator.GetRemainingCount() > 0)
            {
                TypedReference r = iterator.GetNextArg();

                int index = -1;
                for (int i = 0; i < temp.Length; i++)
                {
                    if (temp[i].Split(' ')[0] == __refvalue(r, ParametersEl).Name)
                    {
                        index = i;
                        break;
                    }
                }

                if (index == -1)
                    throw new Exception("Описание параметра " + __refvalue(r, ParametersEl).Name + " отсутствует в файле \"" + fileName + "\"");

                if (temp[index].Split(' ')[2].Split(';').Length == 2)
                {
                    __refvalue(r, ParametersEl).IsPoint = true;
                    __refvalue(r, ParametersEl).X = BigInteger.Parse(temp[index].Split(' ')[2].Split(';')[0].Trim(new char[] { '(', ')' }));
                    __refvalue(r, ParametersEl).Y = BigInteger.Parse(temp[index].Split(' ')[2].Split(';')[1].Trim(new char[] { '(', ')' }));
                }
                else
                {
                    __refvalue(r, ParametersEl).IsPoint = false;
                    __refvalue(r, ParametersEl).Val = BigInteger.Parse(temp[index].Split(' ')[2]);
                }

                s_out = s_out + __refvalue(r, ParametersEl).Name + ", ";
            }

            s_out = s_out.Substring(0, s_out.Length - 2);
            WriteLine("Из файла \"" + fileName + "\" были получены параметры: " + s_out);
            WriteLine();

            return true;
        }

        public static bool OutToFile(string fileName, params ParametersEl[] values)
        {
            string[] temp = new string[values.Length];
            string[] names = new string[values.Length];

            string s_out = "";
            for (int i = 0; i < values.Length; i++)
            { 
                temp[i] = values[i].ToString();
                names[i] = values[i].Name;
                s_out = s_out + values[i].Name + ", ";
            }

            string[] temp_read = new string[1];
            try
            {
                temp_read = File.ReadAllLines(fileName, Encoding.Default);
            }
            catch
            {
                temp_read = new string[0];
            }

            List<string> tempTo = new List<string>();

            for (int i = 0; i < temp_read.Length; i++)
            {
                if (temp_read[i].Length < 2)
                    continue;

                if (names.Contains(temp_read[i].Split(' ')[0]))
                    continue;

                tempTo.Add(temp_read[i]);
            }

            foreach (string s in temp)
                tempTo.Add(s);

            File.WriteAllLines(fileName, tempTo.ToArray(), Encoding.Unicode);

            s_out = s_out.Substring(0, s_out.Length - 2);
            WriteLine("В файл \"" + fileName + "\" были сохранены параметры: " + s_out);
            WriteLine();

            return true;
        }

        public override string ToString()
        {
            if (!_isPoint)
                return _name + " = " + _val.ToString();
            else
                return _name + " = (" + _x.ToString() + ";" + _y.ToString() + ")";
        }
    }
}
