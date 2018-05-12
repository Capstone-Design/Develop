using System;
using System.Threading;
using System.Windows.Forms;

namespace SystrayConponent
{
    static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Mutex mutex = null;
            if(!Mutex.TryOpenExisting("MySystrayExtensionMutex",  out mutex))
            {
                mutex = new Mutex(false, "MySystrayExtensionMutex");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
                mutex.Close();
            }
            
        }
    }
}
