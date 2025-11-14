using ElectronicEducationEnviroment.Admin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ElectronicEducationEnviroment
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ElectronicEducationEnviromentEntities Context { get; set; } = new ElectronicEducationEnviromentEntities();

        public static bool isTeacher { get; set; }
        public static int IdUser { get; set; } 
    }
}
