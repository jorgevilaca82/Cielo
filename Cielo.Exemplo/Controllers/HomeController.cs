using System;
using System.Web.Mvc;
using Cielo.Messages;
using Cielo.Helpers;

namespace Cielo.Exemplo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var cielo = new CieloClient();

            var pedido = new DadosPedido("1254", 1.00M, "produto");
            //:57660
            var resposta = cielo.CriarTransacao(pedido, Bandeira.Visa, new Uri("http://localhost/Home/Retorno/1254"));

            if (!resposta.IsErro())
            {
                Session["tid"] = resposta.Transacao.tid;

                return Redirect(resposta.Transacao.urlautenticacao);
            }
            else
                return View();
        }

        public ActionResult Retorno(string id)
        {
            var cielo = new CieloClient();

            var resposta = cielo.ConsultarTransacao(Session["tid"].ToString()).Result;

            if (!resposta.IsErro())
            {
                resposta = cielo.CapturarTransacao(resposta.Transacao.tid, 0.5M, "capturanto metade do valor").Result;
            }

            return View();
        }

    }
}
