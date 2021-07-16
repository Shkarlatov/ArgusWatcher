using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;

namespace ArgusWatcher
{
    static class Program
    {
        private static Mutex m_Mutex;
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool createdNew;
            m_Mutex = new Mutex(true, "ArgusWatcherMutex", out createdNew);
            if(createdNew)
            {
                var f = new Form1();
                Application.Run();
            }
            else
            {
                MessageBox.Show("Приложение уже запущено.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}
