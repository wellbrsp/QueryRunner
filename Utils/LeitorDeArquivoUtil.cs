using QueryRunner.DAO;
using QueryRunner.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace QueryRunner.Utils
{
    public class LeitorDeArquivoUtil
    {
        public static ConcurrentDictionary<int, string> LerArquivosSql(Dictionary<int, string> listaArquivosEmFilaDeExecucao)
        {
            //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Inicio do método: " + MethodBase.GetCurrentMethod().Name);
            ConcurrentDictionary<int, string> cmmdDictionary = new ConcurrentDictionary<int, string>();

            if (listaArquivosEmFilaDeExecucao.Count > 0)
            {
                foreach (KeyValuePair<int, string> arq in listaArquivosEmFilaDeExecucao)
                {
                    if (arq.Value.Contains(".sql"))
                    {
                        try
                        {
                            string comandoSql = File.ReadAllText(arq.Value);
                            cmmdDictionary.TryAdd(arq.Key, comandoSql);
                        }
                        catch (Exception ex)
                        {
                            //LogUtil.EscreverLog(DateTime.Now.ToString() + ": " + ex.Message);
                        }
                    }
                }
            } 
            else
            {
                //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Nenhum arquivo .sql localizado");
            }

            //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Fim do método: " + MethodBase.GetCurrentMethod().Name);
            return cmmdDictionary;
        }

        public static ConcurrentDictionary<int, string> LerLinhasArquivosSql(Dictionary<int, string> listaArquivosEmFilaDeExecucao)
        {
            //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Inicio do método: " + MethodBase.GetCurrentMethod().Name);
            ConcurrentDictionary<int, string> cmmdDictionary = new ConcurrentDictionary<int, string>();
            string linha;
            string comandoSql = "";

            if (listaArquivosEmFilaDeExecucao.Count > 0)
            {
                foreach (KeyValuePair<int, string> arq in listaArquivosEmFilaDeExecucao)
                {
                    int idFila = arq.Key;
                    string nomeArquivo = arq.Value.ToLower();
                    string extensaoSql = ".sql".ToLower();

                    if (nomeArquivo.Contains(extensaoSql))
                    {
                        try
                        {
                            StreamReader sr = new StreamReader(arq.Value);    

                            while((linha = sr.ReadLine()) != null)
                            {
                                comandoSql += linha + " ";
                            }
                            
                            if (DataBaseUtil.ValidarSqlInjection(comandoSql))
                            {
                                string dirArqSqlErro = ExecutorDAO.ObterParametros().Find(x => x.NomeParametro.Contains("")).ValorParametro;
                                string nomeArqSql = ExecutorDAO.ObterFilaExecucao(0, idFila)[0].NomeArqSql;

                                ExecutorDAO.AtualizarFila(idFila, comandoSql, null, null, null, dirArqSqlErro, null, "Comando SQL contém código indevido", 4);
                                
                                comandoSql = null;
                            }

                            sr.Close();
                        }
                        catch (Exception ex)
                        {
                            //LogUtil.EscreverLog(DateTime.Now.ToString() + ": " + ex.Message);
                        }

                        if (!String.IsNullOrEmpty(comandoSql))
                        {
                            cmmdDictionary.TryAdd(idFila, comandoSql);
                        }

                        comandoSql = "";
                    }
                }
            } 
            else
            {
                //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Nenhum arquivo .sql localizado");
            }

            //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Fim do método: " + MethodBase.GetCurrentMethod().Name);
            return cmmdDictionary;
        }

        public static void RegistrarArquivoEmFila(List<ParametroModel> parametros, List<string> diretorios)
        {
            LogUtil.EscreverLog("Iniciando o método - " + MethodBase.GetCurrentMethod().Name);

            Dictionary<string, string> arquivosEmFila = new Dictionary<string, string>();

            var dirRaiz = parametros.Find(x => x.NomeParametro.Contains("Diretorio_Raiz"));
            var dirArqExec = parametros.Find(x => x.NomeParametro.Contains("Diretorio_Arq_Em_Execucao"));
            var dirArqSqlErro = parametros.Find(x => x.NomeParametro.Contains("Diretorio_Arq_SQL_Erro"));

            // Obtem servidores e arquivos que estão em fila
            foreach (var dir in diretorios)
            {
                string[] arquivos = null;

                if (dir.Contains("Fila") && !dir.Contains("Execucao"))
                {
                    arquivos = Directory.GetFiles(dir);

                    foreach (var arq in arquivos)
                    {
                        string[] diretorioSplitted = arq.Split('\\');
                        string servidor = diretorioSplitted[3];

                        arquivosEmFila.Add(arq, servidor);
                    }
                }
            }

            foreach (KeyValuePair<string, string> arq in arquivosEmFila)
            {
                string dirOrigem = arq.Key;
                string servidor = arq.Value;
                
                int i = dirOrigem.LastIndexOf(@"\") + 1;
                string nomeArqSql = dirOrigem.Substring(i, dirOrigem.Length - i);

                string loginSolicitante = nomeArqSql.Substring(0, 6).ToUpper();

                LogUtil.EscreverLog("Registrando arquivo " + dirOrigem);

                // Valida login do solicitante
                if (ExecutorDAO.ObterDadosColaboradorGestor(loginSolicitante) != null)
                {
                    string dirDestino = dirRaiz.ValorParametro + servidor + @"\" + dirArqExec.ValorParametro + nomeArqSql;
                    int statusAguardandoExec = 1;

                    if (MovimentadorDeArquivoUtil.MoverArquivosSQL(0, dirOrigem, dirDestino))
                    {
                        ExecutorDAO.RegistrarArquivo(servidor, loginSolicitante, dirOrigem, nomeArqSql, null, null, statusAguardandoExec);
                    }
                }
                else
                {
                    LogUtil.EscreverLog("Solicitante não identificado");

                    string dirDestino = dirRaiz.ValorParametro + servidor + @"\" + dirArqSqlErro.ValorParametro + nomeArqSql;
                    int statusLoginNaoLocalizado = 5;

                    if (MovimentadorDeArquivoUtil.MoverArquivosSQL(0, dirOrigem, dirDestino))
                    {
                        ExecutorDAO.RegistrarArquivo(servidor, null, dirOrigem, nomeArqSql, null, dirDestino, statusLoginNaoLocalizado);
                    }
                }
            }

            LogUtil.EscreverLog("Finalizando o método - " + MethodBase.GetCurrentMethod().Name);
        }
    }
}
