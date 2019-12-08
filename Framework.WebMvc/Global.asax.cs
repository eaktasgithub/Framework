using AutoMapper;
using Framework.Business.Dependency.Ninject;
using Framework.Business.Mapping.AutoMapper;
using Framework.Core.CrossCuttingConcerns.Security;
using Framework.Core.Utilities.Mvc.Infrastructure;
using System;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
 

namespace Framework.WebMvc
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AutoMapperConfiguration.Configure();
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            ControllerBuilder.Current.SetControllerFactory(new NinjectControllerFactory(new NinjectBindModule()));
          
        }
        /*
         * AutoMapper  Asp.Net Mvc Konfig�rasyonu
         */
        public class AutoMapperConfiguration
        {
            public static void Configure()
            {
                Mapper.Initialize(config => config.AddProfile(new AutoMapperProfile()));
            }
        }
        /*
         * Burada Init medtodu ile PostAuthenticateRequest event'ini yakal�yoruz(Handle ediyoruz)
         * Gelen b�t�n requestlerde burada ilk AuthenticationTicket i�lemlerini yap�yor olaca��z
         */
        public override void Init()
        {
            PostAuthenticateRequest += MvcApplication_PostAuthenticateRequest;
        }
        /*
         * Burada cookie bilgilerine ula��p o cookie  bilgilerini kullan�caz
         */
        private void MvcApplication_PostAuthenticateRequest(object sender, EventArgs e)
        {
            try
            {
                /*
            *Cookie de�erini okuyoruz.
            */
                var cookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                if (cookie == null)
                {
                    return;
                }
                /*
                 * FormsAuthentication.Encrypt() �ifreleyip olu�turdu�umuz ticket'�n de�erini al�yoruz
                 */
                var encrypt = cookie.Value;
                if (String.IsNullOrEmpty(encrypt))
                {
                    return;
                }
                /*
                 * E�er cookie varsa ve  �ifreli data bo� de�ilse �ifreli ticket de�erini tekrardan ��z�ml�yoruz
                 */
                var ticket = FormsAuthentication.Decrypt(encrypt);
                /*
                 * ��z�mlerken kendi yazd���m�z TicketToIdentity metodunu kullanarak �ifreli veriyi ��z�p Identity nesnesine
                 * �eviriyoruz
                 */
                var identity = CookieToIdentityObjectCast.TicketToIdentity(ticket);
                /*
                 * Art�k bir  identity nesnemiz var ve  bunu principal'a ekliyoruz
                 */
                var princibal = new GenericPrincipal(identity, identity.Roles);

                HttpContext.Current.User = princibal;
                Thread.CurrentPrincipal = princibal;
            }
            catch
            {
            }

        }
    }
}
