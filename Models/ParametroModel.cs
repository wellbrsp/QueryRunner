using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryRunner.Models
{
    public class ParametroModel
    {
        private int _idParametro;
        private string _nomeParametro;
        private string _descricaoParametro;
        private string _valorParametro;

        public ParametroModel(int idParametro, string nomeParametro, string descricaoParametro, string valorParametro)
        {
            _idParametro = idParametro;
            _nomeParametro = nomeParametro;
            _descricaoParametro = descricaoParametro;
            _valorParametro = valorParametro;
        }

        public int IdParametro
        {
            get
            {
                return _idParametro;
            }
        }

        public string NomeParametro
        {
            get
            {
                return _nomeParametro;
            }
        }

        public string DescricaoParametro
        {
            get
            {
                return _descricaoParametro;
            }
        }

        public string ValorParametro
        {
            get
            {
                return _valorParametro;
            }
        }
    }
}
