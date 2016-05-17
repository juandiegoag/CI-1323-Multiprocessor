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
            Console.ReadKey();
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

            Console.Write("Digite el quantum -> ");//quantum para todos
            variablesGlobales.quantum = Int32.Parse(Console.ReadLine());
            variablesGlobales.q = variablesGlobales.quantum;

            cpu1 = new Procesador(1);//crea los procesadores con el quantum, y les asigna su ID
            cpu2 = new Procesador(2);
            cpu3 = new Procesador(3);

            while (hilos-- > 0)//se dividen los hilos entre los tres cpu repartidos al 1,2,3,3,2,1,2,... y asi 
            {//sucesivamente por cuantos hilos hayan 
                string path = dialog();//ventana de eleccion de archivos
                List<int> lista = leerHilo(abrirArchivo(path)); //se crea una lista de enteros a partir del archivo
                Console.WriteLine("Hilo " + path + " asignado a cpu " + ((hilos % 3) + 1));
                switch ((hilos % 3) + 1)
                {
                    case 1:
                        {
                            cpu1.asignar(lista);
                            cpu1.numHilos++;
                        }
                        break;
                    case 2:
                        {
                            cpu2.asignar(lista);
                            cpu2.numHilos++;
                        }
                        break;
                    case 3:
                        {
                            cpu3.asignar(lista);
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


        public List<int> leerHilo(string hilo)
        {
            char[] delimitadores = { ' ', '.' };
            string[] arregloInstrucciones = hilo.Split(delimitadores);
            List<int> listaInstrucciones = new List<int>();
            for (int i = 0; i < arregloInstrucciones.Length - 1; i++)
            {//obtiene los metodos del string que no son cualquiera de los anteriores, los convierte a int y los guarda
                listaInstrucciones.Add(Convert.ToInt32(arregloInstrucciones[i]));//en memoria segun el offset que se le diga
            }
            return listaInstrucciones;
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
            Console.WriteLine("Simulacion finalizada. ");
            hilo1.Abort();
            hilo2.Abort();
            hilo3.Abort();
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
        int suspendido;
        Logger log;
        int ciclosPorHilo;
        bool finit;
        

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
            suspendido = 0;
            log = new Logger(i);
            ciclosPorHilo = 0;

        }

        public void asignar(List<int> listaInstrucciones)
        {
            int cPInicialDelHilo = memoria.almacenarMemoria(listaInstrucciones) / 4;//guarda en memoria el hilillo que se le ponga
            crearContexto(cPInicialDelHilo);
        }


        public void crearContexto(int cPdelHilo)
        {
            int[] contextoNuevo = new int[33];
            contextoNuevo[32] = cPdelHilo;
            colaHilos.Enqueue(contextoNuevo);
            log.imprimir("ContextoCreado");
            finit = false;
        }

        public void reestablecerQuantum()
        {
            reloj = variablesGlobales.q;
            log.imprimir("Reestableciendo quantum a "+ variablesGlobales.q + " \n");
        }


        public void ejecutar()
        {
            if (numHilos > 0)
            {
                insertarContexto(); //inserta el contexto inicial
            }
            while (numHilos > 0)
            {
                log.imprimir("Hilos restantes: "+numHilos+"\n");
                log.imprimir("Quantum de proceso "+id+": "+reloj+"\n");
                

                while (reloj-- > 0 && numHilos > 0)
                {
                    ciclosPorHilo++;
                    if (suspendido == 0 && finit == false) {
                        decodificar(cache.traerPalabra(cP / 4, cP % 4));
                    }
                    else if (suspendido > 0 && finit == false)
                    {
                        log.imprimir("Fallo de cache, procesador suspendido por "+suspendido+" ciclos restantes \n");
                        suspendido--;
                    }
                    variablesGlobales.barrera.SignalAndWait();
                }
                if (finit)
                {
                    if (numHilos >= 1)
                    {
                        insertarContexto();
                        reestablecerQuantum();
                    }
                }
                else
                {
                    if (numHilos >= 1)
                    {
                        cambioDeContexto();
                        reestablecerQuantum();
                    }
                }

            }
            log.imprimir("El procesador " + id + " ha terminado su trabajo.\n ");
            variablesGlobales.barrera.RemoveParticipant();
            log.exportarResultados();

        }


        public void decodificar(int[] instrucciones)
        {
            //metodo que se encarga de decodificar los sets de instrucciones de 4 argumentos, y mapearlos en su correspondiente
            //funcion en MIPS DADDI, DADD, DMUL, ...
            log.imprimir("Procesador " + id + " ejecutando instruccion:\n");
            for (int i = 0; i < 4; i++)
            {
                log.imprimir( instrucciones[i] + " ");
            }
            log.imprimir("\n");
             

            int codigoOp = instrucciones[0]; //codigo de instruccion
            /*registros o inmediatos */
            int i1 = instrucciones[1]; 
            int i2 = instrucciones[2];
            int i3 = instrucciones[3];

            switch (codigoOp)
            {

                case -1:
                    suspendido = 16;
                    cP--;
                break;
                    /***** OPERACIONES ARITMETICAS BASICAS *****/
                case 8:
                    registros.insertarValorRegistro((registros.valorRegistro(i1) + i3), i2); //suma de registro con inmediato
                break;
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
                    if (registros.valorRegistro(i1) == 0){
                        cP += i3;
                    }
                break;

                case 5:
                    if (registros.valorRegistro(i1) != 0)
                    {
                        cP += i3;
                    }
                break;

                case 3:
                    registros.insertarValorRegistro(cP, 31);
                    cP += i3/4;
                break;

                case 2:
                    cP = registros.valorRegistro(i1);
                break;
                case 63:
                    numHilos--;
                    log.imprimir("\nHilo finalizado en el procesador: " + id); 
                    log.imprimir("\nTotal de ciclos para la ejecucion del hilo: " + ciclosPorHilo+ "\n");
                    ciclosPorHilo = 0;
                    finit = true;
                    log.imprimir(registros.imprimir());
                    log.imprimir("\n FIN \n");  //terminar 
                break;

                default:
                break;

            }
            cP++;       //Incrementar PC

        }

        public void cambioDeContexto()
        {
            log.imprimir("Cambio de contexto. ");
            int[] estadoAnterior = new int[33];
            Array.Copy(registros.reg(), 0, estadoAnterior, 0, 32);//Guarda los registros
            estadoAnterior[32] = cP;
            log.imprimir("Viejo CP es: " + cP + "\n");
            colaHilos.Enqueue(estadoAnterior);
            log.imprimir("Cargando registros de contexto nuevo");
            insertarContexto();
            log.imprimir("Nuevo CP es: "+ cP +"\n");
        }

        public void insertarContexto()
        {
            int[] nuevoEstado = colaHilos.Dequeue();
            registros.setReg(nuevoEstado);
            cP = nuevoEstado[32];
            finit = false;
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
            falloCache(bloque);
            for (int i = 0; i < 4; i++)
            {
                palabraRetornada[i] = -1;
            }
            return palabraRetornada;
        }


        public void falloCache(int bloque) //metodo que maneja el fallo de cache
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
            memoria = new int[384];
            indiceHilos = new List<int>();
            ptrUltimo = 128; //comienza en ocho
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

        public int almacenarMemoria(List<int> instrucciones)//se le envia una lista de enteros con el contenido de un hilillo, retorna posicion inicial de hilillo
        {//que ya fue concatenado por espacios vacios y '.'
            for (int i = 0; i < instrucciones.Count; i++) //no copia el ultimo, que es el cP
            {//obtiene los metodos del string que no son cualquiera de los anteriores, los convierte a int y los guarda
                memoria[i + ptrUltimo] = instrucciones[i];//en memoria segun el offset que se le diga
            }
            int ptrUltimoRetornado = ptrUltimo; //conserva la posicion inicial del hilillo
            ptrUltimo += instrucciones.Count;//actualiza el offset
            return ptrUltimoRetornado;  // retorna posicion inicial de hilillo
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
            r = new int[33];
            r[0] = 0; //registro 0 siempre esta en valor 0
            //registro 32 es cP
        }
        /**sets y gets**/
        public int[] reg()
        {
            return r;
        }

        public void setReg(int[] regs)
        {
            Array.Copy(regs, r, 33);
        }

        public int valorRegistro(int x) {
            return r[x];
        }

        public void insertarValorRegistro(int valor, int x) {
            r[x] = valor;
        }
        //Imprime los registros 
        public string imprimir() {
            string regs = "";
            for (int i = 0; i < 32; i++) {
                regs+=("R" + i + ": "+r[i]+"\t");
                if (i%8 == 0) {
                    regs+=("\n");
                }
            }
            return regs;
        }

    }

    class Logger {
        string global;
        string fileName;
        string path;

        public Logger(int i) {
            fileName = "cpu" + i;
            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName); ;
            global = "";
        }

        public void imprimir(string texto) {
            global += texto;
        }

        public void exportarResultados() {
            File.WriteAllText(path, global);
        }


    }



}