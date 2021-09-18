using QueryRunner.DAO;
using System;
using System.Collections.Generic;
using System.IO;

namespace QueryRunner.Utils
{
    public class LogUtil
    {
        public static void EscreverLog(string mensagem)
        {
            string dirArqLog = ExecutorDAO.ObterParametros().Find(x => x.NomeParametro.Contains("Diretorio_Arq_LOG")).ValorParametro;

            string data = String.Format(DateTime.Now.ToString("yyyyMMdd"));
            string nomeArquivoLog = dirArqLog + "Log__" + data + ".txt";
            string msgLog = DateTime.Now.ToString() + ": " + mensagem;

            if (!Directory.Exists(dirArqLog))
            {
                Directory.CreateDirectory(dirArqLog);
            }

            EscritorDeArquivoUtil.EscreverTxt(nomeArquivoLog, msgLog);
        }
    }
}
