using System.Windows.Forms;
using Teigha.Runtime;
#if ACAD
#endif
#if BCAD
#endif

namespace TestProject1
{
    public class Class1
    {
        [CommandMethod("MyCommand")]
        public void MyFirstCommand()
        {
            MessageBox.Show("Command succeeded!");
        }
    }
}
