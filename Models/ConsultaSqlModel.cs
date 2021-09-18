using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryRunner.Models
{
    class ConsultaSqlModel
    {
        private int _idFila;
        private string _servidor;
        private string _nomeArquivoSql;
        private string _comandoSql;
        private string _nomeArqResult;
        private List<string> _retornoConsultaSql;

        public ConsultaSqlModel(int idFila, string servidor, string nomeArquivoSql, string comandoSql, List<string> retornoConsultaSql)
        {
            _idFila = idFila;
            _servidor = servidor;
            _nomeArquivoSql = nomeArquivoSql;
            _comandoSql = comandoSql;
            _nomeArqResult = nomeArquivoSql.Substring(0, nomeArquivoSql.Length - 4) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "__result.csv";
            _retornoConsultaSql = retornoConsultaSql;
        }

        public int IdFila { get => _idFila; }
        public string Servidor { get => _servidor; set => _servidor = value; }
        public string NomeArquivoSql { get => _nomeArquivoSql; set => _nomeArquivoSql = value; }
        public string ComandoSql { get => _comandoSql; set => _comandoSql = value; }
        public string NomeArqResult { get => _nomeArqResult; }
        public List<string> RetornoConsultaSql { get => _retornoConsultaSql; set => _retornoConsultaSql = value; }
    }
}
