using QueryRunner.DAO;
using QueryRunner.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QueryRunner.Utils
{
    class MovimentadorDeArquivoUtil
    {
        public static bool MoverArquivosSQL(int idFila, string arqOrigem, string arqDestino)
        {
            //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Inicio do método " + MethodBase.GetCurrentMethod().Name);
            //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Iniciando movimentação do arquivo " + arqOrigem + " para o diretorio " + arqDestino);



            if (File.Exists(arqDestino)) 
            {
                int statusFila = ExecutorDAO.ObterFilaExecucao(0, idFila)[0].StatusId;

                /* Status
                 * 1-Aguardando execução
                 * 2-Em execução
                 * 3-Executado com sucesso
                 * 4-Executado com erro
                */
                if (statusFila == 3 || statusFila == 4)
                {
                    File.Delete(arqDestino);
                    File.Move(arqOrigem, arqDestino);

                    //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Finalizando movimentação do arquivo " + arqOrigem + " para o diretorio " + arqDestino);
                    //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Fim do método " + MethodBase.GetCurrentMethod().Name);

                    return true;
                }
                else
                {
                    //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Movimentação cancelada, " + arqOrigem + " ainda está em fila de execução");
                    //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Fim do método " + MethodBase.GetCurrentMethod().Name);

                    return false;
                }
            }
            else
            {
                File.Move(arqOrigem, arqDestino);

                //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Finalizando movimentação do arquivo " + arqOrigem + " para o diretorio " + arqDestino);
                //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Fim do método " + MethodBase.GetCurrentMethod().Name);

                return true;
            }
        }
        
        public static List<string> CriarDiretorios(List<ParametroModel> parametros)
        {
            LogUtil.EscreverLog("Iniciando o método - " + MethodBase.GetCurrentMethod().Name);

            List<string> diretorios = DiretorioUtil.ObterDiretorios(parametros);

            foreach (var dir in diretorios)
            {
                try
                {
                    //Verifica estrutura de pastas de acordo com a parametrização, caso não exista, cria a estrutura de pastas
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                }
            }
            
            LogUtil.EscreverLog("Finalizando o método - " + MethodBase.GetCurrentMethod().Name);

            return diretorios;
        }
    }
}
