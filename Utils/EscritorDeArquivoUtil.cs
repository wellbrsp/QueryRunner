using System;
using System.IO;
using System.Text;

namespace QueryRunner.Utils
{
    class EscritorDeArquivoUtil
    {
        public static bool EscreverCSV(string dirDestino, string resultadoSql)
        {
            //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Iniciando escrita do result (.csv) " + diretorio);
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(dirDestino, true, Encoding.GetEncoding("iso-8859-1"));
                sw.WriteLine(resultadoSql);
                
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Erro");
                //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Erro ao escrever result ( " + diretorio + " )" + e.Message);
                return false;
            }
            finally
            {
                sw.Flush();
                sw.Close();
            }
            //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Finalizando escrita do result (.csv) " + diretorio);
        }

        public static void EscreverTxt(string diretorio, string texto)
        {
            StreamWriter sw = null;

            using (sw = new StreamWriter(diretorio, true, Encoding.GetEncoding("iso-8859-1")))
            {
                sw.WriteLine(texto);
            }
        }
    }
}
