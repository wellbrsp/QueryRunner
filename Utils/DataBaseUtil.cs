using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryRunner.Utils
{
    static class DataBaseUtil
    {
        public static SqlConnection GetSqlConnection(string dataSource)
        {
            string ConnectionString = @"Data Source=" + dataSource + ";Initial Catalog=master;User Id=USER_QUERY_RUNNER;Password=polinesia87";

            SqlConnection sqlConn = new SqlConnection(ConnectionString);

            return sqlConn;
        }

        public static bool ValidarSqlInjection(string cmdSql)
        {
            cmdSql = cmdSql.ToUpper();

            char[] arrayCmdSql = cmdSql.ToCharArray();
            StringBuilder linha = new StringBuilder("");

            // Varre todos os caracteres das linhas de comando para remover espaços em duplicidade
            for (int i = 0; i < arrayCmdSql.Length; i++)
            {
                linha.Append(arrayCmdSql[i]);

                if (i > 0 && i < arrayCmdSql.Length)
                {
                    if (arrayCmdSql[i - 1].Equals(' ') && arrayCmdSql[i].Equals(' ')){
                        linha.Remove(linha.Length -1, 1);
                    }
                }
            }

            char[] arrayLinha = linha.ToString().ToCharArray();
            
            linha = linha.Replace(linha.ToString(), "");

            // VERIFICA SE POSSUI COMANDOS NÃO PERMITIDOS
            for (int i = 0; i < arrayLinha.Length; i++)
            {
                linha.Append(arrayLinha[i]);

                //DELETE
                if (linha.ToString().Contains("DELETE FROM "))
                {
                    if (!arrayLinha[i + 1].Equals("#") || !arrayLinha[i + 1].Equals("@"))
                    {
                        Console.WriteLine("Possui SQL Injection!!");
                        return true;
                    }
                }

                //TRUNCATE
                if (linha.ToString().Contains("TRUNCATE TABLE "))
                {
                    if (!arrayLinha[i + 1].Equals("#") || !arrayLinha[i + 1].Equals("@"))
                    {
                        Console.WriteLine("Possui SQL Injection!!");
                        return true;
                    }
                }

                //INSERT
                if (linha.ToString().Contains("INSERT INTO "))
                {
                    if (!arrayLinha[i + 1].Equals("#") || !arrayLinha[i + 1].Equals("@"))
                    {
                        Console.WriteLine("Possui SQL Injection!!");
                        return true;
                    }
                }

                //UPDATE
                if (linha.ToString().Contains("UPDATE "))
                {
                    if (!arrayLinha[i + 1].Equals("#") || !arrayLinha[i + 1].Equals("@"))
                    {
                        Console.WriteLine("Possui SQL Injection!!");
                        return true;
                    }
                }

                //CREATE TABLE
                if (linha.ToString().Contains("CREATE TABLE "))
                {
                    if (!arrayLinha[i + 1].Equals("#"))
                    {
                        Console.WriteLine("Possui SQL Injection!!");
                        return true;
                    }
                }

                //DROP TABLE
                if (linha.ToString().Contains("DROP TABLE "))
                {
                    if (!arrayLinha[i + 1].Equals("#"))
                    {
                        Console.WriteLine("Possui SQL Injection!!");
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
