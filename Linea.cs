using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace Basureneitor
{
    class Linea
    {
        public string contenido;
        public string contenidoAbsoluto;
        public List<Instruccion> instrucciones = new List<Instruccion>();
        public Linea(string _contenido)
        {
            contenido = _contenido;
            ObtenerInstrucciones();
        }
        public Linea(string _contenido, string _contenidoAbsoluto)
        {
            contenidoAbsoluto = _contenidoAbsoluto;
            contenido = _contenido;
            ObtenerInstrucciones();
        }
        public void ObtenerInstrucciones()
        {

            string instruccion = "";
            for (int x = 0; x < contenido.Length; x++)
            {

                byte[] asciiBytes = Encoding.ASCII.GetBytes(contenido[x].ToString());

                if (asciiBytes[0] == 32)
                {
                    instrucciones.Add(new Instruccion(instruccion));
                    instruccion = "";
                }
                else if (x == contenido.Length - 1)
                {
                    instruccion += contenido[x].ToString();
                    instrucciones.Add(new Instruccion(instruccion));
                    instruccion = "";
                }
                else
                {
                    instruccion += contenido[x].ToString();
                }
            }

        }
    }
    //===============================================================================




    //===============================================================================
    class Instruccion
    {
       public string contenido;
        public string token;
        public List<string> caracteres = new List<string>();
        public Instruccion(string _contenido)
        {
            contenido = _contenido;
            ObtenerCaracteres();
        }
        public Instruccion(string _contenido, string _token)
        {
            contenido = _contenido;
            token = _token;
            ObtenerCaracteres();
        }

        public Instruccion()
        { ObtenerCaracteres(); }

        public int ObtenerLongitud()
        {
            int cantidad = 0;
            bool expresion = false;
            for (int x = 0; x < contenido.Length; x++)
            {
                if (contenido[x].ToString() == "<")
                {
                    expresion = true;
                }
                else if (expresion)
                {
                    if (contenido[x].ToString() == ">")
                    {
                        cantidad++;
                        expresion = false;
                    }
                }
                else { cantidad++; }
            }
            return cantidad;
        }
        public void ObtenerCaracteres()
        {
            string expre = "";
            bool expresion = false;
            for (int x = 0; x < contenido.Length; x++)
            {
                if (contenido[x].ToString() == "<")
                {
                    expresion = true;
                    expre = "<";
                }
                else if (expresion)
                {
                    expre += contenido[x];
                    if (contenido[x].ToString() == ">")
                    {
                        expresion = false;
                        caracteres.Add(expre);
                    }
                }
                else
                {
                    caracteres.Add(contenido[x].ToString());
                }

            }

        }
    }
    //==============================================================



    //==============================================================
}
