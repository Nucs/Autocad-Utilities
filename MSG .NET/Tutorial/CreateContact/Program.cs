using System;
using Independentsoft.Msg;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Message contact = new Message();

            contact.MessageClass = "IPM.Contact";
            contact.Subject = "John Smith";
            contact.DisplayNamePrefix = "Mr.";
            contact.DisplayName = "John Smith";
            contact.GivenName = "John";
            contact.Surname = "Smith";
            contact.CompanyName = "Independentsoft";
            contact.Email1Address = "john@independentsoft.com";
            contact.Email1DisplayAs = "John";
            contact.Email1DisplayName = "John";
            contact.Email1Type = "SMTP";
            contact.BusinessAddressCity = "NY";
            contact.BusinessAddressStreet = "First Street";
            contact.BusinessAddressCountry = "USA";
            contact.BusinessAddress = "First Street, NY, USA";
            contact.BusinessPhone = "555-666-777";

            contact.Save("c:\\temp\\contact.msg", true);
        }
    }
}
