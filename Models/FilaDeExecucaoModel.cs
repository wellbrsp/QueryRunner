using System;

namespace QueryRunner.Models
{
    public class FilaDeExecucaoModel
    {
        private int _idFila;
        private string _servidor;
        private string _loginSolicitante;
        private DateTime _dtHoraRegistro;
        private string _nomeArqSql;
        private string _cmdSql;
        private string _dirArqEmFila;
        private string _dirArqEmExec;
        private string _nomeArqResult;
        private string _dirArqResult;
        private string _dirArqSqlErro;
        private string _dirArqSqlOk;
        private string _msgExec;
        private int _statusId;
        private string _statusDescricao;
        private DateTime _dtHoraUltimaAlteracao;

        public FilaDeExecucaoModel(int idFila, string servidor, string loginSolicitante, DateTime dtHoraRegistro, string nomeArqSql, string cmdSql, string dirArqEmFila, string dirArqEmExec, string dirArqResult, string dirArqSqlErro, string dirArqSqlOk, string msgExec, int statusId, string statusDescricao, DateTime dtHoraUltimaAlteracao)
        {
            _idFila = idFila;
            _servidor = servidor;
            _loginSolicitante = loginSolicitante;
            _dtHoraRegistro = dtHoraRegistro;
            _nomeArqSql = nomeArqSql;
            _cmdSql = cmdSql;
            _dirArqEmFila = dirArqEmFila;
            _dirArqEmExec = dirArqEmExec;
            _nomeArqResult = nomeArqSql.Substring(0, nomeArqSql.Length - 4) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "__result.csv";
            _dirArqResult = dirArqResult;
            _dirArqSqlErro = dirArqSqlErro;
            _dirArqSqlOk = dirArqSqlOk;
            _msgExec = msgExec;
            _statusId = statusId;
            _statusDescricao = statusDescricao;
            _dtHoraUltimaAlteracao = dtHoraUltimaAlteracao;
        }


        public int IdFila { get => _idFila; }
        public string Servidor { get => _servidor + @"\"; }
        public string LoginSolicitante { get => _loginSolicitante; }
        public DateTime DtHoraRegistro { get => _dtHoraRegistro; }
        public string NomeArqSql { get => _nomeArqSql; set => _nomeArqSql = value; }
        public string CmdSql { get => _cmdSql; set => _cmdSql = value; }
        public string DirArqEmFila { get => _dirArqEmFila; set => _dirArqEmFila = value; }
        public string DirArqEmExec { get => _dirArqEmExec; set => _dirArqEmExec = value; }
        public string NomeArqResult { get => _nomeArqResult; }
        public string DirArqResult { get => _dirArqResult; set => _dirArqResult = value; }
        public string DirArqSqlErro { get => _dirArqSqlErro; set => _dirArqSqlErro = value; }
        public string DirArqSqlOk { get => _dirArqSqlOk; set => _dirArqSqlOk = value; }
        public string MsgExecucao { get => _msgExec; set => _msgExec = value; }
        public int StatusId { get => _statusId; set => _statusId = value; }
        public string StatusDescricao { get => _statusDescricao; }
        public DateTime DtHoraUltimaAlteracao { get => _dtHoraUltimaAlteracao; set => _dtHoraUltimaAlteracao = value; }
    }
}
