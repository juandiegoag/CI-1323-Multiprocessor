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
            multiprocesador.cpu1 = new Procesador(1);
            multiprocesador.cpu2 = new Procesador(2);
            multiprocesador.cpu3 = new Procesador(3);
 
            S.iniciar();
            S.ejecutar();
            Console.ReadKey();
        }
    }

    public static class multiprocesador //variables globales del multiprocesador
    {
        /**Variables globales quantum y reloj para todos los procesadores, con su set y get**/

        static Procesador _cpu1;
        public static Procesador cpu1
        {
            set { _cpu1 = value; }
            get { return _cpu1; }
        }

        static Procesador _cpu2;
        public static Procesador cpu2
        {
            set { _cpu2 = value; }
            get { return _cpu2; }
        }

        static Procesador _cpu3;
        public static Procesador cpu3
        {
            set { _cpu3 = value; }
            get { return _cpu3; }
        }

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

        static object[] _locksCaches;
        public static object[] locksCaches
        {
            get { return _locksCaches; }
            set { _locksCaches = value; }
        }

        static object[] _locksDirs;
        public static object[] locksDirs
        {
            get { return _locksDirs; }
            set { _locksDirs = value; }
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
        

        public Simulador()
        {
        }

        public void iniciar()//el metodo pide todos los datos necesarios para correr la simulacion
        {
            Console.Write("Digite el numero de hilos -> ");//numero de hilillos
            int hilos = Int32.Parse(Console.ReadLine());

            Console.Write("Digite el quantum -> ");//quantum para todos
            multiprocesador.quantum = Int32.Parse(Console.ReadLine());
            multiprocesador.q = multiprocesador.quantum;


            while (hilos-- > 0)//se dividen los hilos entre los tres cpu repartidos al 1,2,3,3,2,1,2,... y asi 
            {//sucesivamente por cuantos hilos hayan 
                string path = dialog();//ventana de eleccion de archivos
                List<int> lista = leerHilo(abrirArchivo(path)); //se crea una lista de enteros a partir del archivo
                Console.WriteLine("Hilo " + path + " asignado a cpu " + ((hilos % 3) + 1));
                switch ((hilos % 3) + 1)
                {
                    case 1:
                        {
                            multiprocesador.cpu1.asignar(lista);
                            multiprocesador.cpu1.numHilos++;
                        }
                        break;
                    case 2:
                        {
                            multiprocesador.cpu2.asignar(lista);
                            multiprocesador.cpu2.numHilos++;
                        }
                        break;
                    case 3:
                        {
                            multiprocesador.cpu3.asignar(lista);
                            multiprocesador.cpu3.numHilos++;
                        }
                        break;

                    default:
                        break;
                }

            }
            Console.Write(multiprocesador.cpu2.numHilos + " hilos cargados al Procesador " + multiprocesador.cpu2.id + "\n");
            Console.Write(multiprocesador.cpu1.numHilos + " hilos cargados al Procesador " + multiprocesador.cpu1.id + "\n");
            Console.Write(multiprocesador.cpu3.numHilos + " hilos cargados al Procesador " + multiprocesador.cpu3.id + "\n");
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
            Thread hilo1 = new Thread(new ThreadStart(multiprocesador.cpu1.ejecutar));//crea los 3 threads con la funcion de ejectuar
            Thread hilo2 = new Thread(new ThreadStart(multiprocesador.cpu2.ejecutar));//de cada CPU
            Thread hilo3 = new Thread(new ThreadStart(multiprocesador.cpu3.ejecutar));
            hilo1.Start();//le da inicio a cada uno de los threads
            hilo2.Start();
            hilo3.Start();
            Console.WriteLine("Simulacion finalizada. ");
 
        }
    }


    public class Procesador
    {
        public int reloj;
        int cicloActual;
        int cP;                         //PC
        public int numHilos;            //Número de hilos que se le asigna a cada procesador
        public bool primer;             //booleano que indica si es el primer cambio de contexto que se da en el procesador
        Cache cache;
        public Memoria memoria;
        public Directorio directorio;
        public CacheDatos cacheD;
        public Registros registros;
        public Queue<int[]> colaHilos;
        public int id;
        int suspendido;
        Logger log;
        int ciclosPorHilo;
        bool finit;


        public Procesador(int i) //constructor 
        {
            reloj = multiprocesador.quantum; //quantum global, digitado por el usuario
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
            cacheD = new CacheDatos();
            directorio = new Directorio();
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
            reloj = multiprocesador.q;
            log.imprimir("Reestableciendo quantum a "+ multiprocesador.q + " \n");
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
                    if (suspendido == 0 && finit == false)
                    {
                        decodificar(cache.traerPalabra(cP / 4, cP % 4));
                    }
                    else if (suspendido > 0 && finit == false)
                    {
                        log.imprimir("Fallo de cache, procesador suspendido por "+suspendido+" ciclos restantes \n");
                        suspendido--;
                    }
                    multiprocesador.barrera.SignalAndWait();
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
            multiprocesador.barrera.RemoveParticipant();
            log.exportarResultados();

        }


        public void decodificar(int[] instrucciones/*, ref Directorio dir1, ref Directorio dir2, ref Directorio*/ )
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

                case 35: //LW
                    Procesador cpuLoad = this;

                    int posicionMemoriaLoad = i3 + registros.valorRegistro(i1);

                    int? datoLoad = this.cacheD.traerDato(posicionMemoriaLoad % 4, posicionMemoriaLoad / 16, ref cpuLoad);
                    switch (this.id)
                    {
                        case 1:
                            multiprocesador.cpu1 = cpuLoad;
                            break;
                        case 2:
                            multiprocesador.cpu2 = cpuLoad;
                            break;
                        case 3:
                            multiprocesador.cpu3 = cpuLoad;
                            break;
                    }
                    if (datoLoad == null) { cP--; }
                    //hay que hacer el caso if (dato == null) para que se proceda a intentar de nuevo
                    //con el fallo de caché ya resuelto                                        

                    break;
                case 43: //SW

                    Procesador cpuStore = this;
                    int posicionMemoriaStore = i3 + registros.valorRegistro(i1);
                    int datoEscribir = registros.valorRegistro(i2);
                    bool datoStore = this.cacheD.escribirDato(posicionMemoriaStore % 4, posicionMemoriaStore / 16, datoEscribir, ref cpuStore);
                    switch (this.id)
                    {
                        case 1:
                            multiprocesador.cpu1 = cpuStore;
                            break;
                        case 2:
                            multiprocesador.cpu2 = cpuStore;
                            break;
                        case 3:
                            multiprocesador.cpu3 = cpuStore;
                            break;
                    }
                    if (datoStore == false) { cP--; }
                    //hay que hacer el caso if (dato == null) para que se proceda a intentar de nuevo
                    //con el fallo de caché ya resuelto                                        

                    break;
                /***BRANCHING Y DEMAS***/
                case 4:
                    if (registros.valorRegistro(i1) == 0)
                    {
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

    public class Cache
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

    public class CacheDatos
    {
        public int[] datos;
        public int[] etiqueta;
        public char[] estado;

        public CacheDatos()
        {
            datos    = new int [16];
            etiqueta = new int [4];
            estado   = new char[4];

            for (int i = 0; i < 4; i++)
            {
                etiqueta[i] = -1;
                estado[i]  = 'I';
            }
            for (int i = 0; i < 16; i++)
            {
                datos[i] = 0;
            }

        }

        public bool revisarInvalidos(List<int> inv, int bloque)
        {
            bool finalizado = true;
            foreach (int n in inv)
            {
                switch (n)
                {
                    case 1:
                        if (Monitor.TryEnter(multiprocesador.cpu1.cacheD))
                        {
                            try
                            {
                                multiprocesador.cpu1.cacheD.invalidarBloque(bloque);
                            }
                            finally
                            {
                                Monitor.Exit(multiprocesador.cpu1.cacheD);
                            }
                        }
                        else
                        {
                            finalizado = false;
                        }
                        break;
                    case 2:
                        if (Monitor.TryEnter(multiprocesador.cpu2.cacheD))
                        {
                            try
                            {
                                multiprocesador.cpu2.cacheD.invalidarBloque(bloque);
                            }
                            finally
                            {
                                Monitor.Exit(multiprocesador.cpu2.cacheD);
                            }
                        }
                        else
                        {
                            finalizado = false;
                        }
                        break;
                    case 3:
                        if (Monitor.TryEnter(multiprocesador.cpu3.cacheD))
                        {
                            try
                            {
                                multiprocesador.cpu3.cacheD.invalidarBloque(bloque);
                            }
                            finally
                            {
                                Monitor.Exit(multiprocesador.cpu3.cacheD);
                            }
                        }
                        else
                        {
                            finalizado = false;
                        }
                        break;
                }
            }
            return finalizado;
        }

        public bool escribirDato(int palabra, int bloque, int dato, ref Procesador cpu)
        {
            bool hit = true;
            bool miss = true;
            int posicion = bloque % 4;
            int? pinga = null;

            if (Monitor.TryEnter(cpu.cacheD))
            {
                try
                {
                    int bloqueSalvado = cpu.cacheD.etiqueta[posicion];
                    for (int i = 0; i < 4; i++)
                    {
                        if (etiqueta[i] == bloque && estado[i] != 'I')
                        {
                            miss = false;

                            if (estado[i] == 'C')
                            {
                                List<int> listaInvalidos = new List<int>();

                                int id = cpu.id - 1;
                                int bloqueLocal = bloque % 8;
                                //Primero se revisa a ver cuales cachés deberán ser invalidadas, pero no se les modifica
                                //aún pues puede que se bloqueen las caches y luego se modifiquen los directorios
                                //sin modificar la cache previamente, creando una incoherencia en los datos del bloque

                                if (bloque >= 0 && bloque < 8)
                                {
                                    if (Monitor.TryEnter(multiprocesador.cpu1.directorio))
                                    {
                                        try
                                        {
                                            listaInvalidos = multiprocesador.cpu1.directorio.revisarBloque(bloqueLocal, id);
                                            if(!revisarInvalidos(listaInvalidos, bloque))
                                            {
                                                //Si no se logró invalidar el bloque en todas las caches
                                                //entonces se detiene el intento de escritura y se detiene 
                                                //el método para que vuelva a intentar hasta que logre invalidar en todas las caches
                                                hit = false;
                                            }    
                                            //multiprocesador.cpu1.directorio.invalidarCopias(bloqueLocal, id);
                                        }
                                        finally
                                        {
                                            Monitor.Exit(multiprocesador.cpu1.directorio);
                                        }
                                    }
                                    else
                                    {
                                        hit = false;
                                    }

                                }
                                else if (bloque >= 8 && bloque < 16)
                                {
                                    if ( Monitor.TryEnter(multiprocesador.cpu2.directorio) )
                                    {            
                                        try      
                                        {        
                                            listaInvalidos = multiprocesador.cpu2.directorio.revisarBloque(bloqueLocal, id);
                                            if (!revisarInvalidos(listaInvalidos, bloque))
                                            {
                                                //Si no se logró invalidar el bloque en todas las caches
                                                //entonces se detiene el intento de escritura y se detiene 
                                                //el método para que vuelva a intentar hasta que logre invalidar en todas las caches
                                                hit = false;
                                            }
                                            //multiprocesador.cpu1.directorio.invalidarCopias(bloqueLocal, id);
                                        }
                                        finally  
                                        {        
                                            Monitor.Exit(multiprocesador.cpu2.directorio);
                                        }        
                                    }
                                    else
                                    {
                                        hit = false;
                                    }
                                }                
                                else if (bloque >= 16 && bloque < 24)
                                {                
                                    if (Monitor.TryEnter(multiprocesador.cpu3.directorio))
                                    {            
                                        try      
                                        {        
                                            listaInvalidos = multiprocesador.cpu3.directorio.revisarBloque(bloqueLocal, id);
                                            if (!revisarInvalidos(listaInvalidos, bloque))
                                            {
                                                //Si no se logró invalidar el bloque en todas las caches
                                                //entonces se detiene el intento de escritura y se detiene 
                                                //el método para que vuelva a intentar hasta que logre invalidar en todas las caches
                                                hit = false;
                                            }
                                            //multiprocesador.cpu1.directorio.invalidarCopias(bloqueLocal, id);
                                        }
                                        finally  
                                        {        
                                            Monitor.Exit(multiprocesador.cpu3);
                                        }        
                                    }
                                    else
                                    {
                                        hit = false;
                                    }
                                }                
                                                 
                                //método para invalidar en las caches
                                                 
                            }                    
                                                 
                            if(hit)              
                            {
                                datos[i * 4 + palabra] = dato;//en caso de que estado sea 'M' el valor se compia encima sin mayor problema
                                break;
                            }
                        }
                    }

                    if (miss)
                    {
                         
                        if (bloque >= 0 && bloque < 8)
                        {
                            if(procesarBloqueVictima(bloqueSalvado, posicion, ref cpu))
                            {
                                pinga = falloCacheDatos(bloque, palabra, ref multiprocesador.cpu1.directorio, ref cpu);
                            }
                        }
                        else if (bloque >= 8 && bloque < 16)
                        {
                            if(procesarBloqueVictima(bloqueSalvado, posicion, ref cpu))
                            {
                                pinga = falloCacheDatos(bloque, palabra, ref multiprocesador.cpu2.directorio, ref cpu);
                            }
                        }
                        else if (bloque >= 16 && bloque < 24)
                        {
                            if (procesarBloqueVictima(bloqueSalvado, posicion, ref cpu))
                            {
                                pinga = falloCacheDatos(bloque, palabra, ref multiprocesador.cpu3.directorio, ref cpu);
                            }
                        }

                        if (pinga == null)
                        {
                            hit = false;
                        }
                        else
                        {
                            escribirEnCache(dato, bloque, palabra);
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(cpu.cacheD);
                }
            }
            else
            {
                hit = false;
            }

            return hit;
        }

        public void escribirEnCache(int dato, int bloque, int palabra)
        {
            for(int i = 0; i < 4; i++)
            {
                if(bloque == etiqueta[i])
                {
                    datos[i * 4 + palabra] = dato;
                    break;
                }
            }
        }

        public void invalidarBloque(int bloque)
        {
            for(int i = 0; i < 4; i++)
            {
                if(etiqueta[i] == bloque)
                {
                    estado[i] = 'I';
                    break;
                }
            }
        }

        //Método para el LW que trae el dato solicitado en la instrucción de load
        //En caso de que haya fallo de caché se devuelve un nulo, sin embargo antes de devolverlo
        //se resuelve el fallo
        public int? traerDato(int palabra, int bloque, ref Procesador cpu)//recupera un int de la cache para subirlo al procesador
        {   //[16] [3] [24] [2] [12] [0] [14]
            //[][][][] [][][][] [][][][] [][][][]
            //[16][24][12][14] --> etiqueta
            bool miss = true;
            int? dato = null;
            if (Monitor.TryEnter(cpu.cacheD))
            {
                try
                {
                    int posicion = bloque % 4;
                    int bloqueSalvado = cpu.cacheD.etiqueta[posicion];
                    for (int i = 0; i < 4; i++)
                    {
                        if (etiqueta[i] == bloque && estado[i] != 'I')
                        {
                            miss = false;
                            dato = datos[i * 4 + palabra];
                            break;
                        }
                    }
                    if (miss)
                    {
                        if (bloque >= 0 && bloque < 8)
                        {
                            if (procesarBloqueVictima(bloqueSalvado, posicion, ref cpu))
                            {
                               dato = falloCacheDatos(bloque, palabra, ref multiprocesador.cpu1.directorio, ref cpu);
                            }  
                        }      
                        else if (bloque >= 8 && bloque < 16)
                        {      
                            if (procesarBloqueVictima(bloqueSalvado, posicion, ref cpu))
                            {  
                                dato = falloCacheDatos(bloque, palabra, ref multiprocesador.cpu2.directorio, ref cpu);
                            }  
                        }      
                        else if (bloque >= 16 && bloque < 24)
                        {      
                            if (procesarBloqueVictima(bloqueSalvado, posicion, ref cpu))
                            {  
                                dato = falloCacheDatos(bloque, palabra, ref multiprocesador.cpu3.directorio, ref cpu);
                            }
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(cpu.cacheD);
                }
            }
            return dato;
        }

        public int[] traerDatoCacheRemota(int bloque, ref CacheDatos cache)//Método para traer un dato de una caché remota, se da en caso de un miss
        {                                                  //y que tras revisar el directorio, el bloque buscado esté modificado
            int[] bloqueRetornado = new int[4];

            int posicion = 0;
            for(int i = 0; i < 4; i++)
            {
                if (cache.etiqueta[i] == bloque && cache.estado[i] == 'M')
                    posicion = i;
            }
            //tal vez hay que modular el bloque
            //bloque = bloque % 8;
            for (int i = 0; i < 4; i++)
            {
                bloqueRetornado[i] = cache.datos[posicion + i];
            }

            return bloqueRetornado;
        }

        public int? falloCacheDatos(int bloque, int palabra, ref Directorio directorio, ref Procesador cpu)//directorio es el dueño actual del bloque, no al que le voy a asignar el bloque
        {
            //el tipo indica si el fallo es de load o store
            //bloque es el número de bloque que se está buscando
            //directorio es el directorio dueño de ese bloque
            //cpu es el procesador desde el cual hay fallo de caché


            int bloqueLocal = bloque % 8;
            int posicion = bloque % 4; //indica el bloque que va a ser reemplazado en la cache para resolver el miss
            int[] bloqueRetornado = new int[4];
            int bloqueSalvado = cpu.cacheD.etiqueta[posicion];//Bloque que se va a salvar en caso de que no se le pueda nada más caer encima
            int? dato = null;
            //Se hace lo necesario con el bloque víctima
            //procesarBloqueVictima(bloqueSalvado, posicion, ref cpu);
            // una vez resuelto cualquier conflicto, se obtiene el bloque nuevo que se copiará en la caché
            bool finalizado = true;
            if (Monitor.TryEnter(directorio.dir))
            {
                try
                {
                    if (directorio.dir[bloqueLocal].condicion == 'M')
                    {
                        //En caso de que el bloque buscado esté modificado en su directorio dueño
                        //se procede a traerlo de la caché donde se encuentra
                        //con el bloque a mano se procede a copiarlo en la cache propia
                        // y en la memoria compartida donde pertenezca
                        if (bloque >= 0 && bloque < 8)
                        {
                            if (Monitor.TryEnter(multiprocesador.cpu1.cacheD))
                            {
                                try
                                {
                                    bloqueRetornado = traerDatoCacheRemota(bloque, ref multiprocesador.cpu1.cacheD);//se escribe en memoria el bloque obtenido de la cache
                                    multiprocesador.cpu1.memoria.escribirDatos(bloqueRetornado, bloque);
                                }
                                finally
                                {
                                    Monitor.Exit(multiprocesador.cpu1.cacheD);
                                }
                            }
                            else
                            {
                                finalizado = false;
                            }

                        }
                        else if (bloque >= 8 && bloque < 16)
                        {
                            if (Monitor.TryEnter(multiprocesador.cpu2.cacheD))
                            {
                                try
                                {
                                    bloqueRetornado = traerDatoCacheRemota(bloque, ref multiprocesador.cpu2.cacheD);//se escribe en memoria el bloque obtenido de la cache
                                    multiprocesador.cpu2.memoria.escribirDatos(bloqueRetornado, bloque);
                                }
                                finally
                                {
                                    Monitor.Exit(multiprocesador.cpu2.cacheD);
                                }
                            }
                            else
                            {
                                finalizado = false;
                            }
                        }
                        else if (bloque >= 16 && bloque < 24)
                        {
                            if (Monitor.TryEnter(multiprocesador.cpu3.cacheD))
                            {
                                try
                                {
                                    bloqueRetornado = traerDatoCacheRemota(bloque, ref multiprocesador.cpu3.cacheD);//se escribe en memoria el bloque obtenido de la cache
                                    multiprocesador.cpu3.memoria.escribirDatos(bloqueRetornado, bloque);
                                }
                                finally
                                {
                                    Monitor.Exit(multiprocesador.cpu3.cacheD);
                                }
                            }
                            else
                            {
                                finalizado = false;
                            }
                        }

                    }
                    else //El bloque no se encuentra en ninguna caché, por lo que se sube directo desde memoria sin "pedir permisos".
                    {    //Este es el caso en el que el bloque a traer esté compartido o uncached
                        if (bloque >= 0 && bloque < 8)
                        {
                            bloqueRetornado = multiprocesador.cpu1.memoria.traerBloqueDatos(bloque % 8);
                        }
                        else if (bloque >= 8 && bloque < 16)
                        {
                            bloqueRetornado = multiprocesador.cpu2.memoria.traerBloqueDatos(bloque % 8);
                        }
                        else if (bloque >= 16 && bloque < 24)
                        {
                            bloqueRetornado = multiprocesador.cpu3.memoria.traerBloqueDatos(bloque % 8);
                        }

                    }

                    if(finalizado)
                    {
                        //Como se va a subir un nuevo bloque a la caché local entonces se marca el cpu respectivo
                        //dentro del directorio dueño del bloque, además se coloca que dicho bloque ahora está compartido
                        int numDir = cpu.id - 1;
                        directorio.dir[bloqueLocal].condicion = 'C';
                        directorio.dir[bloqueLocal].estado[numDir] = true;
                        //AGREGAR CICLO DE ATRASO!!!
                        //En este punto ya se tiene el nuevo bloque a copiarse en la cache y se copia normalmente
                        //pues ya se procesó el bloque víctima
                        for (int i = 0; i < 4; i++)
                        {
                            cpu.cacheD.datos[posicion + i] = bloqueRetornado[i];
                        }
                        //Una vez copiado el nuevo bloque en la caché entonces se cambia el estado de la caché
                        //para marcar que ese bloque se encuentra presente en la caché modificando la etiqueta
                        //y se cambia el estado para mostrar que se encuentra compartido
                        cpu.cacheD.etiqueta[posicion] = bloque;
                        cpu.cacheD.estado[posicion] = 'C';

                        //Al final obtiene el dato directamente del bloque retornado para que pueda ser pasado
                        //a un registro, en el caso de un load, en caso de un store dato sigue en nulo
                        dato = bloqueRetornado[palabra];
                    }
                }
                finally
                {
                    Monitor.Exit(cpu.directorio);
                }

            }
            else
            {

            }
            return dato;
        }
        

        public bool procesarBloqueVictima(int bloqueSalvado, int posicion , ref Procesador cpu)
        {
            bool termino = true;
            if (cpu.cacheD.estado[posicion] != 'I')
            {
                //procesarBloqueVictima(int bloqueSalvado, ref Procesador cpu)
                //Se hacen los arreglos necesarios a los directorios
                //Tratamiento del bloque víctima
                if (cpu.cacheD.estado[posicion] == 'C')
                {
                    int bloque = bloqueSalvado % 8;//En este caso no se salva el bloque, solo se marca como que no está compartido en la caché correspondiente

                    if (bloqueSalvado >= 0 && bloqueSalvado < 8)
                    {
                        if (Monitor.TryEnter(multiprocesador.cpu1.directorio))
                        {
                            try
                            {
                                multiprocesador.cpu1.directorio.dir[bloque].estado[cpu.id - 1] = false;
                                multiprocesador.cpu1.directorio.revisarCampo(bloque);
                            }
                            finally
                            {
                                Monitor.Exit(multiprocesador.cpu1.directorio);
                            }
                        }
                        else
                        {
                            termino = false;
                        }
                    }
                    else if (bloqueSalvado >= 8 && bloqueSalvado < 16)
                    {
                        if (Monitor.TryEnter(multiprocesador.cpu2.directorio))
                        {
                            try
                            {
                                multiprocesador.cpu2.directorio.dir[bloque].estado[cpu.id - 1] = false;
                                multiprocesador.cpu2.directorio.revisarCampo(bloque);
                            }
                            finally
                            {
                                Monitor.Exit(multiprocesador.cpu2.directorio);
                            }                                                   
                        }
                        else
                        {
                            termino = false;
                        }
                    }
                    else if (bloqueSalvado >= 16 && bloqueSalvado < 24)
                    {
                        if (Monitor.TryEnter(multiprocesador.cpu3.directorio))
                        {
                            try
                            {
                                multiprocesador.cpu3.directorio.dir[bloque].estado[cpu.id - 1] = false;
                                multiprocesador.cpu3.directorio.revisarCampo(bloque);
                            }
                            finally
                            {
                                Monitor.Exit(multiprocesador.cpu3.directorio);
                            }
                        }
                        else
                        {
                            termino = false;
                        }
                    }

                }
                else if (cpu.cacheD.estado[posicion] == 'M') //Si el bloque es modificado entonces se guarda primero en memoria antes de ser reemplazado
                {

                    int[] bloqueCopia = new int[4];

                    //Se hace una copia del bloque a salvar
                    for (int i = 0; i < 4; i++)
                    {
                        bloqueCopia[i] = this.datos[posicion + i];
                    }

                    //se guarda dicho bloque en la memoria compartida respectiva
                    if (bloqueSalvado >= 0 && bloqueSalvado < 8)
                    {
                        if (Monitor.TryEnter(multiprocesador.cpu1.directorio))
                        {
                            try
                            {
                                multiprocesador.cpu1.memoria.escribirDatos(bloqueCopia, bloqueSalvado);
                                multiprocesador.cpu1.directorio.dir[bloqueSalvado % 8].condicion = 'U';
                            }
                            finally
                            {
                                Monitor.Exit(multiprocesador.cpu1.directorio);
                            }
                        }
                        else
                        {
                            termino = false;
                        }
                    }
                    else if (bloqueSalvado >= 8 && bloqueSalvado < 16)
                    {
                        if (Monitor.TryEnter(multiprocesador.cpu2.directorio))
                        {
                            try
                            {
                                multiprocesador.cpu2.memoria.escribirDatos(bloqueCopia, bloqueSalvado);
                                multiprocesador.cpu2.directorio.dir[bloqueSalvado % 8].condicion = 'U';
                            }
                            finally
                            {
                                Monitor.Exit(multiprocesador.cpu2.directorio);
                            }
                        }
                        else
                        {
                            termino = false;
                        }
                    }
                    else if (bloqueSalvado >= 16 && bloqueSalvado < 24)
                    {
                        if (Monitor.TryEnter(multiprocesador.cpu3.directorio))
                        {
                            try
                            {
                                multiprocesador.cpu3.memoria.escribirDatos(bloqueCopia, bloqueSalvado);
                                multiprocesador.cpu3.directorio.dir[bloqueSalvado % 8].condicion = 'U';
                            }
                            finally
                            {
                                Monitor.Exit(multiprocesador.cpu3.directorio);
                            }
                        }
                        else
                        {
                            termino = false;
                        }
                    }
                }
                cpu.cacheD.estado[posicion] = (termino == true) ? 'I' : cpu.cacheD.estado[posicion];            
            }
            return termino;
            //No se escribe el caso en que el bloque a reemplazar sea inválido pues simplemente se reemplaza en esa situación

        }
    }

   public class Memoria
    {
        int[] memoria; //array de memoria "disco"
        int ptrUltimo; //puntero a la ultima posicion con datos de memoria, se utiliza como offset 
        public List<int> indiceHilos;
        public int[] memoriaC;
        public Memoria()
        {
            memoriaC = new int[32];
            memoria = new int[256];
            indiceHilos = new List<int>();
            ptrUltimo = 0; //comienza en ocho
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

        public int[] traerBloqueDatos(int bloque)
        {
            int[] bloqueretornado = new int[4];
            for(int i = 0; i < 4; i++)
            {
                bloqueretornado[i] = this.memoriaC[bloque + i];
            }
            return bloqueretornado;

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

        //Se encarga de copiar el array datos en la posición de memoria compartida indicada
        public void escribirDatos(int[] datos, int bloque ) //bloque viene sin módulo
        {
            int posicion = bloque % 8;

            for(int i = 0; i < 4; i++)
            {
                memoriaC[posicion + i] = datos[i];
            }
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

    public class Registros
    {
        int rL;
        int[] r;
        public Registros()
        {
            rL = -1;
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

    public class elementoDirectorio {
        public char condicion;
        public bool[] estado;
        public elementoDirectorio() {
            condicion = 'U';
            estado = new bool[3];
        }
    }

   public class Directorio
    {
        public elementoDirectorio[] dir;
        public Directorio()
        {
            dir = new elementoDirectorio[8];
            for (int i = 0; i < 8; i++)
            {
                dir[i] = new elementoDirectorio();
            }
        }


        //Método que checkea en un campo del directorio a ver si está marcado como compartido
        //Se puede dar el caso en el que se acaba de descheckear el último procesador
        //que tenía al bloque como compartido, en dicho caso ningun procesador lo tendría en su cach'e
        //y por lo tanto debe colocarse como "uncached"
        //"bloque" ya debe estar modulado
        public void revisarCampo(int bloque)      
        {
            bool uncached = true; 
            //Se revisan los campos de cada cpu uno a uno                                  
            for (int i = 0; i < 3; i++)
            {
                if (dir[bloque].estado[i] == true)
                {
                    uncached = false;//si en alguno está marcado entonces no está uncached por lo que su condición queda igual
                    break;
                }     
            }    
            //Si ninguno estaba marcado entonces hay que cambiar la condición de ese bloque a "uncached"
            if(uncached)
            {
                dir[bloque].condicion = 'U';
            }                                                                                  
        }

        //Se encarga de revisar el espacio correspondiente al bloque pasado por parámetro
        //si el bloque está marcado para alguno de los otros cpus, entonces se desmarca
        //id indica que procesador no debe desmarcarse pues ahora es dueño del bloque
        public void invalidarCopias(int bloque, int id)
        {
            
            for(int i = 0; i < 3; i++)
            {
                if(dir[bloque].estado[i] && i != id)
                {
                    dir[bloque].estado[i] = false;
                   
                }
                else
                {
                    dir[bloque].estado[i] = true;
                }
            }
            dir[bloque].condicion = 'M';
            
        }

        //Metodo de revisar bloque
        //Se encarga de ver cuales procesadores van a tener que ser invalidados en el directorio
        //Sin embargo no los invalida aún
        public List <int> revisarBloque(int bloque, int id)
        {
            List<int> listaCaches = new List<int>();
            for (int i = 0; i < 3; i++)
            {
                if (dir[bloque].estado[i] && i != id)
                {
                    listaCaches.Add(i + 1);
                }
               
            }
            return listaCaches;
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