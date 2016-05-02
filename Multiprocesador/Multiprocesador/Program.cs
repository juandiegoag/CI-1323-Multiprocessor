using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiprocesador
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("no es unsigned. ");
            Console.Write("fin del programa. ");
        }
    }
    class Simulador
    {
        int reloj; //dame la hora 
        int quantum;//of solace 

    }
}

    class Procesador
    {
        int cicloActual;
        int cP;
        Cache cache;
        Memoria memoria;
        Registros registro;
    
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

    }

    class Cache
    {

        int[] memoria = new int[64]; //4 chars = 1 palabra
        int[] bloke   = new int[4]; //numero de bloque que se tiene en memoria cache


        public char[4] traerPalabra(int bloque, int palabra)
        {
            return 'fuck';
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
        char[] memoria = new char[256];

        public char[4] traerPalabra(int bloque, int palabra)
        {
            return 'fuck';
        }
    }

    class Registros
    {
        int[] r = new int[32];

    }
}

