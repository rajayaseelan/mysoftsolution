using System.Collections;
using Castle.Core;
using Castle.Facilities.Startable;
using Castle.MicroKernel;
using Castle.MicroKernel.Facilities;
using System.Linq;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// The facility is used toautomatically discover services from castle config.
    /// </summary>
    public class ServiceDiscoverFacility : AbstractFacility
    {
        #region Private Members

        private ArrayList waitList;
        private IServiceContainer container;
        private IServiceCache cache;

        private bool CheckIfComponentImplementsIService(ComponentModel model)
        {
            bool markedWithServiceContract = false;
            var attr = CoreHelper.GetTypeAttribute<ServiceContractAttribute>(model.Services.First());
            if (attr != null)
            {
                markedWithServiceContract = true;
            }

            return markedWithServiceContract;
        }

        /// <summary>
        /// For each new component registered,
        /// some components in the WaitingDependency
        /// state may have became valid, so we check them
        /// </summary>
        private void CheckWaitingList()
        {
            IHandler[] handlerArray1 = (IHandler[])this.waitList.ToArray(typeof(IHandler));
            IHandler[] handlerArray2 = handlerArray1;
            for (int num1 = 0; num1 < handlerArray2.Length; num1++)
            {
                IHandler handler1 = handlerArray2[num1];
                if (handler1.CurrentState == HandlerState.Valid)
                {
                    this.RegisterService(handler1.ComponentModel);
                    this.waitList.Remove(handler1);
                }
            }
        }

        private void RegisterService(ComponentModel model)
        {
            DynamicService service = new DynamicService(container, cache, model.Services.First());
            container.RegisterComponent("Service_" + service.ServiceName, typeof(IService), service);
        }

        private void OnComponentModelCreated(ComponentModel model)
        {
            bool flag1 = this.CheckIfComponentImplementsIService(model);
            model.ExtendedProperties["service discover"] = flag1;
        }

        private void OnComponentRegistered(string key, IHandler handler)
        {
            if ((bool)handler.ComponentModel.ExtendedProperties["service discover"])
            {
                if (handler.CurrentState == HandlerState.WaitingDependency)
                {
                    this.waitList.Add(handler);
                }
                else
                {
                    this.RegisterService(handler.ComponentModel);
                }
            }
            this.CheckWaitingList();
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceDiscoverFacility"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="cache">The cache.</param>
        public ServiceDiscoverFacility(IServiceContainer container, IServiceCache cache)
        {
            this.waitList = new ArrayList();
            this.container = container;
            this.cache = cache;
        }

        /// <summary>
        /// Inits this instance.
        /// </summary>
        protected override void Init()
        {
            base.Kernel.ComponentModelCreated += new ComponentModelDelegate(this.OnComponentModelCreated);
            base.Kernel.ComponentRegistered += new ComponentDataDelegate(this.OnComponentRegistered);
        }
    }
}
