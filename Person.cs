using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApp
{
    internal class Person
    {
        string name = null;
        string phoneNumber = null;
        public void setName(string tempname)
        {
            name = tempname;
        }
        public string getName()
        {
            return name;
        }
        public void setNumber(string tempnumber)
        {
            phoneNumber = tempnumber;
        }
        public string getPhoneNumber()
        {
            return phoneNumber;
        }
        public void Displayinfo()
        {
            Console.WriteLine("Name: " + name);
            Console.WriteLine("Phone Number: " + phoneNumber);
        }
    }
}
