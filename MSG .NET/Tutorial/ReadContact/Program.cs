using System;
using Independentsoft.Msg;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Message contact = new Message("c:\\temp\\contact.msg");

            Console.WriteLine("Subject: " + contact.Subject);
            Console.WriteLine("DisplayNamePrefix: " + contact.DisplayNamePrefix);
            Console.WriteLine("DisplayName: " + contact.DisplayName);
            Console.WriteLine("GivenName: " + contact.GivenName);
            Console.WriteLine("MiddleName: " + contact.MiddleName);
            Console.WriteLine("Surname: " + contact.Surname);
            Console.WriteLine("CompanyName: " + contact.CompanyName);
            Console.WriteLine("Title: " + contact.Title);
            Console.WriteLine("Email1DisplayName: " + contact.Email1DisplayName);
            Console.WriteLine("Email1DisplayAs: " + contact.Email1DisplayAs);
            Console.WriteLine("Email1Address: " + contact.Email1Address);
            Console.WriteLine("BusinessPhone: " + contact.BusinessPhone);
            Console.WriteLine("CellularPhone: " + contact.CellularPhone);
            Console.WriteLine("BusinessAddress: " + contact.BusinessAddress);
            Console.WriteLine("BusinessAddressStreet: " + contact.BusinessAddressStreet);
            Console.WriteLine("BusinessAddressPostalCode: " + contact.BusinessAddressPostalCode);
            Console.WriteLine("BusinessAddressCity: " + contact.BusinessAddressCity);
            Console.WriteLine("BusinessAddressState: " + contact.BusinessAddressState);
            Console.WriteLine("BusinessAddressCountry: " + contact.BusinessAddressCountry);

            Console.WriteLine("Press any key to exit.");
            Console.Read();
        }
    }
}
