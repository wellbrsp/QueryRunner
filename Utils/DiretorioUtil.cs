using QueryRunner.DAO;
using QueryRunner.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QueryRunner.Utils
{
    class DiretorioUtil
    {
        public static List<string> ObterDiretorios(List<ParametroModel> parametros)
        {
            List<string> listaDiretorios = new List<string>();

            List<ParametroModel> listaParametros = parametros;

            string srvExec = null;
            string dirRaiz = null;
            string dirArqEmFila = null;
            string dirArqResult = null;
            string dirArqSqlErro = null;
            string dirArqSqlOk = null;
            string dirArqSqlLog = null;
            string dirArqExec = null;

            foreach (ParametroModel parametro in parametros)
            {
                switch (parametro.NomeParametro)
                {
                    case "Servidor_Exec":
                        srvExec = parametro.ValorParametro;
                        break;
                    case "Diretorio_Raiz":
                        dirRaiz = parametro.ValorParametro;
                        break;
                    case "Diretorio_Arq_Em_Fila":
                        dirArqEmFila = parametro.ValorParametro;
                        break;
                    case "Diretorio_Arq_Result":
                        dirArqResult = parametro.ValorParametro;
                        break;
                    case "Diretorio_Arq_SQL_Erro":
                        dirArqSqlErro = parametro.ValorParametro;
                        break;
                    case "Diretorio_Arq_SQL_OK":
                        dirArqSqlOk = parametro.ValorParametro;
                        break;
                    case "Diretorio_Arq_LOG":
                        dirArqSqlLog = parametro.ValorParametro;
                        break;
                    case "Diretorio_Arq_Em_Execucao":
                        dirArqExec = parametro.ValorParametro;
                        break;
                    default:
                        break;
                }
            }

            string[] servidores = srvExec.Split(',');

            for (int i = 0; i < servidores.Length; i++)
            {
                listaDiretorios.Add(dirRaiz + servidores[i] + @"\" + dirArqEmFila);
                listaDiretorios.Add(dirRaiz + servidores[i] + @"\" +  dirArqResult);
                listaDiretorios.Add(dirRaiz + servidores[i] + @"\" +  dirArqSqlErro);
                listaDiretorios.Add(dirRaiz + servidores[i] + @"\" +  dirArqSqlOk);
                listaDiretorios.Add(dirRaiz + servidores[i] + @"\" +  dirArqSqlLog);
                listaDiretorios.Add(dirRaiz + servidores[i] + @"\" +  dirArqExec);
            }

            return listaDiretorios;
        }

        public static string ObterServidorDoArquivoPeloDiretorio(string diretorioArq)
        {
            string servidor = diretorioArq ?? "";




            return servidor;
        }
    }
}
