using QueryRunner.DAO;
using QueryRunner.Models;
using QueryRunner.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Timers;

namespace QueryRunner
{
    public partial class Service1 : ServiceBase
    {
        Timer timer1 = new Timer();

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //timer1.Elapsed += new ElapsedEventHandler(timer1_Elapsed);

            //timer1.Interval = 15000; // 15 segundos
            //timer1.Enabled = true;
            //timer1.Start();
        }

        public void timer1_Elapsed()
        //private void timer1_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Carrega parametros
            List<ParametroModel> parametros = ExecutorDAO.ObterParametros();

            string dirRaiz = parametros.Find(x => x.NomeParametro.Contains("Diretorio_Raiz")).ValorParametro;
            string dirArqSqlEmExec = parametros.Find(x => x.NomeParametro.Contains("Diretorio_Arq_Em_Execucao")).ValorParametro;
            string dirArqSqlOk = parametros.Find(x => x.NomeParametro.Contains("Diretorio_Arq_SQL_OK")).ValorParametro;
            string dirArqSqlErro = parametros.Find(x => x.NomeParametro.Contains("Diretorio_Arq_SQL_Erro")).ValorParametro;
            string dirArqResult = parametros.Find(x => x.NomeParametro.Contains("Diretorio_Arq_Result")).ValorParametro;
            
            LogUtil.EscreverLog("::::: Iniciando processamento :::::");

            //cria e retorna lista com relação de diretórios criados
            List<string> diretoriosCriados = MovimentadorDeArquivoUtil.CriarDiretorios(parametros);
                        
            // Varre diretorios e registra novos arquivos na fila (status 1-Aguardando execução)
            LeitorDeArquivoUtil.RegistrarArquivoEmFila(parametros, diretoriosCriados);

            //Obtem filas de execução com status '1-Aguardando execução e 2-Em execução'
            List<FilaDeExecucaoModel> filaStatusPendente = ExecutorDAO.ObterFilaExecucao(1, 0);
            List<FilaDeExecucaoModel> filaStatusEmExec = ExecutorDAO.ObterFilaExecucao(2, 0);

            // Verifica se existem arquivos pendentes ou em execução
            if (filaStatusPendente.Count > 0 || filaStatusEmExec.Count > 0)
            {
                // Arquivos com status 1-Aguardando execução
                if (filaStatusPendente.Count > 0)
                {
                    Dictionary<int, string> arqPendentesExecucao = new Dictionary<int, string>();
                    
                    // Registra arquivos no status 2-Em execução
                    foreach (FilaDeExecucaoModel arq in filaStatusPendente)
                    {
                        int idFila = arq.IdFila;
                        int idStatusEmExec = 2;

                        string nomeArquivoSql = arq.NomeArqSql;
                        string servidorArquivo = arq.Servidor;
                        
                        string nomeArqCompleto = dirRaiz + servidorArquivo + dirArqSqlEmExec + nomeArquivoSql;

                        ExecutorDAO.AtualizarFila(idFila, null, null, null, nomeArqCompleto, null, null, null, idStatusEmExec);

                        arqPendentesExecucao.Add(idFila, nomeArqCompleto);
                    }

                    // implementar validacoes de injection
                    //ConcurrentDictionary<int, string> comandosSQL = LeitorDeArquivoUtil.LerArquivosSql(arqPendentesExecucao);
                    ConcurrentDictionary<int, string> comandosSQL = LeitorDeArquivoUtil.LerLinhasArquivosSql(arqPendentesExecucao);
                    ConcurrentDictionary<int, ConsultaSqlModel> lstResultadoConsultaSql = new ConcurrentDictionary<int, ConsultaSqlModel>();
                    List<ConsultaSqlModel> listaConsultaSql = new List<ConsultaSqlModel>();

                    int idStatusAtual = 2;

                    //obtem resultado das consultas SQL
                    Parallel.ForEach(comandosSQL, ckv =>
                    {
                        int idFila = ckv.Key;
                        string comandoSql = ckv.Value;

                        string servidor = ExecutorDAO.ObterFilaExecucao(idStatusAtual, idFila)[0].Servidor;
                        string nomeArqSql = ExecutorDAO.ObterFilaExecucao(idStatusAtual, idFila)[0].NomeArqSql;

                        if (ExecutorDAO.ObterResultadoConsultaSql(comandoSql, idFila, parametros).Count > 0)
                        {
                            ConsultaSqlModel consultaSqlModel = new ConsultaSqlModel(idFila, servidor, nomeArqSql, comandoSql, ExecutorDAO.ObterResultadoConsultaSql(comandoSql, idFila, parametros));
                            listaConsultaSql.Add(consultaSqlModel);

                            lstResultadoConsultaSql.TryAdd(idFila, consultaSqlModel);
                        }
                        else
                        {
                            string diretorioArqSqlErro = (dirRaiz + servidor + dirArqSqlErro + nomeArqSql);

                            MovimentadorDeArquivoUtil.MoverArquivosSQL(idFila, (dirRaiz + servidor + dirArqSqlEmExec + nomeArqSql), diretorioArqSqlErro);
                        }
                    });
                    
                    // Atualiza status fila
                    foreach (ConsultaSqlModel consulta in listaConsultaSql)
                    {
                        string diretorioArqSqlExec = dirRaiz + consulta.Servidor + dirArqSqlEmExec + consulta.NomeArquivoSql;
                        ExecutorDAO.AtualizarFila(consulta.IdFila, consulta.ComandoSql, null, null, diretorioArqSqlExec, null, null, null, idStatusAtual);
                    }


                    if (lstResultadoConsultaSql.Values.Count > 0)
                    {
                        //escreve arquivos .csv com os resultados
                        Parallel.ForEach(lstResultadoConsultaSql, ckv =>
                        {
                            int idFila = ckv.Key;
                            
                            string resultadoConsultaSql = null;

                            string servidor = ckv.Value.Servidor;
                            string nomeArqSql = ckv.Value.NomeArquivoSql;
                            string nomeArqCsv = ckv.Value.NomeArqResult;

                            string dirDestinoCsv = dirRaiz + servidor + dirArqResult + nomeArqCsv;

                            ckv.Value.RetornoConsultaSql.ForEach(delegate (String linha)
                            {
                                resultadoConsultaSql += linha;
                            });

                            if (EscritorDeArquivoUtil.EscreverCSV(dirDestinoCsv, resultadoConsultaSql))
                            {
                                string dirCompletoArqSqlExec = dirRaiz + servidor + dirArqSqlEmExec + nomeArqSql;
                                string dirCompletoArqSqlOk = dirRaiz + servidor + dirArqSqlOk + nomeArqSql;

                                MovimentadorDeArquivoUtil.MoverArquivosSQL(idFila, dirCompletoArqSqlExec, dirCompletoArqSqlOk);
                            }
                            else
                            {
                                listaConsultaSql.Remove(listaConsultaSql.Find(x => x.IdFila == idFila));
                            }
                        });

                        // Atualiza status fila
                        foreach (ConsultaSqlModel consulta in listaConsultaSql)
                        {
                            string diretorioArqSqlExec = dirRaiz + consulta.Servidor + dirArqSqlEmExec + consulta.NomeArquivoSql;
                            string dirDestinoCsv = dirRaiz + consulta.Servidor + dirArqResult + consulta.NomeArqResult;
                            string dirCompletoArqSqlOk = dirRaiz + consulta.Servidor + dirArqSqlOk + consulta.NomeArquivoSql;

                            ExecutorDAO.AtualizarFila(consulta.IdFila, null, consulta.NomeArqResult, dirDestinoCsv, null, null, dirCompletoArqSqlOk, null, 3);
                        }

                        // Dispara e-mail com arquivo result
                        List<FilaDeExecucaoModel> filaEnvioEmailPendente = ExecutorDAO.ObterFilaExecucao(3, 0);

                        foreach (FilaDeExecucaoModel item in filaEnvioEmailPendente)
                        {
                            string emailColaborador = ExecutorDAO.ObterDadosColaboradorGestor(item.LoginSolicitante).EmailColaborador;
                            string emailGestor = ExecutorDAO.ObterDadosColaboradorGestor(item.LoginSolicitante).EmailGestor;

                            string destinatarios = emailColaborador + ";" + emailGestor;
                            string assunto = "[ Daycoval Query Runner ] Retorno da consulta : " + item.NomeArqSql;
                            string mensagem = "";

                            /*
                             * Utilizado tags html (<b> e <br>)para formatar corpo da mensagem
                             */
                            mensagem += String.Format("<b> Data/Hora solicitação: </b> {0} <br>", item.DtHoraRegistro);
                            mensagem += String.Format("<b> Solicitante: </b> [ {0} ] - {1}  <br>", item.LoginSolicitante, ExecutorDAO.ObterDadosColaboradorGestor(item.LoginSolicitante).NomeColaborador);
                            mensagem += String.Format("<b> Gestor: </b> {0}  <br>", ExecutorDAO.ObterDadosColaboradorGestor(item.LoginSolicitante).NomeGestor);
                            mensagem += String.Format("<b> Arquivo .sql: </b> {0}  <br>", item.NomeArqSql);
                            mensagem += String.Format("<b> Arquivo retorno(.csv): </b> {0}  <br>", item.NomeArqResult);

                            ExecutorDAO.EnviarEmailViaMSSQL(item.IdFila, destinatarios, assunto, mensagem, item.DirArqResult);

                            ExecutorDAO.AtualizarFila(item.IdFila, null, null, null, null, null, null, null, 6);
                        }

                    }
                }
            }
            LogUtil.EscreverLog("::::: Finalizando processamento :::::" + "\n");
        }

        protected override void OnStop()
        {
        }
    }
}
