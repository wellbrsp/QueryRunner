using QueryRunner.Models;
using QueryRunner.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace QueryRunner.DAO
{
    static class ExecutorDAO
    {
        public static List<string> ObterResultadoConsultaSql(string cmdSql, int idFila, List<ParametroModel> parametros)
        {
            //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Inicio do método " + MethodBase.GetCurrentMethod().Name);
            string dirRaiz = parametros.Find(x => x.NomeParametro.Contains("Diretorio_Raiz")).ValorParametro;
            string dirArqResult = parametros.Find(x => x.NomeParametro.Contains("Diretorio_Arq_Result")).ValorParametro;
            string sqlMaxRegistros = parametros.Find(x => x.NomeParametro.Contains("SQL_MAX_Registros")).ValorParametro;
            string dirArqSqlErro = parametros.Find(x => x.NomeParametro.Contains("Diretorio_Arq_SQL_Erro")).ValorParametro;

            string servidor = ExecutorDAO.ObterFilaExecucao(0, idFila)[0].Servidor;
            string nomeArquivo = ExecutorDAO.ObterFilaExecucao(0, idFila)[0].NomeArqSql;

            List<string> lstRetornoConsultaSql = new List<string>();

            SqlConnection conn = null;
            string procedure = String.Format("DCV_QUERY_RUNNER.dbo.P_QUERY_RUNEER");

            using (conn = DataBaseUtil.GetSqlConnection(servidor))
            {
                conn.Open();

                SqlCommand comandoSql = new SqlCommand(procedure, conn);
                comandoSql.CommandType = CommandType.StoredProcedure;
                comandoSql.Parameters.AddWithValue("@queryParam", SqlDbType.NVarChar).Value = cmdSql;
               
                string linha = "";
                SqlDataReader dataReader = null;

                try
                {
                    dataReader = comandoSql.ExecuteReader();

                    if (dataReader.HasRows)
                    {
                        int qtdColunasSQL = dataReader.FieldCount;
                        string nomeColunaSQL;

                        // cria colunas
                        for (int i = 0; i < qtdColunasSQL; i++)
                        {
                            nomeColunaSQL = dataReader.GetName(i);
                            linha += nomeColunaSQL + ";";
                        }

                        linha = linha.Substring(0, linha.Length - 1) + "\n";

                        int nroRegistros = 0;
                        int maxRegistros = Convert.ToInt32(sqlMaxRegistros);

                        while (dataReader.Read())
                        {
                            if (nroRegistros < maxRegistros)
                            {
                                for (int i = 0; i < qtdColunasSQL; i++)
                                {
                                    linha += dataReader[i].ToString() + ";";
                                }

                                linha = linha.Substring(0, linha.Length - 1) + "\n";
                            }
                            else
                            {
                                break;
                            }

                            nroRegistros++;
                        }

                        lstRetornoConsultaSql.Add(linha);
                    }

                    dataReader.Close();
                }
                catch (Exception ex)
                {
                    int idStatusErro = 4;
                    string dirDestinoArqSqlErro = (dirRaiz + servidor + dirArqSqlErro + nomeArquivo);
                    
                    ExecutorDAO.AtualizarFila(idFila, cmdSql, null, null, null, dirDestinoArqSqlErro, null, ex.Message, idStatusErro);
                }
            }

            return lstRetornoConsultaSql;

            //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Fim do método " + MethodBase.GetCurrentMethod().Name);
        }

        public static void GravarLogUsuario(string usuario, string arqEntrada, string arqSaida, string comando, string statusExecucao, string result, DateTime data)
        {
            //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Inicio do método  " + MethodBase.GetCurrentMethod().Name);

            SqlConnection conn = null;
            string procedure = String.Format("DCV_QUERY_RUNNER.dbo.P_REGISTRA_LOG");

            using (conn = DataBaseUtil.GetSqlConnection("NOTE-WELL"))
            {
                conn.Open();
                SqlCommand comandoSql = new SqlCommand(procedure, conn);
                comandoSql.CommandType = CommandType.StoredProcedure;

                comandoSql.Parameters.AddWithValue("@usuario", SqlDbType.NVarChar).Value = usuario;
                comandoSql.Parameters.AddWithValue("@arqentrada", SqlDbType.NVarChar).Value = arqEntrada;
                comandoSql.Parameters.AddWithValue("@arqsaida", SqlDbType.NVarChar).Value = arqSaida;
                comandoSql.Parameters.AddWithValue("@comando", SqlDbType.NVarChar).Value = comando;
                comandoSql.Parameters.AddWithValue("@status_execucao", SqlDbType.NVarChar).Value = statusExecucao;
                comandoSql.Parameters.AddWithValue("@result", SqlDbType.NVarChar).Value = result;
                comandoSql.Parameters.AddWithValue("@data", SqlDbType.DateTime).Value = data;

                comandoSql.ExecuteNonQuery();
            }

            //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Fim do método " + MethodBase.GetCurrentMethod().Name);
        }

        public static void RegistrarArquivo(string servidor, string loginSolicitante, string diretorio, string nomeArqSql, string cmdSql, string dirArqSqlErro, int statusFila)
        {
            //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Inicio do método " + MethodBase.GetCurrentMethod().Name);

            cmdSql = cmdSql ?? "";
            nomeArqSql = nomeArqSql ?? "";
            loginSolicitante = loginSolicitante ?? "";
            dirArqSqlErro = dirArqSqlErro ?? "";

            SqlConnection conn = null;
            string procedure = String.Format("DCV_QUERY_RUNNER.dbo.P_REGISTRAR_ARQUIVO");

            using (conn = DataBaseUtil.GetSqlConnection("NOTE-WELL"))
            {
                conn.Open();
                SqlCommand sqlCmd = new SqlCommand(procedure, conn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.AddWithValue("@servidor", SqlDbType.NVarChar).Value = servidor;
                sqlCmd.Parameters.AddWithValue("@login_solicitante", SqlDbType.NVarChar).Value = loginSolicitante;
                sqlCmd.Parameters.AddWithValue("@diretorio", SqlDbType.NVarChar).Value = diretorio;
                sqlCmd.Parameters.AddWithValue("@nome_arquivo_sql", SqlDbType.NVarChar).Value = nomeArqSql;
                sqlCmd.Parameters.AddWithValue("@comando_sql", SqlDbType.NVarChar).Value = cmdSql;
                sqlCmd.Parameters.AddWithValue("@dir_arq_sql_erro", SqlDbType.NVarChar).Value = dirArqSqlErro;
                sqlCmd.Parameters.AddWithValue("@status_fila", SqlDbType.Int).Value = statusFila;

                sqlCmd.ExecuteNonQuery();
            }

            //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Fim do método " + MethodBase.GetCurrentMethod().Name);
        }

        public static List<FilaDeExecucaoModel> ObterFilaExecucao(int idStatusFila, int idFilaExec)
        {
            //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Inicio do método " + MethodBase.GetCurrentMethod().Name);

            List<FilaDeExecucaoModel> filaExecucao = new List<FilaDeExecucaoModel>();

            SqlConnection conn;
            
            string procedure = String.Format("DCV_QUERY_RUNNER.dbo.P_OBTER_FILA_EXECUCAO");

            using (conn = DataBaseUtil.GetSqlConnection("NOTE-WELL"))
            {
                conn.Open();
                SqlCommand sqlCmd = new SqlCommand(procedure, conn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.AddWithValue("@id_fila", SqlDbType.Int).Value = idFilaExec;
                sqlCmd.Parameters.AddWithValue("@id_status", SqlDbType.Int).Value = idStatusFila;

                using (var dataReader = sqlCmd.ExecuteReader())
                {
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            int idFila = dataReader.GetInt32(0);
                            string servidor = dataReader.IsDBNull(1) ? null : dataReader.GetString(1);
                            string loginSolicitante = dataReader.IsDBNull(2) ? null : dataReader.GetString(2);
                            DateTime dtHoraRegistro = dataReader.GetDateTime(3);
                            string nomeArqSql = dataReader.IsDBNull(4) ? null : dataReader.GetString(4);
                            string cmdSql = dataReader.IsDBNull(5) ? null : dataReader.GetString(5);
                            string dirArqEmFila = dataReader.IsDBNull(6) ? null : dataReader.GetString(6);
                            string dirArqEmExec = dataReader.IsDBNull(7) ? null : dataReader.GetString(7);
                            string dirArqResultado = dataReader.IsDBNull(9) ? null : dataReader.GetString(9);
                            string dirArqSqlErro = dataReader.IsDBNull(10) ? null : dataReader.GetString(10);
                            string dirArqSqlOk = dataReader.IsDBNull(11) ? null : dataReader.GetString(11);
                            string msgExecucao = dataReader.IsDBNull(12) ? null : dataReader.GetString(12);
                            int statsId = dataReader.GetInt32(13);
                            string statusDescricao = dataReader.IsDBNull(14) ? null : dataReader.GetString(14);
                            DateTime dtHoraUltimaAlteracao = dataReader.GetDateTime(15);

                            filaExecucao.Add(new FilaDeExecucaoModel(idFila, servidor, loginSolicitante, dtHoraRegistro, nomeArqSql, cmdSql, dirArqEmFila, dirArqEmExec, dirArqResultado, dirArqSqlErro, dirArqSqlOk, msgExecucao, statsId, statusDescricao, dtHoraUltimaAlteracao));
                        }
                    }
                }
            }

            //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Fim do método " + MethodBase.GetCurrentMethod().Name);

            return filaExecucao;
        }

        public static void AtualizarFila(int idFila, string cmdSql, string nomeArqResult, string dirArqResult, string dirArqSqlExec, string dirArqSqlErro, string dirArqSqlOk, string msgExec,int idStatusFila)
        {
            //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Inicio do método " + MethodBase.GetCurrentMethod().Name);

            string comandoSlq = cmdSql ?? "";
            string nomeArquivoResultado = nomeArqResult ?? "";
            string dirArquivoSqlExecucao = dirArqSqlExec ?? "";
            string dirArquivoResultado = dirArqResult ?? "";
            string dirArquivoSqlErro = dirArqSqlErro ?? "";
            string dirArquivoSqlOk = dirArqSqlOk ?? "";
            string msgExecucao = msgExec ?? "";

            SqlConnection conn;

            string procedure = String.Format("DCV_QUERY_RUNNER.dbo.P_ATUALIZAR_FILA");

            using (conn = DataBaseUtil.GetSqlConnection("NOTE-WELL"))
            {
                conn.Open();

                SqlCommand sqlCmd = new SqlCommand(procedure, conn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.AddWithValue("@idFila", SqlDbType.Int).Value = idFila;
                sqlCmd.Parameters.AddWithValue("@cmdSql", SqlDbType.NVarChar).Value = comandoSlq;
                sqlCmd.Parameters.AddWithValue("@nomeArqResult", SqlDbType.Int).Value = nomeArquivoResultado;
                sqlCmd.Parameters.AddWithValue("@dirArqEmExecucao", SqlDbType.Int).Value = dirArquivoSqlExecucao;
                sqlCmd.Parameters.AddWithValue("@dirArqResult", SqlDbType.Int).Value = dirArquivoResultado;
                sqlCmd.Parameters.AddWithValue("@dirArqSqlErro", SqlDbType.Int).Value = dirArquivoSqlErro;
                sqlCmd.Parameters.AddWithValue("@dirArqSqlOk", SqlDbType.Int).Value = dirArquivoSqlOk;
                sqlCmd.Parameters.AddWithValue("@msgExecucao", SqlDbType.NVarChar).Value = msgExecucao;
                sqlCmd.Parameters.AddWithValue("@idStatus", SqlDbType.Int).Value = idStatusFila;

                sqlCmd.ExecuteNonQuery();
                 
            }

            //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Fim do método " + MethodBase.GetCurrentMethod().Name);
        }

        public static List<ParametroModel> ObterParametros()
        {
            //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Inicio do método " + MethodBase.GetCurrentMethod().Name);

            SqlConnection conn;

            string procedure = String.Format("DCV_QUERY_RUNNER.dbo.P_OBTER_PARAMETROS");

            List<ParametroModel> listaParametros = new List<ParametroModel>();

            using (conn = DataBaseUtil.GetSqlConnection("NOTE-WELL"))
            {
                conn.Open();

                SqlCommand sqlCmd = new SqlCommand(procedure, conn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                using (SqlDataReader dataReader = sqlCmd.ExecuteReader())
                {
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            int id = dataReader.GetInt32(0);
                            string nome = dataReader[1].ToString();
                            string descricao = dataReader[2].ToString();
                            string valor = dataReader[3].ToString();

                            listaParametros.Add(new ParametroModel(id, nome, descricao, valor));
                        }
                    }
                }
            }

            //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Fim do método " + MethodBase.GetCurrentMethod().Name);
            return listaParametros;
        }

        //Adequar para integrar no Topdesk
        public static ColaboradorGestorModel ObterDadosColaboradorGestor(string loginSolicitante)
        {
            //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Inicio do método " + MethodBase.GetCurrentMethod().Name);

            SqlConnection conn;

            string procedure = String.Format("DCV_QUERY_RUNNER.dbo.P_OBTER_DADOS_COLABORADOR_X_GESTOR");

            ColaboradorGestorModel colaboradorGestorModel = null;

            using (conn = DataBaseUtil.GetSqlConnection("NOTE-WELL"))
            {
                conn.Open();

                SqlCommand sqlCmd = new SqlCommand(procedure, conn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.AddWithValue("@login_colaborador", SqlDbType.NVarChar).Value = loginSolicitante;

                using (SqlDataReader dataReader = sqlCmd.ExecuteReader())
                {
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            int id = dataReader.GetInt32(0);
                            string nomeGestor = dataReader.GetString(1);
                            string nomeColaborador = dataReader.GetString(2);
                            string emailGestor = dataReader.GetString(3);
                            string emailColaborador = dataReader.GetString(4);
                            string loginGestor = dataReader.GetString(5);
                            string loginColaborador = dataReader.GetString(6);

                            colaboradorGestorModel = new ColaboradorGestorModel(nomeColaborador, loginColaborador, emailColaborador, nomeGestor, loginGestor, emailGestor);
                        }
                    }
                }
            }

            //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Fim do método " + MethodBase.GetCurrentMethod().Name);
            return colaboradorGestorModel;
        }

        public static void EnviarEmailViaMSSQL(int idFila, string destinatarios, string assunto, string mensagem, string arquivos)
        {
            //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Inicio do método " + MethodBase.GetCurrentMethod().Name);

            string servidor = ExecutorDAO.ObterFilaExecucao(0, idFila)[0].Servidor;

            SqlConnection conn;
            string procedure = String.Format("DCV_QUERY_RUNNER.dbo.P_ENVIAR_EMAIL");

            using (conn = DataBaseUtil.GetSqlConnection(servidor))
            {
                conn.Open();

                SqlCommand sqlCmd = new SqlCommand(procedure, conn);
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.AddWithValue("@destinatarios", SqlDbType.NVarChar).Value = destinatarios;
                sqlCmd.Parameters.AddWithValue("@assunto", SqlDbType.NVarChar).Value = assunto;
                sqlCmd.Parameters.AddWithValue("@mensagem", SqlDbType.NVarChar).Value = mensagem;
                sqlCmd.Parameters.AddWithValue("@arquivos", SqlDbType.NVarChar).Value = arquivos;

                sqlCmd.ExecuteNonQuery();
            }

            //LogUtil.EscreverLog(DateTime.Now.ToString() + ": Fim do método " + MethodBase.GetCurrentMethod().Name);
        }
    }
}
