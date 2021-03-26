using Unity;

namespace JT100.Wish.Core
{
    public class IOC
    {
        private IUnityContainer container;
        private readonly static object obj = new object();
        private static IOC ioc;
        private IOC()
        {
            container = new UnityContainer();
        }

        /// <summary>
        /// Ioc
        /// </summary>
        public static IOC Instance
        {
            get
            {
                if (ioc == null)
                {
                    lock (obj)
                    {
                        if (ioc == null)
                        {
                            ioc = new IOC();
                        }
                    }
                }
                return ioc;
            }
        }

        //private void 
    }
}
