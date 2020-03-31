using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Basureneitor
{
    public partial class AnalizadorLexico : Form
    {
        
        public AnalizadorLexico()
        {
            InitializeComponent();
        }
        string cs = @"Data Source=OSWALDO\SQLEXPRESS;Initial Catalog=analizador_lexico;Persist Security Info=True;User ID=admin;Password=12345"; 
        
        List<Linea> lineas = new List<Linea>();

        private void RtbEntrada_TextChanged(object sender, EventArgs e)
        {
           
        }
        public string ObtenerToken(List<string> _caracteres)
        {
            string result = null;
            SqlConnection con = new SqlConnection(cs);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            int z = 0;
            int estado = 0;
            foreach (string x in _caracteres)
            {
                z++;
                try
                {
                    cmd.CommandText = $@"SELECT [A{x}]  FROM Matriz WHERE [A{x}] IS NOT NULL AND Estado = {estado.ToString()}";
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                estado = int.Parse(rdr[0].ToString());
                            }
                            rdr.Close();
                        }
                        else
                        {
                            rdr.Close();
                            //En el caso de que no exista, pues se busca por una definicion regular
                            string x2 = ObtenerDefinicion(x);
                            cmd.CommandText = $@"SELECT [A{x2}]  FROM Matriz WHERE [A{x2}] IS NOT NULL AND Estado = {estado.ToString()}";
                            using (SqlDataReader rdr2 = cmd.ExecuteReader())
                            {
                                if (rdr2.HasRows)
                                {
                                    while (rdr2.Read())
                                    {
                                        estado = int.Parse(rdr2[0].ToString());
                                    }
                                }
                                else
                                {
                                    return null;
                                }
                                rdr2.Close();
                            }
                        }
                    }
                    // Si es el ultimo elemento hace un paso extra para pasar el delimitador a directamente el estado del token
                    if (z == _caracteres.Count())
                    {
                        cmd.CommandText = $"SELECT [ADEL]  FROM Matriz WHERE [ADEL] IS NOT NULL AND Estado = {estado.ToString()}";
                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            if (rdr.HasRows)
                            {
                                while (rdr.Read())
                                {
                                    estado = int.Parse(rdr[0].ToString());
                                }
                            }
                            else
                            {
                                //En el caso de que no exista, pues se busca por una definicion regular
                                string x2 = ObtenerDefinicion(x);
                                cmd.CommandText = $@"SELECT [A{x2}]  FROM Matriz WHERE [A{x2}] IS NOT NULL AND Estado = {estado.ToString()}";
                                using (SqlDataReader rdr2 = cmd.ExecuteReader())
                                {
                                    if (rdr2.HasRows)
                                    {
                                        while (rdr2.Read())
                                        {
                                            estado = int.Parse(rdr2[0].ToString());
                                        }
                                        rdr2.Close();
                                    }
                                    else
                                    {
                                        rdr2.Close();
                                        return null;
                                    }
                                }
                            }
                            rdr.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    string x2 = ObtenerDefinicion(x);
                    cmd.CommandText = $@"SELECT [A{x2}]  FROM Matriz WHERE [A{x2}] IS NOT NULL AND Estado = {estado.ToString()}";
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                estado = int.Parse(rdr[0].ToString());
                            }
                            rdr.Close();
                        }
                        else
                        {
                            rdr.Close();
                            return null;
                        }
                    }
                    if (z == _caracteres.Count())
                    {
                        cmd.CommandText = $"SELECT [ADEL]  FROM Matriz WHERE [ADEL] IS NOT NULL AND Estado = {estado.ToString()}";
                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            if (rdr.HasRows)
                            {
                                while (rdr.Read())
                                {
                                    estado = int.Parse(rdr[0].ToString());
                                }
                            }
                            else
                            {
                                rdr.Close();
                                return null;
                            }
                            rdr.Close();
                        }
                    }
                }
            }
            //Al haber analizado todos los caracteres y tener exitosamente un estado final se busca el token en ese estado
            cmd.CommandText = $"SELECT token FROM Matriz WHERE Estado = {estado.ToString()}";
            using (SqlDataReader rdr = cmd.ExecuteReader())
            {
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        string tkn = rdr["token"].ToString();
                        if (String.IsNullOrEmpty(tkn))
                        {
                            return null;
                        }
                        else
                        {
                            return tkn;
                        }
                    }
                    rdr.Close();
                }
                else
                    rdr.Close();
                return null;
            }
            return null;
        }
        public string ObtenerDefinicion(string x)
        {
            byte[] asciiBytes = Encoding.ASCII.GetBytes(x);
            if (asciiBytes[0] >= 65 && asciiBytes[0] <= 122)
            {
                return "LETRA";
            }
            else if (asciiBytes[0] >= 48 && asciiBytes[0] <= 57)
            {
                return "DIGITO";
            }
            else
            {
                return "LETRA";
            }
        }

        private void BtnCargar_Click(object sender, EventArgs e)
        {
            numeros.Clear();
            decimales.Clear();
            cadenas.Clear();
            rtbSalida.Clear();
            //Se separan las lineas del richtextbox y las instrucciones
            lineas.Clear();
            string linea = "";
            for (int x = 0; x < rtbEntrada.Text.Length; x++)
            {
                byte[] asciiBytes = Encoding.ASCII.GetBytes(rtbEntrada.Text[x].ToString());
                if (asciiBytes[0] == 10)
                {
                    lineas.Add(new Linea(linea));
                    linea = "";
                }
                else if (x == rtbEntrada.Text.Length - 1)
                {
                    linea += rtbEntrada.Text[x].ToString();
                    lineas.Add(new Linea(linea));
                    linea = "";
                }
                else
                {
                    linea += rtbEntrada.Text[x].ToString();
                }
            }
            //Transformacion a tokens
            foreach (Linea l in lineas)
            {
                foreach (Instruccion i in l.instrucciones)
                {
                    i.token = ObtenerToken(i.caracteres);
                    if (i.token == null)
                    {
                        MessageBox.Show($"Error en la palabra reservada {i.contenido}, no existe un camino en su matriz de transicion que de como resultado un token.");
                    }
                    else
                    {
                        rtbSalida.Text += BuscarIncrementable(i.token.Substring(0, 4), i.contenido) + " ";
                    }
                }

                if (!String.IsNullOrEmpty(l.contenido))
                    rtbSalida.Text += (" \n");
            }
        }

        private Dictionary<string, string> numeros = new Dictionary<string, string>();
        private Dictionary<string, string> decimales = new Dictionary<string, string>();
        private Dictionary<string, string> variables = new Dictionary<string, string>();
        private Dictionary<string, string> cadenas = new Dictionary<string, string>();
        public string BuscarIncrementable(string _token, string _valor)
        {
            int digito = 0;
            string newToken = _token;
            try
            {

                switch (_token.Substring(0, 2))
                {
                    case "NU":

                        if (numeros.Count == 0)
                        {
                            digito++;
                            newToken = "NU" + "0" + digito.ToString();
                            numeros.Add(_valor, newToken);
                        }
                        else
                        {
                            digito = int.Parse(numeros.Last().Value.Substring(2, 2));
                            if (digito < 9)
                            {
                                digito++;
                                newToken = "NU" + "0" + digito.ToString();
                            }
                            else
                            {
                                digito++;
                                newToken = "NU" + digito.ToString();
                            }
                            if (!numeros.ContainsKey(_valor))
                            {
                                numeros.Add(_valor, newToken);
                            }
                            else
                            {
                                newToken = numeros[_valor];
                            }
                        }






                        break;
                    case "DE":
                        if (decimales.Count == 0)
                        {
                            digito++;
                            newToken = "DE" + "0" + digito.ToString();
                            decimales.Add(_valor, newToken);
                        }
                        else
                        {
                            digito = int.Parse(decimales.Last().Value.Substring(2, 2));
                            if (digito < 9)
                            {
                                digito++;
                                newToken = "DE" + "0" + digito.ToString();
                            }
                            else
                            {
                                digito++;
                                newToken = "DE" + digito.ToString();
                            }
                            if (!decimales.ContainsKey(_valor))
                            {
                                decimales.Add(_valor, newToken);
                            }
                            else
                            {
                                newToken = decimales[_valor];
                            }
                        }
                        break;
                    case "CA":
                        if (cadenas.Count == 0)
                        {
                            digito++;
                            newToken = "CA" + "0" + digito.ToString();
                            cadenas.Add(_valor, newToken);
                        }
                        else
                        {
                            digito = int.Parse(cadenas.Last().Value.Substring(2, 2));
                            if (digito < 9)
                            {
                                digito++;
                                newToken = "CA" + "0" + digito.ToString();
                            }
                            else
                            {
                                digito++;
                                newToken = "CA" + digito.ToString();
                            }
                            if (!cadenas.ContainsKey(_valor))
                            {
                                cadenas.Add(_valor, newToken);
                            }
                            else
                            {
                                newToken = cadenas[_valor];
                            }
                        }
                        break;
                    case "ID":
                        if (variables.Count == 0)
                        {
                            digito++;
                            newToken = "ID" + "0" + digito.ToString();
                            variables.Add(_valor, newToken);
                        }
                        else
                        {
                            digito = int.Parse(variables.Last().Value.Substring(2, 2));
                            if (digito < 9)
                            {
                                digito++;
                                newToken = "ID" + "0" + digito.ToString();
                            }
                            else
                            {
                                digito++;
                                newToken = "ID" + digito.ToString();
                            }
                            if (!variables.ContainsKey(_valor))
                            {
                                variables.Add(_valor, newToken);
                            }
                            else
                            {
                                newToken = variables[_valor];
                            }
                        }
                        break;
                    default:
                        //   MessageBox.Show($"El token: {_valor} no tiene incrementable");
                        break;
                }
                return newToken;
            }
            catch
            {
                return newToken;
            }

        }

    }
}
