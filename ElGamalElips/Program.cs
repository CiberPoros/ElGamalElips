using System;
using System.Text;
using System.Numerics;
using BigMath;
using static System.Console;
using static ElGamalElips.ParametersEl;
using BigMath.RandomNumbers;
using System.Security.Cryptography;
using System.IO;

namespace ElGamalElips
{
    class Program
    {
        static string publicParametersPath = "общие_параметры.txt";
        static string openKeyPath = "открытый_ключ.txt";
        static string closeKeyPath = "закрытый_ключ.txt";
        static string messagePath = "сообщение.txt";
        static string signaturePath = "подпись.txt";
        static string verifyCalc_H = "проверка1_вычисление_h(m)";
        static string verifyMulty_H_Q = "проверка2_вычисление_h(m)Q";
        static string verifyMulty_fR_P = "проверка3_вычисление_f(R)P";
        static string verifyMulty_s_R = "проверка4_вычисление_sR";
        static string verifySum_frP_sR = "проверка5_вычисление_f(R)P_plus_sR";

        static Random rnd = new Random();
        static RandomBigInteger rnd_big_integer = new RandomBigInteger();

        static void Main(string[] args)
        {
            for (; ; )
            {
                Console.WriteLine("1. Генерация общих параметров");
                Console.WriteLine("2. Генерация личных параметров");
                Console.WriteLine("3. Создание подписи");
                Console.WriteLine("4. Проверка подписи. Вычисление h(m)");
                Console.WriteLine("5. Проверка подписи. Вычисление h(m) * Q");
                Console.WriteLine("6. Проверка подписи. Вычисление f(R) * P");
                Console.WriteLine("7. Проверка подписи. Вычисление s * R");
                Console.WriteLine("8. Проверка подписи. Вычисление f(R) * p + s * R");
                Console.WriteLine("9. Проверка подписи. Проверка равенства h(m) * Q = f(R) * p + s * R");
                Console.WriteLine("0. Закрыть программу");
                Console.WriteLine();

                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        GeneratePublicParameters();
                        WriteLine();
                        break;
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        GeneratePrivateParameters();
                        WriteLine();
                        break;
                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        CreateSignature();
                        WriteLine();
                        break;
                    case ConsoleKey.D4:
                    case ConsoleKey.NumPad4:
                        CheckSignature1();
                        WriteLine();
                        break;
                    case ConsoleKey.D5:
                    case ConsoleKey.NumPad5:
                        CheckSignature2();
                        WriteLine();
                        break;
                    case ConsoleKey.D6:
                    case ConsoleKey.NumPad6:
                        CheckSignature3();
                        WriteLine();
                        break;
                    case ConsoleKey.D7:
                    case ConsoleKey.NumPad7:
                        CheckSignature4();
                        WriteLine();
                        break;
                    case ConsoleKey.D8:
                    case ConsoleKey.NumPad8:
                        CheckSignature5();
                        WriteLine();
                        break;
                    case ConsoleKey.D9:
                    case ConsoleKey.NumPad9:
                        CheckSignature6();
                        WriteLine();
                        break;
                    case ConsoleKey.D0:
                    case ConsoleKey.NumPad0:
                        return;
                }
            }
        }

        static void GeneratePublicParameters()
        {
            DeleteFilesByNames(publicParametersPath, openKeyPath, closeKeyPath, signaturePath, verifyCalc_H, verifyMulty_H_Q, verifyMulty_fR_P, verifyMulty_s_R, verifySum_frP_sR);

            var pars = ReadPar();
            BigInteger r = -1, p = -1;
            F_int A = null, q1 = null, q2 = null;

            while (r < 0 || r % 2 == 0 || !BigMath.Generator.IsPrimeByMillerRabin(r, 50, ref rnd))
                GeneratorEl.Gen(pars.Item1, pars.Item2, out p, out A, out q1, out q2, out r);

            OutToFile(publicParametersPath, new ParametersEl(p, "p"), new ParametersEl(A._val, "A"), new ParametersEl(q1, q2, "Q"), new ParametersEl(r, "r"));

            WriteLine("Генерация общих параметров завершена успешно!");
        }

        static void GeneratePrivateParameters()
        {
            DeleteFilesByNames(openKeyPath, closeKeyPath, signaturePath, verifyCalc_H, verifyMulty_H_Q, verifyMulty_fR_P, verifyMulty_s_R, verifySum_frP_sR);

            ParametersEl _p = new ParametersEl(0, "p"), _A = new ParametersEl(0, "A"), _r = new ParametersEl(0, "r"), Q = new ParametersEl(0, 0, "Q");

            try
            { 
                ReadInFile(publicParametersPath, __arglist(_p, _A, _r, Q));
            }
            catch (Exception e)
            {
                WriteLine(e.Message);
                return;
            }

            BigInteger l = rnd_big_integer.NextBigInteger(ref rnd, 1, _r.Val);

            F_int q1 = new F_int(Q.X, _p.Val), q2 = new F_int(Q.Y, _p.Val);

            GeneratorEl.MultiPointOnConst(q1, q2, l, new F_int(_A.Val, _p.Val), out F_int p1, out F_int p2);

            OutToFile(openKeyPath, new ParametersEl(p1, p2, "P"));
            OutToFile(closeKeyPath, new ParametersEl(l, "l"));

            WriteLine("Генерация индивидуальных параметров завершена успешно!");
        }

        static void CreateSignature()
        {
            DeleteFilesByNames(signaturePath, verifyCalc_H, verifyMulty_H_Q, verifyMulty_fR_P, verifyMulty_s_R, verifySum_frP_sR);

            ParametersEl _p = new ParametersEl(0, "p"), _A = new ParametersEl(0, "A"), _r = new ParametersEl(0, "r"), Q = new ParametersEl(0, 0, "Q"), _l = new ParametersEl(0, "l");

            try
            { 
                ReadInFile(publicParametersPath, __arglist(_p, _A, _r, Q));
                ReadInFile(closeKeyPath, __arglist(_l));
            }
            catch (Exception e)
            {
                WriteLine(e.Message);
                return;
            }

            BigInteger k = -1, h = -1;
            try
            {
                h = GetHash(File.ReadAllText(messagePath, Encoding.Default), _r.Val);
            }
            catch
            {
                WriteLine("Файл \"" + messagePath +  "\" отсутствует или не доступен для чтения!");
                return;
            }

            F_int q1 = new F_int(Q.X, _p.Val), q2 = new F_int(Q.Y, _p.Val), A = new F_int(_A.Val, _p.Val);
            F_int r1 = new F_int(0, _p.Val), r2 = new F_int(0, _p.Val);

            while (r1._val == 0)
            {
                k = rnd_big_integer.NextBigInteger(ref rnd, 1, _r.Val);
                GeneratorEl.MultiPointOnConst(q1, q2, k, A, out r1, out r2);
            }

            BigInteger l = _l.Val, r = _r.Val, p = _p.Val;

            BigInteger s = (((((((h - (l * r1._val) % r) % r) + r) % r) * F_int.GetInverseElement(k, r)) % r) + r) % r;

            OutToFile(signaturePath, new ParametersEl(r1, r2, "R"), new ParametersEl(s, "s"), new ParametersEl(h, "h"), new ParametersEl(k, "k"));

            WriteLine("Подпись сообщения создана успешно!");
        }

        static void CheckSignature1()
        {
            DeleteFilesByNames(verifyCalc_H, verifyMulty_H_Q, verifyMulty_fR_P, verifyMulty_s_R, verifySum_frP_sR);

            ParametersEl _r = new ParametersEl(0, "r");

            try
            { 
                ReadInFile(publicParametersPath, __arglist(_r));
            }
            catch (Exception e)
            {
                WriteLine(e.Message);
                return;
            }

            BigInteger h = -1;
            try
            {
                h = GetHash(File.ReadAllText(messagePath, Encoding.Default), _r.Val);
            }
            catch
            {
                WriteLine("Файл \"" + messagePath + "\" отсутствует или не доступен для чтения!");
                return;
            }

            OutToFile(verifyCalc_H, new ParametersEl(h, "h"));
        }

        static void CheckSignature2()
        {
            DeleteFilesByNames(verifyMulty_H_Q, verifyMulty_fR_P, verifyMulty_s_R, verifySum_frP_sR);

            ParametersEl _p = new ParametersEl(0, "p"), _A = new ParametersEl(0, "A"), Q = new ParametersEl(0, 0, "Q"), h = new ParametersEl(0, "h");

            try
            {
                ReadInFile(publicParametersPath, __arglist(_p, _A, Q));
                ReadInFile(verifyCalc_H, __arglist(h));
            }
            catch (Exception e)
            {
                WriteLine(e.Message);
                return;
            }

            GeneratorEl.MultiPointOnConst(new F_int(Q.X, _p.Val), new F_int(Q.Y, _p.Val), 
                h.Val, new F_int(_A.Val, _p.Val), out F_int left1, out F_int left2);

            OutToFile(verifyMulty_H_Q, new ParametersEl(left1, left2, "hQ"));
        }

        static void CheckSignature3()
        {
            DeleteFilesByNames(verifyMulty_fR_P, verifyMulty_s_R, verifySum_frP_sR);

            ParametersEl _p = new ParametersEl(0, "p"), _A = new ParametersEl(0, "A"), P = new ParametersEl(0, 0, "P"), R = new ParametersEl(0, 0, "R");

            try
            {
                ReadInFile(publicParametersPath, __arglist(_p, _A));
                ReadInFile(openKeyPath, __arglist(P));
                ReadInFile(signaturePath, __arglist(R));
            }
            catch (Exception e)
            {
                WriteLine(e.Message);
                return;
            }

            GeneratorEl.MultiPointOnConst(new F_int(P.X, _p.Val), new F_int(P.Y, _p.Val), 
                new F_int(R.X, _p.Val)._val, new F_int(_A.Val, _p.Val), out F_int right_1_1, out F_int right_1_2);

            OutToFile(verifyMulty_fR_P, new ParametersEl(right_1_1, right_1_2, "fRP"));
        }

        static void CheckSignature4()
        {
            DeleteFilesByNames(verifyMulty_s_R, verifySum_frP_sR);

            ParametersEl _p = new ParametersEl(0, "p"), _A = new ParametersEl(0, "A"), R = new ParametersEl(0, 0, "R"), s = new ParametersEl(0, "s");

            try
            {
                ReadInFile(publicParametersPath, __arglist(_p, _A));
                ReadInFile(signaturePath, __arglist(R, s));
            }
            catch (Exception e)
            {
                WriteLine(e.Message);
                return;
            }

            GeneratorEl.MultiPointOnConst(new F_int(R.X, _p.Val), new F_int(R.Y, _p.Val), 
                s.Val, new F_int(_A.Val, _p.Val), out F_int right_2_1, out F_int right_2_2);

            OutToFile(verifyMulty_s_R, new ParametersEl(right_2_1, right_2_2, "sR"));
        }

        static void CheckSignature5()
        {
            DeleteFilesByNames(verifySum_frP_sR);

            ParametersEl _A = new ParametersEl(0, "A"), _p = new ParametersEl(0, "p"), fRP = new ParametersEl(0, 0, "fRP"), sR = new ParametersEl(0, 0, "sR");

            try
            {
                ReadInFile(publicParametersPath, __arglist(_p, _A));
                ReadInFile(verifyMulty_fR_P, __arglist(fRP));
                ReadInFile(verifyMulty_s_R, __arglist(sR));
            }
            catch (Exception e)
            {
                WriteLine(e.Message);
                return;
            }

            GeneratorEl.MultiPointOnPoint(
                new F_int(fRP.X, _p.Val), new F_int(fRP.Y, _p.Val), 
                new F_int(sR.X, _p.Val), new F_int(sR.Y, _p.Val), 
                out F_int right1, out F_int right2, 
                new F_int(_A.Val, _p.Val));

            OutToFile(verifySum_frP_sR, new ParametersEl(right1, right2, "frP_sum"));
        }

        static void CheckSignature6()
        {
            ParametersEl hQ = new ParametersEl(0, 0, "hQ"), frP_sum = new ParametersEl(0, 0, "frP_sum");

            try
            {
                ReadInFile(verifyMulty_H_Q, __arglist(hQ));
                ReadInFile(verifySum_frP_sR, __arglist(frP_sum));
            }
            catch (Exception e)
            {
                WriteLine(e.Message);
                return;
            }

            if (hQ.X != frP_sum.X || hQ.Y != frP_sum.Y)
                WriteLine("Подпись не верна!");
            else
                WriteLine("Подпись верна!");
        }

        static (int, int) ReadPar()
        {
            WriteLine("Введите 2 числа через пробел: l и m:");

            string[] s = ReadLine().Split(' ');

            Exception e = new Exception("Ошибка ввода параметров элиптической кривой");

            if (s.Length != 2)
                throw e;

            int l = -1, m = -1;
            try
            {
                l = Convert.ToInt32(s[0]);
                m = Convert.ToInt32(s[1]);

                if (l < 3 || m < 1)
                    throw e;

                return (l, m);
            }
            catch
            {
                throw e;
            }
        }

        static BigInteger GetHash(string s, BigInteger mod) => (((new BigInteger(MD5.Create().ComputeHash(Encoding.Default.GetBytes(s)))) % mod) + mod) % mod;

        static bool DeleteFilesByNames(params string[] fileNames)
        {
            foreach (string path in fileNames)
                File.Delete(path);

            return true;
        }
    }
}
