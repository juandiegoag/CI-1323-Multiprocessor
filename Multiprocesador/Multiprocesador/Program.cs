using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace Multiprocesador
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Simulador S = new Simulador();
            S.iniciar();
        }
    }
    class Simulador
    {
        int reloj; //dame la hora 
        int quantum;//of solace 
        Procesador cpu1;
        Procesador cpu2;
        Procesador cpu3;

        public Simulador()
        {
            cpu1 = new Procesador(1);
            cpu2 = new Procesador(2);
            cpu3 = new Procesador(3);

        }

        public void iniciar()
        {
            Console.Write("Digite el numero de hilos");
            int hilos = Int32.Parse(Console.ReadLine());

            Console.Write("Digite el quantum plágurnar");
            quantum = Int32.Parse(Console.ReadLine());

            while (hilos-- > 0)
            {
                string path = dialog();
                switch ((hilos % 3) + 1)
                {
                    case 1:
                        cpu1.asignar(path);
                        break;

                    case 2:
                        cpu2.asignar(path);
                        break;

                    case 3:
                        cpu3.asignar(path);
                        break;

                    default:
                        break;
                }

            }
        }

        public string dialog()
        {
            OpenFileDialog choofdlog = new OpenFileDialog();
            choofdlog.Filter = "Text files (*.txt*)|*.txt*";
            choofdlog.FilterIndex = 1;
            choofdlog.Multiselect = false;
            string sFileName = "";
            if (choofdlog.ShowDialog() == DialogResult.OK)
            {
                sFileName = choofdlog.FileName;
            }
            return sFileName;
        }


        public void ejecutar()
        {

        }
    }


    class Procesador
    {
        int cicloActual;
        int cP;
        Cache cache;
        Memoria memoria;
        Registros registros;
        Stack<string> hilosDeInstruccion;
        int id;

        public Procesador(int i)
        {
            cache = new Cache();
            memoria = new Memoria();
            registros = new Registros();
            hilosDeInstruccion = new Stack<string>();
            id = i;
        }

        public void asignar(string path)
        {
            Console.Write("Imprimiendo desde CPU " + id);
            memoria.almacenarMemoria(abrirArchivo(path));
            memoria.imprimir();
        }

        public void decodificar(int[] instrucciones)
        {

            switch (instrucciones[0])
            {
                case 8:
                    //even yo wife cslls me daddy
                    break;

                case 420:

                    break;

                case 63:
                    //terminar 
                    break;


                default:
                    break;

            }

        }

        private string abrirArchivo(string path)
        {
            return convertirArrayString(File.ReadAllLines(path, Encoding.UTF8));
        }

        public static string convertirArrayString(string[] array)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string value in array)
            {
                builder.Append(value);
                builder.Append('.');
            }
            return builder.ToString();
        }


    }

    class Cache
    {

        int[] memoria = new int[64]; //4 chars = 1 palabra
        int[] bloke = new int[4]; //numero de bloque que se tiene en memoria cache


        public char[] traerPalabra(int bloque, int palabra)
        {
            char[] satanas = new char[4];
            return satanas;
        }

        public Cache()
        {


            for (int i = 0; i < 64; i++)
            {
                memoria[i] = '0';
            }

        }





    }
    class Memoria
    {
        int[] memoria;
        int ptrUltimo;

        public Memoria()
        {
            memoria = new int[256];
            ptrUltimo = 0;
        }

        public char[] traerPalabra(int bloque, int palabra)
        {
            char[] satanas = new char[4];
            return satanas;
        }

        public void almacenarMemoria(string hilo)
        {
            //Console.Write(hilo);
            char[] delimitadores = { ' ', '.' };
            string[] unnombreahimientrastanto = hilo.Split(delimitadores);
            for (int i = 0; i < unnombreahimientrastanto.Length - 1; i++)
            {
                memoria[i + ptrUltimo] = Convert.ToInt32(unnombreahimientrastanto[i]);
            }
            ptrUltimo += unnombreahimientrastanto.Length;
        }

        public void imprimir()
        {
            foreach (int x in memoria)
            {
                Console.Write(x);
                Console.Write(" ");
            }
            Console.Write("\n");
        }
    }

    class Registros
    {
        int[] r = new int[32];

    }



}