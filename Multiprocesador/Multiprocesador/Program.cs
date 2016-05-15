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
            Simulador S = new Simulador(); //crea la instancia del simulador
            S.iniciar();
            S.ejecutar();
        }
    }

    public static class variablesGlobales
    {
        /**Variables globales quantum y reloj para todos los procesadores, con su set y get**/
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

        static int _q;
        public static int q
        {
            set { _q = value; }
            get { return _q; }
        }


        public static Barrier barrera = new Barrier(3, (b) =>//instancia de una barrera global que espera la llegada
        {                                                    //de 3 hilos, y luego ejecuta el codigo en la funcion (b)
            _reloj++;
            _quantum--;
            Console.WriteLine("Barrera");
        });

    }

    class Simulador
    {
        //instancias de los tres procesadores en cuestion
        Procesador cpu1;
        Procesador cpu2;
        Procesador cpu3;

        public Simulador()
        {
        }

        public void iniciar()//el metodo pide todos los datos necesarios para correr la simulacion
        {
            Console.Write("Digite el numero de hilos -> ");//numero de hilillos
            int hilos = Int32.Parse(Console.ReadLine());

            Console.Write("Digite el quantum plágurnar -> ");//quantum para todos
            variablesGlobales.quantum = Int32.Parse(Console.ReadLine());
            variablesGlobales.q = variablesGlobales.quantum;

            cpu1 = new Procesador(1);//crea los procesadores con el quantum, y les asigna su ID
            cpu2 = new Procesador(2);
            cpu3 = new Procesador(3);

            while (hilos-- > 0)//se dividen los hilos entre los tres cpu repartidos al 1,2,3,3,2,1,2,... y asi 
            {//sucesivamente por cuantos hilos hayan 
                string path = dialog();//ventana de eleccion de archivos
                switch ((hilos % 3) + 1)
                {
                    case 1:
                        {
                            cpu1.asignar(path);
                            cpu1.numHilos++;
                        }
                        break;
                    case 2:
                        {
                            cpu2.asignar(path);
                            cpu2.numHilos++;
                        }
                        break;
                    case 3:
                        {
                            cpu3.asignar(path);
                            cpu3.numHilos++;
                        }
                        break;

                    default:
                        break;
                }

            }
            Console.Write(cpu2.numHilos + " hilos cargados al Procesador " + cpu2.id + "\n");
            Console.Write(cpu1.numHilos + " hilos cargados al Procesador " + cpu1.id + "\n");
            Console.Write(cpu3.numHilos + " hilos cargados al Procesador " + cpu3.id + "\n");
        }

        public string dialog()//ventana para escoger archivos
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
            Thread hilo1 = new Thread(new ThreadStart(cpu1.ejecutar));//crea los 3 threads con la funcion de ejectuar
            Thread hilo2 = new Thread(new ThreadStart(cpu2.ejecutar));//de cada CPU
            Thread hilo3 = new Thread(new ThreadStart(cpu3.ejecutar));
            hilo1.Start();//le da inicio a cada uno de los threads
            hilo2.Start();
            hilo3.Start();
            Console.ReadKey();

        }
    }


    class Procesador
    {
        public int reloj;
        int cicloActual;
        int cP;                         //PC
        public int numHilos;            //Número de hilos que se le asigna a cada procesador
        public bool primer;             //booleano que indica si es el primer cambio de contexto que se da en el procesador
        Cache cache;
        public Memoria memoria;
        public Registros registros;
        public Queue<int[]> colaHilos;
        public int id;

        public Procesador(int i) //constructor 
        {
            reloj = variablesGlobales.quantum; //quantum global, digitado por el usuario
            memoria = new Memoria(); //memoria del procesador
            cache = new Cache(memoria); //cache del procesador, y se le envia la memoria del CPU para poder retribuir datos
            registros = new Registros(); // 32 registros del procesador
            id = i; //ID de procesador
            cP = 0; //contador de programa
            numHilos = 0;
            colaHilos = new Queue<int[]>();
            primer = true;
        }

        public void asignar(string path)
        {
            memoria.almacenarMemoria(abrirArchivo(path));//guarda en memoria el hilillo que se le ponga

        }

        public void reestablecerQuantum()
        {
            reloj = variablesGlobales.q;
            Console.Write("Reestableciendo quantum a "+ variablesGlobales.q + " \n");
        }

        public void ejecutar()
        {

            while (numHilos > 0)
            {
                Console.Write("Hilos restantes: "+numHilos+"\n");
                Console.Write("Quantum de proceso "+id+": "+reloj+"\n");
                Console.ReadKey();

                while (reloj-- > 0)
                {
                    decodificar(cache.traerPalabra(cP / 4, cP % 4));
                    variablesGlobales.barrera.SignalAndWait();
                }
                if (numHilos >= 1)
                {
                    cambioDeContexto();
                    reestablecerQuantum();
                   
                }
                else
                {
                    Console.Write("El procesador " + id + " ha terminado su trabajo.\n ");
                }
            }


            Console.Write("TODO EN ORDEN");
            Console.ReadKey();
        }


        public void decodificar(int[] instrucciones)
        {
            //metodo que se encarga de decodificar los sets de instrucciones de 4 argumentos, y mapearlos en su correspondiente
            //funcion en MIPS DADDI, DADD, DMUL, ...
            Console.Write("Procesador " + id + " ejecutando instruccion:\n");
            for (int i = 0; i < 4; i++)
            {
                Console.Write( instrucciones[i] + " ");
            }
            Console.Write("\n");
            Console.ReadKey();

            int codigoOp = instrucciones[0]; //codigo de instruccion
            /*registros o inmediatos */
            int i1 = instrucciones[1]; 
            int i2 = instrucciones[2];
            int i3 = instrucciones[3];

            switch (codigoOp)
            {

                case -1:
                    cP--;
                break;

                case 8:
                    registros.insertarValorRegistro((registros.valorRegistro(i1) + i3), i2); //suma de registro con inmediato
                break;
                    /***** OPERACIONES ARITMETICAS BASICAS *****/
                case 32:
                    registros.insertarValorRegistro((registros.valorRegistro(i1) + registros.valorRegistro(i2)), i3);
                break;

                case 34:
                    registros.insertarValorRegistro((registros.valorRegistro(i1) - registros.valorRegistro(i2)), i3);
                break;

                case 12:
                    registros.insertarValorRegistro((registros.valorRegistro(i1) * registros.valorRegistro(i2)), i3);
                break;

                case 14:
                    registros.insertarValorRegistro((registros.valorRegistro(i1) / registros.valorRegistro(i2)), i3);
                break;
                    /***BRANCHING Y DEMAS***/
                case 4:
                    if (registros.valorRegistro(i2) == 0){
                        cP += i3;
                    }
                break;

                case 5:
                    if (registros.valorRegistro(i2) != 0)
                    {
                        cP += i3;
                    }
                break;

                case 3:
                    registros.insertarValorRegistro(cP, 31);
                    cP += i3;
                break;

                case 2:
                    cP = registros.valorRegistro(i1);
                break;
                case 63:
                    {
                        numHilos--;
                        registros.imprimir();
                        Console.ReadKey();
                        Console.WriteLine("FIN");  //terminar 
                    }
                    //quantum = 0?? Cambiar de contexto??
                break;

                case 420:
                    Console.WriteLine("Procesador HITS");
                    Console.WriteLine("FOUR TWENTY. BLAZE. IT. FAGGOT. ");
                break;

                default:
                break;

            }
            cP++;       //Incrementar PC

        }

        public void cambioDeContexto()
        {
            Console.ReadKey();
            Console.Write("Cambio de contexto. ");
            int[] estadoAnterior = new int[33];
            estadoAnterior[32] = cP;
            Console.Write("Viejo CP es: " + cP + "\n");
            Array.Copy(registros.reg(), 0, estadoAnterior, 0, 31);//Guarda los registros
            colaHilos.Enqueue(estadoAnterior);

            Console.Write("Cargando registros de contexto nuevo");
            int[] nuevoEstado = colaHilos.Dequeue();
            Array.Copy(nuevoEstado, 0, registros.reg(), 0, 31);

            if (primer)//Hay que quitar este if
            {
                //cP = memoria.indiceHilos[];
            }
            else
            {
                cP = nuevoEstado[32];
            }

            Console.Write("Nuevo CP es: "+ cP +"\n");
            
        }

        private string abrirArchivo(string path) //convierte el archivo que lee del path que se le pasa por parametro
        {//en un unico string pegado 
            return convertirArrayString(File.ReadAllLines(path, Encoding.UTF8));
        }

        public static string convertirArrayString(string[] array)//recibe un string[] y lo transforma en string
        {//usando StringBuilder
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
        Memoria disco; //disco en donde el cache tiene que buscar paginas 
        int[] memoria = new int[64]; //4 ints = 1 palabra, total de la memoria cache
        int[] bloke = new int[4]; //numero de bloque que se tiene en memoria cache

        public Cache(Memoria mem)//inicializa la memoria cache en 0 y el mapeo de bloques en cache en -1
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

        public int[] traerPalabra(int bloque, int palabra)
        {
            int[] palabraRetornada = new int[4];
            for (int i = 0; i < 4; i++)
            {
                if (bloke[i] == bloque)
                {
                    for (int j = 0; j < 4; j++)
                    {//busca en disco la palabra que se necesita y la devuelve
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


        public void faloCache(int bloque) //metodo que maneja el fallo de cache
        {
            int bloqueActual = bloque % 4;//offset para manejar la posicion dle nuevo bloque
            bloke[bloqueActual] = bloque;//cambia en el array que mapea los bloques en cache por el nuevo bloque
            int[] enchilada = disco.traerBloque(bloque);//trae el bloque
            for (int i = 0; i < 16; i++)
            {
                memoria[(bloqueActual * 16) + i] = enchilada[i];//lo guarda en cache
            }

        }
        
        /**imprimir la memoria cache**/
        public void imprimirMem() 
        {
            Console.WriteLine("Memoria: ");
            for (int i = 0; i < 64; i++)
            {
                Console.Write(memoria[i] + " ");
            }
            Console.Write("fin mem\n");
        }


    }

    class Memoria
    {
        int[] memoria; //array de memoria "disco"
        int ptrUltimo; //puntero a la ultima posicion con datos de memoria, se utiliza como offset 
        public List<int> indiceHilos;

        public Memoria()
        {
            memoria = new int[256];
            indiceHilos = new List<int>();
            ptrUltimo = 0; //comienza en cero
        }

        public int[] traerBloque(int bloque)//trae el numero de bloque que se le pide, considerando que
        {                                   //los bloques son de tamano 16, y la memoria es un arreglo lineal
            int[] bloqueRetornado = new int[16];
            for (int i = 0; i < 16; i++)
            {
                bloqueRetornado[i] = memoria[(bloque * 16) + i];
            }
            return bloqueRetornado;
        }

        public void almacenarMemoria(string hilo)//se le envia un string entero con el contenido de un hilillo
        {//que ya fue concatenado por espacios vacios y '.'
            char[] delimitadores = { ' ', '.' };
            string[] unnombreahimientrastanto = hilo.Split(delimitadores);
            for (int i = 0; i < unnombreahimientrastanto.Length - 1; i++)
            {//obtiene los metodos del string que no son cualquiera de los anteriores, los convierte a int y los guarda
                memoria[i + ptrUltimo] = Convert.ToInt32(unnombreahimientrastanto[i]);//en memoria segun el offset que se le diga
            }
            indiceHilos.Add(ptrUltimo);
            ptrUltimo += unnombreahimientrastanto.Length - 1;//actualiza el offset
        }

        public void imprimir() //imprime la memoria
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
        /**sets y gets**/
        public int[] reg()
        {
            return r;
        }

        public int valorRegistro(int x) {
            return r[x];
        }

        public void insertarValorRegistro(int valor, int x) {
            r[x] = valor;
        }
        //Imprime los registros 
        public void imprimir() {
            for (int i = 0; i < 32; i++) {
                Console.Write("R" + i + ": "+r[i]+"\t");
                if (i%8 == 0) {
                    Console.Write("\n");
                }
            }
        }

    }



}