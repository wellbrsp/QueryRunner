using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryRunner.Models
{
    class ColaboradorGestorModel
    {
        private string _nomeColaborador;
        private string _loginColaborador;
        private string _emailColaborador;
        private string _nomeGestor;
        private string _loginGestor;
        private string _emailGestor;

        public ColaboradorGestorModel(string nomeColaborador, string loginColaborador, string emailColaborador, string nomeGestor, string loginGestor, string emailGestor)
        {
            _nomeColaborador = nomeColaborador;
            _loginColaborador = loginColaborador;
            _emailColaborador = emailColaborador;
            _nomeGestor = nomeGestor;
            _loginGestor = loginGestor;
            _emailGestor = emailGestor;
        }

        public string NomeColaborador { get => _nomeColaborador; }
        public string LoginColaborador { get => _loginColaborador; }
        public string EmailColaborador { get => _emailColaborador; }
        public string NomeGestor { get => _nomeGestor; }
        public string LoginGestor { get => _loginGestor; }
        public string EmailGestor { get => _emailGestor; }
    }
}
