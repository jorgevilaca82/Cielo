using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using Cielo.Messages;
using Cielo.Helpers;
using System.Threading.Tasks;

namespace Cielo
{
    public class CieloClient
    {
        #region "Private"

        private string Numero;
        private string Chave;
        private string LojaNumero;
        private string LojaChave;
        private Uri Endpoint;
        private readonly string defaultDateFormat = "yyyyMMdd";

        #endregion

        #region "Constructor"

        public CieloClient()
        {
            if (ConfigurationManager.AppSettings["Cielo.Numero"] == null)
                throw new ArgumentNullException("Cielo.Numero");

            if (ConfigurationManager.AppSettings["Cielo.Chave"] == null)
                throw new ArgumentNullException("Cielo.Chave");

            if (ConfigurationManager.AppSettings["Cielo.LojaNumero"] == null)
                throw new ArgumentNullException("Cielo.LojaNumero");

            if (ConfigurationManager.AppSettings["Cielo.LojaChave"] == null)
                throw new ArgumentNullException("Cielo.LojaChave");

            if (ConfigurationManager.AppSettings["Cielo.Ambiente"] == null)
                throw new ArgumentNullException("Cielo.Ambientes");

            Numero = ConfigurationManager.AppSettings["Cielo.Numero"];
            Chave = ConfigurationManager.AppSettings["Cielo.Chave"];

            LojaNumero = ConfigurationManager.AppSettings["Cielo.LojaNumero"];
            LojaChave = ConfigurationManager.AppSettings["Cielo.LojaChave"];

            if (ConfigurationManager.AppSettings["Cielo.Ambiente"].Equals("Producao"))
                Endpoint = new Uri(ConfigurationManager.AppSettings["Cielo.UrlProducao"]);
            else
                Endpoint = new Uri(ConfigurationManager.AppSettings["Cielo.UrlTeste"]);
        }

        #endregion

        #region "Public Methods"

        public Retorno CriarTransacao(DadosPedido dadosPedido, Bandeira bandeira, Uri urlRetorno)
        {
            var dadosEc = new DadosEcAutenticacao { numero = Numero, chave = Chave };
            var formaPagamento = new FormaPagamento { bandeira = bandeira, parcelas = 1, produto = FormaPagamentoProduto.CreditoAVista };
            
            // Não funciona em QA segundo o próprio suporte da Cielo
            //var req = RequisicaoNovaTransacaoAutorizar.AutorizarAutenticadaENaoAutenticada;
            //var capturar = true;
            
            var req = RequisicaoNovaTransacaoAutorizar.AutorizarSemPassarPorAutenticacao;
            var capturar = false;


            return CriarTransacao(dadosPedido, dadosEc, formaPagamento, urlRetorno, req, capturar).Result;
        }

        public Task<Retorno> CriarTransacao(
            DadosPedido dadosPedido,
            DadosEcAutenticacao dadosEc,
            FormaPagamento formaPagamento,
            Uri urlRetorno,
            RequisicaoNovaTransacaoAutorizar reqAutorizar,
            bool capturar)
        {
            var msg = new RequisicaoNovaTransacao
            {
                id = dadosPedido.numero,
                versao = MensagemVersao.v110,
                dadosec = dadosEc,
                dadospedido = dadosPedido,
                formapagamento = formaPagamento,
                urlretorno = urlRetorno.AbsoluteUri,
                autorizar = reqAutorizar,
                capturar = capturar
            };

            return SetRetorno<RequisicaoNovaTransacao>(msg);
        }

        public Task<Retorno> ConsultarTransacao(string tid)
        {
            var dadosEc = new DadosEcAutenticacao { numero = Numero, chave = Chave };

            var msg = new RequisicaoConsulta
            {
                id = DateTime.Now.ToString(defaultDateFormat),
                versao = MensagemVersao.v110,
                tid = tid,
                dadosec = dadosEc
            };

            return SetRetorno<RequisicaoConsulta>(msg);
        }

        public Task<Retorno> AutorizarTransacao(string tid)
        {
            var dadosEc = new DadosEcAutenticacao { numero = Numero, chave = Chave };

            var msg = new RequisicaoAutorizacaoTid
            {
                id = DateTime.Now.ToString(defaultDateFormat),
                versao = MensagemVersao.v110,
                tid = tid,
                dadosec = dadosEc
            };

            return SetRetorno<RequisicaoAutorizacaoTid>(msg);
        }

        public Task<Retorno> CancelarTransacao(string tid)
        {
            var dadosEc = new DadosEcAutenticacao { numero = Numero, chave = Chave };

            var msg = new RequisicaoCancelamento
            {
                id = DateTime.Now.ToString(defaultDateFormat),
                versao = MensagemVersao.v110,
                tid = tid,
                dadosec = dadosEc
            };

            return SetRetorno<RequisicaoCancelamento>(msg);
        }

        public Retorno CapturarTransacao(string tid)
        {
            return CapturarTransacao(tid, -1, string.Empty).Result;
        }

        public Task<Retorno> CapturarTransacao(string tid, decimal valor, string anexo)
        {
            var dadosEc = new DadosEcAutenticacao { numero = Numero, chave = Chave };

            var msg = new RequisicaoCaptura
            {
                id = DateTime.Now.ToString(defaultDateFormat),
                versao = MensagemVersao.v110,
                tid = tid,
                dadosec = dadosEc
            };

            if (valor > -1)
                msg.valor = valor.ToFormatoCielo();

            if (!string.IsNullOrWhiteSpace(anexo))
                msg.anexo = anexo;

            return SetRetorno<RequisicaoCaptura>(msg);
        }

        #endregion

        #region "Private Methods"

        private async Task<Retorno> SetRetorno<T>(T msg)
        {
            try
            {
                var xml = msg.ToXml<T>(Encoding.GetEncoding("iso-8859-1"));
                var res = await EnviarMensagem(xml);
                return XmlToRetorno(res);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private Retorno XmlToRetorno(string xml)
        {
            var ret = new Retorno();

            if (!string.IsNullOrEmpty(xml))
            {
                RetornoTransacao transacao;
                RetornoErro erro;

                if (xml.Contains("</transacao>"))
                {
                    transacao = xml.ToType<RetornoTransacao>(Encoding.GetEncoding("iso-8859-1"));
                    ret.Transacao = transacao;
                    ret.Transacao.rawXml = xml;
                }
                else if (xml.Contains("</erro>"))
                {
                    erro = xml.ToType<RetornoErro>(Encoding.GetEncoding("iso-8859-1"));
                    ret.Erro = erro;
                }
            }

            return ret;
        }

        private async Task<string> EnviarMensagem(string xml)
        {
            var http = new EasyHttpClient("iso-8859-1", "application/x-www-form-urlencoded", "CieloClient");
            Task<string> res = http.PostAsync(Endpoint.AbsoluteUri, string.Format("mensagem={0}", xml));
            string r = await res;
            return r;
        }

        #endregion
    }
}
