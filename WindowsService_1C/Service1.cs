using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceModel;
using System.ServiceProcess;
using V83;
using System.ServiceModel.Web;
using System.Web.Script.Serialization;

namespace WindowsService_1C
{
    [RunInstaller(true)]
    public partial class Installer1 : Installer
    {
        private ServiceProcessInstaller process;
        private ServiceInstaller service;
        public Installer1()
        {
            process = new ServiceProcessInstaller();
            service = new ServiceInstaller();
            process.Account = ServiceAccount.LocalSystem;

            service.ServiceName = "MyService";
            service.Description = "Convert data 1C to JSON";
            service.StartType = ServiceStartMode.Automatic;

            Installers.Add(process);
            Installers.Add(service);
        }
    }
    public partial class Service1 : ServiceBase
    {      
        ServiceHost service = null;

        public Service1()
        {
            ServiceName = "MyService";
        }
        public static void Main()
        {
            ServiceBase.Run(new Service1());
        }
        protected override void OnStart(string[] args)
        {
            if (service != null)
            {
                service.Close();
            }
            service = new ServiceHost(typeof(ConvertingToJSONService));
            service.Open();
        }

        protected override void OnStop()
        {
            if (service != null)
            {
                service.Close();
                service = null;
            }
        }
    }

    [ServiceContract]
    public interface IConvertingToJSON
    {
       [OperationContract(IsOneWay = false)]
       [WebGet(UriTemplate = "/GetEmployeeData")]
       string GetEmployeeData();
    }

    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.PerSession)]
    public class ConvertingToJSONService : IConvertingToJSON
    {        
        COMConnector connector = new COMConnector();
        dynamic comObject = null;
        public ConvertingToJSONService()
        {

        }

        private dynamic MakeConnection(string connString)
        {
            dynamic comObject = connector.Connect(connString);
               
            return comObject;
        }
  
        public string GetEmployeeData()
        {
            string connString = "srvr = 'srv1s8-n:1641'; ref= 'vents2011'; usr = 'Орел Денис'; pwd = '1450'";
            comObject = MakeConnection(connString);
            
            var listEmployee = (dynamic)comObject._ВЕНТСБиблиотекаСервер.СформироватьСписокАктуальныхСотрудников();
           
            string fio;
            string result = null;
            string fullResult = null;
            
            foreach (var item in listEmployee)
            {
                fio = item.СотрудникФИО().ToString();
                String[] words = fio.Split(' ');
                Employee emp = new Employee()
                {
                    user = item.АктуальностьСотрудника().ToString(),
                    userLastName = words[0].ToString(),
                    username = words[1].ToString(),
                    userMiddleName = words[2].ToString(),
                    department = item.СотрудникПодразделение().ToString(),
                    eMail = item.СотрудникЭлПочта().ToString(),
                    passID = item.IDПропуска().ToString(),
                    passNumber = item.НомерПропуска().ToString(),
                    passStatus = item.ТекущееСостояниеПропуска().ToString()
                   
                };
                result = new JavaScriptSerializer().Serialize(emp);

                fullResult = fullResult + result;
            }
            return fullResult;
        }
    }
   public class Employee
    {
        public string user;
        public string userLastName;
        public string username;
        public string userMiddleName;
        public string department;
        public string eMail;
        public string passID;
        public string passNumber;
        public string passStatus;
    }
}