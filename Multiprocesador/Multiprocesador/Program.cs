using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Threading;

namespace Multiprocesador
{

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Simulador S = new Simulador();
            S.iniciar();
            S.ejecutar();
        }
    }

    public static class variablesGlobales
    {
        static int _reloj;
        public static int reloj
        {
            set { _reloj = value; }
            get { return _reloj; }
        }

        static int _quantum;
        public static int quantum
        {
            set { _quantum = value; }
            get { return _quantum; }
        }

        public static Barrier barrera = new Barrier(3, (b) =>
        {
            _reloj++;
            _quantum--;
            Console.WriteLine("Barrera");
        });

    }

    class Simulador
    {
        Procesador cpu1;
        Procesador cpu2;
        Procesador cpu3;

        public Simulador()
        {
        }

        public void iniciar()
        {
            Console.Write("Digite el numero de hilos -> ");
            int hilos = Int32.Parse(Console.ReadLine());

            Console.Write("Digite el quantum plágurnar -> ");
            variablesGlobales.quantum = Int32.Parse(Console.ReadLine());

            cpu1 = new Procesador(1);
            cpu2 = new Procesador(2);
            cpu3 = new Procesador(3);

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
            Thread hilo1 = new Thread(new ThreadStart(cpu1.ejecutar));
            Thread hilo2 = new Thread(new ThreadStart(cpu2.ejecutar));
            Thread hilo3 = new Thread(new ThreadStart(cpu3.ejecutar));
            hilo1.Start();
            hilo2.Start();
            hilo3.Start();
            Console.ReadKey();

        }
    }


    class Procesador
    {
        int reloj;
        int cicloActual;
        int cP;                  //PC
        Cache cache;
        Memoria memoria;
        Registros registros;
        Stack<string> hilosDeInstruccion;
        int id;

        public Procesador(int i)
        {
            reloj = variablesGlobales.quantum;
            memoria = new Memoria();
            cache = new Cache(memoria);
            registros = new Registros();
            hilosDeInstruccion = new Stack<string>();
            id = i;
            cP = 0;
        }

        public void asignar(string path)
        {
            memoria.almacenarMemoria(abrirArchivo(path));

        }

        public void ejecutar()
        {

            while (reloj-- > 0)
            {
                decodificar(cache.traerPalabra(cP / 4, cP % 4));
                variablesGlobales.barrera.SignalAndWait();
            }
            Console.ReadKey();
            Console.Write("TODO EN ORDEN");
        }


        public void decodificar(int[] instrucciones)
        {
            
            for (int i = 0; i < 4; i++)
            {
                Console.Write(instrucciones[i] + " ");
            }
            Console.Write("\n");
            Console.ReadKey();

            int i0 = instrucciones[0];
            int i1 = instrucciones[1];
            int i2 = instrucciones[2];
            int i3 = instrucciones[3];

            switch (i0)
            {

                case -1:
                    cP--;
                break;

                case 8:
                    //registros.insertarValorRegistro((registros.valorRegistro(i1) + i3), i2);
                    break;

                case 32:
                    //registros.insertarValorRegistro((registros.valorRegistro(i1) + registros.valorRegistro(i2)), i3);
                    break;

                case 34:
                break;

                case 12:
                break;

                case 14:
                break;

                case 420:
                    Console.WriteLine("Procesador HITS");
                    Console.WriteLine("FOUR TWENTY. BLAZE. IT. FAGGOT. ");
                break;

                case 63:
                    Console.ReadKey();
                    Console.WriteLine("FIN");  //terminar 
                    break;


                default:
                    break;

            }
            cP++;       //Incrementar PC

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
        Memoria disco;
        int[] memoria = new int[64]; //4 ints = 1 palabra
        int[] bloke = new int[4]; //numero de bloque que se tiene en memoria cache


        public int[] traerPalabra(int bloque, int palabra)
        {
            int[] palabraRetornada = new int[4];
            for (int i = 0; i < 4; i++)
            {
                if (bloke[i] == bloque)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        palabraRetornada[j] = memoria[(i*16) + (palabra * 4) + j];
                    }
                    return palabraRetornada;
                }
            }
            faloCache(bloque);
            for (int i = 0; i < 4; i++)
            {
                palabraRetornada[i] = -1;
            }
            return palabraRetornada;
        }

        public void imprimirMem()
        {
            Console.WriteLine("Memoria: ");
            for (int i = 0; i < 64; i++)
            {
                Console.Write(memoria[i] + " ");
            }
            Console.Write("fin mem\n");
        }

        public void faloCache(int bloque)
        {
            int bloqueActual = bloque % 4;
            bloke[bloqueActual] = bloque;
            int[] enchilada = disco.traerBloque(bloque);
            for (int i = 0; i < 16; i++)
            {
                memoria[(bloqueActual * 16) + i] = enchilada[i];
            }

        }


        public Cache(Memoria mem)
        {
            disco = mem;
            for (int i = 0; i < 64; i++)
            {
                memoria[i] = 0;
            }
            for (int i = 0; i < 4; i++)
            {
                bloke[i] = -1;
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

        public int[] traerBloque(int bloque)
        {
            int[] bloqueRetornado = new int[16];
            for (int i = 0; i < 16; i++)
            {
                bloqueRetornado[i] = memoria[(bloque * 16) + i];
            }
            return bloqueRetornado;
        }

        public void almacenarMemoria(string hilo)
        {
            char[] delimitadores = { ' ', '.' };
            string[] unnombreahimientrastanto = hilo.Split(delimitadores);
            for (int i = 0; i < unnombreahimientrastanto.Length - 1; i++)
            {
                memoria[i + ptrUltimo] = Convert.ToInt32(unnombreahimientrastanto[i]);
            }
            ptrUltimo += unnombreahimientrastanto.Length - 1;
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
        int[] r;
        public Registros()
        {
            r = new int[32];
            r[0] = 0; //registro 0 siempre esta en valor 0
        }

        public int valorRegistro(int x) {
            return r[x];
        }

        public void insertarValorRegistro(int valor, int x) {
            r[x] = valor;
        }

    }



}